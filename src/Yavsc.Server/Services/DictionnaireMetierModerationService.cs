using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Workflow;

namespace Yavsc.Server.Services
{
    public class DictionnaireMetierModerationService
    {
        private readonly ApplicationDbContext _context;

        public DictionnaireMetierModerationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TermeMetier> ProposerTermAsync(
            long dictionnaireId,
            string mot,
            string definition,
            string langue,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(mot))
            {
                throw new ArgumentException("Le terme est requis.", nameof(mot));
            }

            if (string.IsNullOrWhiteSpace(definition))
            {
                throw new ArgumentException("La définition est requise.", nameof(definition));
            }

            var dictionary = await _context.DictionnaireMetier
                .SingleOrDefaultAsync(d => d.Id == dictionnaireId);

            if (dictionary is null)
            {
                throw new KeyNotFoundException($"Dictionnaire {dictionnaireId} introuvable.");
            }

            var normalizedMot = mot.Trim();
            var exists = await _context.TermeMetier
                .AnyAsync(t => t.DictionnaireMetierId == dictionnaireId
                    && t.Langue == langue
                    && t.Mot == normalizedMot);

            if (exists)
            {
                throw new InvalidOperationException($"Le terme '{normalizedMot}' existe déjà dans ce dictionnaire.");
            }

            var term = new TermeMetier
            {
                DictionnaireMetierId = dictionnaireId,
                DictionnaireMetier = dictionary,
                Mot = normalizedMot,
                Definition = definition.Trim(),
                Langue = string.IsNullOrWhiteSpace(langue) ? "fr" : langue,
                StatutValidation = StatutValidationTerme.Propose,
                ProposeParId = userId,
                ValideParId = null,
                DateSoumission = DateTime.UtcNow,
                DateValidation = null
            };

            _context.TermeMetier.Add(term);
            await _context.SaveChangesAsync();
            return term;
        }

        public async Task<TermeMetier> ValiderTermAsync(long termeId, string moderatorId)
        {
            var term = await _context.TermeMetier
                .SingleOrDefaultAsync(t => t.Id == termeId);

            if (term is null)
            {
                throw new KeyNotFoundException($"Terme {termeId} introuvable.");
            }

            term.StatutValidation = StatutValidationTerme.Valide;
            term.ValideParId = moderatorId;
            term.DateValidation = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return term;
        }

        public async Task<TermeMetier> RejeterTermAsync(long termeId, string moderatorId)
        {
            var term = await _context.TermeMetier
                .SingleOrDefaultAsync(t => t.Id == termeId);

            if (term is null)
            {
                throw new KeyNotFoundException($"Terme {termeId} introuvable.");
            }

            term.StatutValidation = StatutValidationTerme.Rejete;
            term.ValideParId = moderatorId;
            term.DateValidation = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return term;
        }

        public async Task<List<TermeMetier>> GetPendingAsync(long dictionnaireId)
        {
            return await _context.TermeMetier
                .Where(t => t.DictionnaireMetierId == dictionnaireId
                    && t.StatutValidation == StatutValidationTerme.Propose)
                .OrderBy(t => t.DateSoumission)
                .ToListAsync();
        }
    }
}
