// Yavsc.Services.Kyc/ITrustTokenService.cs
namespace Yavsc.Services.Kyc
{
    using System.Threading.Tasks;
    using Yavsc.Models.Kyc;

    public interface ITrustTokenService
    {
        /// <summary>
        /// Depuis un tiers de confiance : token opaque déjà haché côté tiers.
        /// On le stocke tel quel.
        /// </summary>
        Task<TrustToken> GetOrCreateFromTrustedPartyAsync(string externalToken);

        /// <summary>
        /// Fallback : depuis un e-mail confirmé.
        /// On calcule HMAC-SHA256(email, clé_secrète) — jamais l'email stocké.
        /// </summary>
        Task<TrustToken> GetOrCreateFromEmailAsync(string confirmedEmail);
    }
}
