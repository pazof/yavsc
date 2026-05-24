// Yavsc.ViewModels.Kyc/DeclarationInputModel.cs
namespace Yavsc.ViewModels.Kyc
{
    using System.ComponentModel.DataAnnotations;
    using Yavsc.Models.Kyc;

    public class DeclarationInputModel
    {
        /// <summary>
        /// Email du sujet — transformé en token immédiatement, jamais persisté.
        /// Obligatoire si ExternalToken absent.
        /// </summary>
        [EmailAddress]
        public string SubjectEmail { get; set; }

        /// <summary>
        /// Token fourni par un tiers de confiance.
        /// Prioritaire sur SubjectEmail si présent.
        /// </summary>
        public string ExternalToken { get; set; }

        [Required, MinLength(10), MaxLength(2000)]
        public string Content { get; set; }

        [Required]
        public DeclarationSentiment Sentiment { get; set; }

        // Validation croisée : l'un ou l'autre obligatoire
        public bool IsValid => !string.IsNullOrEmpty(SubjectEmail)
                            || !string.IsNullOrEmpty(ExternalToken);
    }
}