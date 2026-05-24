// Yavsc.Services.Kyc/ModerationPipelineService.cs
namespace Yavsc.Services.Kyc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Yavsc.Models;
    using Yavsc.Models.Kyc;
    using Yavsc.ViewModels.Kyc;

    public interface IModerationPipelineService
    {
        /// <summary>
        /// Soumet une déclaration : applique les regex, place en queue.
        /// Retourne la déclaration créée (avec ses flags éventuels).
        /// </summary>
        Task<TrustDeclaration> SubmitDeclarationAsync(
            Guid subjectTokenId,
            Guid declarantTokenId,
            string content,
            DeclarationSentiment sentiment);

        /// <summary>
        /// Le modérateur approuve : applique le ScoreDelta sur le TrustToken.
        /// </summary>
        Task ApproveAsync(long declarationId, int scoreDelta, string moderatorId);

        /// <summary>
        /// Le modérateur rejette : aucun effet sur le score.
        /// </summary>
        Task RejectAsync(long declarationId, string moderatorId);

        /// <summary>
        /// Le modérateur expurge : contenu effacé, score partiellement appliqué.
        /// </summary>
        Task RedactAsync(long declarationId, int scoreDelta, string moderatorId);

        /// <summary>
        /// Retourne la queue de modération (pending + flagged en priorité).
        /// </summary>
        Task<List<ModerationQueueItem>> GetQueueAsync();
    }

    public class ModerationPipelineService : IModerationPipelineService
    {
        private readonly ApplicationDbContext _db;

        // Cache des patterns actifs — rechargé à chaque soumission
        // (en prod : IMemoryCache avec invalidation)
        private List<RegexAlertPattern> _patterns;

        public ModerationPipelineService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ── Soumission ───────────────────────────────────────────────────────

        public async Task<TrustDeclaration> SubmitDeclarationAsync(
            Guid subjectTokenId,
            Guid declarantTokenId,
            string content,
            DeclarationSentiment sentiment)
        {
            // Vérification basique
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Contenu vide", nameof(content));

            if (subjectTokenId == declarantTokenId)
                throw new InvalidOperationException("Auto-déclaration interdite");

            // Chargement des patterns actifs
            _patterns = await _db.RegexAlertPatterns
                .Where(p => p.IsActive)
                .ToListAsync();

            // Analyse regex
            var flags = RunRegexAnalysis(content);
            var hasBloking = flags.Any(f =>
                _patterns.First(p => p.Id == f.PatternId).Severity == PatternSeverity.Blocking);

            var declaration = new TrustDeclaration
            {
                TrustTokenId    = subjectTokenId,
                DeclarantTokenId = declarantTokenId,
                Content         = content,
                Sentiment       = sentiment,
                SubmittedAt     = DateTime.UtcNow,
                // Flagged = priorité haute en queue, sinon Pending
                Status          = flags.Any() ? ModerationStatus.Flagged
                                              : ModerationStatus.Pending,
                Flags           = flags
            };

            _db.TrustDeclarations.Add(declaration);
            await _db.SaveChangesAsync();
            return declaration;
        }

        // ── Décisions de modération ──────────────────────────────────────────

        public async Task ApproveAsync(long declarationId, int scoreDelta, string moderatorId)
        {
            var (declaration, token) = await LoadForModerationAsync(declarationId);

            ValidateScoreDelta(scoreDelta);

            declaration.Status     = ModerationStatus.Approved;
            declaration.ScoreDelta = scoreDelta;

            ApplyDelta(token, scoreDelta);

            await _db.ModerationLogs.AddAsync(new ModerationLog
            {
                DeclarationId = declarationId,
                ModeratorId   = moderatorId,
                Action        = ModerationAction.Approved,
                ScoreDelta    = scoreDelta,
                Timestamp     = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        public async Task RejectAsync(long declarationId, string moderatorId)
        {
            var (declaration, _) = await LoadForModerationAsync(declarationId);

            declaration.Status = ModerationStatus.Rejected;
            // Aucun effet sur le score

            await _db.ModerationLogs.AddAsync(new ModerationLog
            {
                DeclarationId = declarationId,
                ModeratorId   = moderatorId,
                Action        = ModerationAction.Rejected,
                ScoreDelta    = 0,
                Timestamp     = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        public async Task RedactAsync(long declarationId, int scoreDelta, string moderatorId)
        {
            var (declaration, token) = await LoadForModerationAsync(declarationId);

            ValidateScoreDelta(scoreDelta);

            // Contenu expurgé — on garde la trace de la déclaration, pas du texte
            declaration.Content    = "[expurgé]";
            declaration.Status     = ModerationStatus.Redacted;
            declaration.ScoreDelta = scoreDelta;

            ApplyDelta(token, scoreDelta);

            await _db.ModerationLogs.AddAsync(new ModerationLog
            {
                DeclarationId = declarationId,
                ModeratorId   = moderatorId,
                Action        = ModerationAction.Redacted,
                ScoreDelta    = scoreDelta,
                Timestamp     = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        // ── Queue de modération ──────────────────────────────────────────────

        public async Task<List<ModerationQueueItem>> GetQueueAsync()
        {
            return await _db.TrustDeclarations
                .Where(d => d.Status == ModerationStatus.Pending
                         || d.Status == ModerationStatus.Flagged)
                .OrderByDescending(d => d.Status == ModerationStatus.Flagged) // Flagged en tête
                .ThenBy(d => d.SubmittedAt)
                .Select(d => new ModerationQueueItem
                {
                    DeclarationId      = d.Id,
                    // Pseudonyme tronqué — jamais le hash complet exposé
                    SubjectTokenHash   = d.Subject.TokenHash.Substring(0, 8) + "…",
                    Content            = d.Content,
                    Sentiment          = d.Sentiment,
                    Status             = d.Status,
                    HasBlockingFlag  = d.Flags.Any(f =>
                        f.Pattern.Severity == PatternSeverity.Blocking),
                    FlagDescriptions   = d.Flags
                        .Select(f => f.Pattern.Description)
                        .ToList()
                })
                .ToListAsync();
        }

        // ── Privé ────────────────────────────────────────────────────────────

        private List<DeclarationFlag> RunRegexAnalysis(string content)
        {
            var flags = new List<DeclarationFlag>();

            foreach (var pattern in _patterns)
            {
                try
                {
                    var match = Regex.Match(content, pattern.Pattern,
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                        TimeSpan.FromMilliseconds(200)); // timeout anti-ReDoS

                    if (!match.Success) continue;

                    // Extrait tronqué — jamais le contenu complet
                    var excerpt = match.Value.Length > 80
                        ? match.Value.Substring(0, 80) + "…"
                        : match.Value;

                    flags.Add(new DeclarationFlag
                    {
                        PatternId    = pattern.Id,
                        MatchExcerpt = excerpt
                    });
                }
                catch (RegexMatchTimeoutException)
                {
                    // Pattern trop lent — on logue et on continue
                    // TODO: alerter l'admin sur ce pattern
                }
            }

            return flags;
        }

        private async Task<(TrustDeclaration, TrustToken)> LoadForModerationAsync(long declarationId)
        {
            var declaration = await _db.TrustDeclarations
                .Include(d => d.Subject)
                .FirstOrDefaultAsync(d => d.Id == declarationId)
                ?? throw new KeyNotFoundException($"Déclaration {declarationId} introuvable");

            if (declaration.Status == ModerationStatus.Approved
             || declaration.Status == ModerationStatus.Rejected
             || declaration.Status == ModerationStatus.Redacted)
                throw new InvalidOperationException("Déclaration déjà traitée");

            return (declaration, declaration.Subject);
        }

        private static void ApplyDelta(TrustToken token, int delta)
        {
            var current = token.TrustScore ?? 50; // Score initial neutre : 50
            // Bornes 0–100
            token.TrustScore = Math.Clamp(current + delta, 0, 100);
        }

        private static void ValidateScoreDelta(int delta)
        {
            if (delta < -10 || delta > 10)
                throw new ArgumentOutOfRangeException(nameof(delta),
                    "ScoreDelta doit être entre -10 et +10");
        }
    }
}