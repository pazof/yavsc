// Yavsc.Models.Kyc/TrustToken.cs
namespace Yavsc.Models.Kyc
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Token opaque représentant une entité sans l'identifier.
    /// Généré soit par un tiers de confiance (référence externe),
    /// soit par hash d'un e-mail confirmé (fallback interne).
    /// Jamais réversible côté consultant.
    /// </summary>
    public class TrustToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Token fourni par le tiers de confiance, OU
        /// HMAC-SHA256(email_confirmé, clé_secrète_serveur).
        /// Stocké tel quel — pas l'email, pas l'identité.
        /// </summary>
        [Required, MaxLength(128)]
        public string TokenHash { get; set; }

        /// <summary>
        /// Source du token : "trusted_party" | "confirmed_email"
        /// </summary>
        [Required, MaxLength(32)]
        public string TokenSource { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Score de confiance agrégé, calculé par la modération.
        /// Null si jamais évalué.
        /// </summary>
        public int? TrustScore { get; set; }

        public virtual ICollection<TrustDeclaration> Declarations { get; set; }
    }
}