// Yavsc.ViewModels.Kyc/ModerationDecisionModel.cs
namespace Yavsc.ViewModels.Kyc
{
    using System.ComponentModel.DataAnnotations;

    public class ModerationDecisionModel
    {
        [Required]
        public long DeclarationId { get; set; }

        /// <summary>
        /// Entre -10 et +10. Ignoré pour Reject.
        /// </summary>
        [Range(-10, 10)]
        public int ScoreDelta { get; set; }
    }
}