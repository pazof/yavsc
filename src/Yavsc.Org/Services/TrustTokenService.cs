// Yavsc.Services.Kyc/TrustTokenService.cs
namespace Yavsc.Services.Kyc
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Yavsc.Models;
    using Yavsc.Models.Kyc;

    public class TrustTokenService : ITrustTokenService
    {
        private readonly ApplicationDbContext _db;
        private readonly byte[] _hmacKey;

        public TrustTokenService(ApplicationDbContext db, IOptions<KycOptions> options)
        {
            _db = db;
            // La clé est chargée depuis la config, jamais hardcodée
            _hmacKey = Encoding.UTF8.GetBytes(options.Value.HmacSecret
                ?? throw new InvalidOperationException("KycOptions:HmacSecret manquant"));
        }

        public async Task<TrustToken> GetOrCreateFromTrustedPartyAsync(string externalToken)
        {
            if (string.IsNullOrWhiteSpace(externalToken))
                throw new ArgumentException("Token externe invalide", nameof(externalToken));

            // Le token tiers arrive déjà pseudonymisé — on le stocke tel quel
            return await GetOrCreateAsync(externalToken, "trusted_party");
        }

        public async Task<TrustToken> GetOrCreateFromEmailAsync(string confirmedEmail)
        {
            if (string.IsNullOrWhiteSpace(confirmedEmail))
                throw new ArgumentException("Email invalide", nameof(confirmedEmail));

            // HMAC-SHA256(email normalisé, clé_secrète) → pseudonyme stable, irréversible
            var tokenHash = ComputeHmac(confirmedEmail.Trim().ToLowerInvariant());
            return await GetOrCreateAsync(tokenHash, "confirmed_email");
        }

        // ── Privé ────────────────────────────────────────────────────────────

        private async Task<TrustToken> GetOrCreateAsync(string tokenHash, string source)
        {
            // Idempotent : même entité → même token
            var existing = await _db.TrustTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (existing != null)
                return existing;

            var token = new TrustToken
            {
                TokenHash  = tokenHash,
                TokenSource = source,
                CreatedAt  = DateTime.UtcNow
            };

            _db.TrustTokens.Add(token);
            await _db.SaveChangesAsync();
            return token;
        }

        private string ComputeHmac(string input)
        {
            using var hmac = new HMACSHA256(_hmacKey);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).ToLowerInvariant(); // 64 chars, stockable
        }
    }
}