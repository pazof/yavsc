// Yavsc.Services.Kyc/KycOptions.cs
namespace Yavsc.Services.Kyc
{
    public class KycOptions
    {
        /// <summary>
        /// Clé secrète serveur pour le HMAC — jamais en clair dans le code,
        /// à mettre dans les secrets (user-secrets / env var / vault).
        /// </summary>
        public string HmacSecret { get; set; }
    }
}
