// Yavsc.Models.Kyc/RegexAlertPattern.cs
namespace Yavsc.Models.Kyc
{
    using System.ComponentModel.DataAnnotations;

    public enum PatternSeverity { Warning, Blocking }

    /// <summary>
    /// Expression régulière configurée par l'admin.
    /// Appliquée automatiquement à chaque déclaration soumise.
    /// </summary>
    public class RegexAlertPattern
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Pattern { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public PatternSeverity Severity { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DeclarationFlag
    {
        [Key]
        public long Id { get; set; }

        public long DeclarationId { get; set; }
        public virtual TrustDeclaration Declaration { get; set; }

        public int PatternId { get; set; }
        public virtual RegexAlertPattern Pattern { get; set; }

        // Extrait du texte qui a matché (tronqué, jamais le texte complet)
        [MaxLength(100)]
        public string MatchExcerpt { get; set; }
    }
}