#nullable enable annotations

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Workflow
{
    public enum StatutValidationTerme
    {
        Propose,
        Valide,
        Rejete
    }

    public class DictionnaireMetier
    {
        [Key]
        public long Id { get; set; }

        [Required, MaxLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Langue { get; set; } = "fr";

        [Required, MaxLength(128)]
        [ForeignKey(nameof(DomaineActivite))]
        public string DomaineActiviteCode { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual Activity? DomaineActivite { get; set; }

        [JsonIgnore]
        public virtual ICollection<TermeMetier> Termes { get; set; } = new List<TermeMetier>();
    }

    public class TermeMetier
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long DictionnaireMetierId { get; set; }

        [ForeignKey(nameof(DictionnaireMetierId))]
        [JsonIgnore]
        public virtual DictionnaireMetier? DictionnaireMetier { get; set; }

        [Required, MaxLength(200)]
        public string Mot { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Definition { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Langue { get; set; } = "fr";

        public StatutValidationTerme StatutValidation { get; set; } = StatutValidationTerme.Propose;

        [MaxLength(450)]
        public string? ProposeParId { get; set; }

        [ForeignKey(nameof(ProposeParId))]
        [JsonIgnore]
        public virtual ApplicationUser? ProposePar { get; set; }

        [MaxLength(450)]
        public string? ValideParId { get; set; }

        [ForeignKey(nameof(ValideParId))]
        [JsonIgnore]
        public virtual ApplicationUser? ValidePar { get; set; }

        public DateTime DateSoumission { get; set; } = DateTime.UtcNow;

        public DateTime? DateValidation { get; set; }
    }
}
