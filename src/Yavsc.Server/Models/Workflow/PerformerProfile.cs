#nullable enable annotations

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Models.Relationship;
    using Newtonsoft.Json;
    using Yavsc.Attributes.Validation;
    using Yavsc.Workflow;

    public class PerformerProfile : IPerformerProfile, IValidatableObject {

        [Key]
        public string PerformerId { get; set; }
        [ForeignKey("PerformerId")]
        public virtual ApplicationUser Performer { get; set; }

        [InverseProperty("User")]
        [Display(Name="Activity"), JsonIgnore]
        public virtual List<UserActivity> Activity { get; set; }

        [Required, Display(Name = "Country of exercise")]
        [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Country code must be a 2-letter code.")]
        public string ExerciseCountryCode { get; set; } = "fr";

        [Required,YaStringLength(14),Display(Name="SIREN")]
        public string SIREN { get; set; }

        public long OrganizationAddressId { get; set; }

        [Required,Display(Name="Organization address"),ForeignKey("OrganizationAddressId")]
        public virtual Location OrganizationAddress { get; set; }

        [Display(Name="Accept notifications on client query")]
        public bool AcceptNotifications { get; set; }

        [Display(Name="Accept notifications from non-VIP users")]
        public bool AcceptPublicContact { get; set; }

        [Display(Name="Use my geo-localization, and give me clients near by me")]
        public bool UseGeoLocalizationToReduceDistanceWithClients { get; set; }

        [Display(Name="Web site")]
        public string WebSite { get; set; }

        [Display(Name="Active")]
        public bool Active { get; set; }

        [Obsolete("Implement and use a new specialization setting")]
        [Display(Name="Maximal Daily Cost (euro/day)"),DisplayFormat(DataFormatString="{0:C}")]
        public int? MaxDailyCost { get; set; }

        [Obsolete("Implement and use a new specialization setting")]
        [Display(Name="Minimal Daily Cost (euro/day)"),DisplayFormat(DataFormatString="{0:C}")]
        public int? MinDailyCost { get; set; }

        [Display(Name="Rate from clients")]
        public int Rate { get; set; }

        [NotMapped]
        public bool DoesBlog { get {
            return Performer?.Posts?.Count > 0 ;
        } }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var countryCode = PerformerCodeInputValidationCatalog.NormalizeCountryCode(ExerciseCountryCode);

            if (string.IsNullOrWhiteSpace(countryCode))
            {
                yield return new ValidationResult(
                    "Le pays d'exercice est requis.",
                    new[] { nameof(ExerciseCountryCode) });
                yield break;
            }

            var rule = PerformerCodeInputValidationCatalog.GetRule(countryCode);
            if (rule is null)
            {
                yield return new ValidationResult(
                    "Le pays d'exercice doit etre l'un des suivants: fr, en, pt.",
                    new[] { nameof(ExerciseCountryCode) });
                yield break;
            }

            if (string.IsNullOrWhiteSpace(SIREN))
            {
                yield break;
            }

            var normalizedCode = SIREN.Trim();
            if (!Regex.IsMatch(normalizedCode, rule.RegularExpression, RegexOptions.CultureInvariant))
            {
                yield return new ValidationResult(
                    rule.ErrorMessage,
                    new[] { nameof(SIREN) });
            }
        }

    }
}
