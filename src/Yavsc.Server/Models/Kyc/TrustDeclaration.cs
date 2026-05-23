// Yavsc.Models.Kyc/TrustDeclaration.cs
namespace Yavsc.Models.Kyc
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public enum DeclarationSentiment { Positive, Neutral, Negative }

    public class TrustDeclaration
    {
        [Key]
        public long Id { get; set; }

        // Vers qui porte la déclaration (pseudonyme)
        [Required]
        public Guid TrustTokenId { get; set; }
        public virtual TrustToken Subject { get; set; }

        // Qui déclare — pseudonymisé aussi
        [Required]
        public Guid DeclarantTokenId { get; set; }

        /// <summary>
        /// Texte libre soumis par le déclarant.
        /// Passé à la modération + regex avant tout effet sur le score.
        /// </summary>
        [MaxLength(2000)]
        public string Content { get; set; }

        public DeclarationSentiment Sentiment { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // État de modération
        public ModerationStatus Status { get; set; } = ModerationStatus.Pending;

        /// <summary>
        /// Score d'impact sur le TrustToken, attribué par la modération.
        /// Entre -10 et +10. Null = pas encore traité.
        /// </summary>
        public int? ScoreDelta { get; set; }

        // Flags regex levés automatiquement avant modération humaine
        public virtual ICollection<DeclarationFlag> Flags { get; set; }
    }

    public enum ModerationStatus
    {
        Pending,      // en attente
        Flagged,      // regex alarmante détectée, modération prioritaire
        Approved,     // validé, ScoreDelta appliqué
        Rejected,     // rejeté, aucun effet sur le score
        Redacted      // accepté mais contenu expurgé
    }
}