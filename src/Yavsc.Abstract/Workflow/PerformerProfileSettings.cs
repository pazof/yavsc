#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Yavsc.Models.Workflow;

namespace Yavsc.Abstract.Workflow;

public sealed class PerformerProfileSettings : IValidatableObject
{
    public string PerformerId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ExerciseCountryCode { get; set; } = "fr";
    public string SIREN { get; set; } = string.Empty;
    public bool CanIssueEstimates => !string.IsNullOrWhiteSpace(SIREN);
    public string WebSite { get; set; } = string.Empty;
    /// <summary>Owner-only PayPal recipient. Null preserves the existing setting; empty disables donations.</summary>
    [MaxLength(127)]
    public string? DonationPayPalEmail { get; set; }
    public bool Active { get; set; }
    public bool AcceptNotifications { get; set; }
    public bool AcceptPublicContact { get; set; }
    public bool UseGeoLocalizationToReduceDistanceWithClients { get; set; }
    public List<string> SelectedActivityCodes { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var countryCode = PerformerCodeInputValidationCatalog.NormalizeCountryCode(
            string.IsNullOrWhiteSpace(ExerciseCountryCode) ? "fr" : ExerciseCountryCode);
        var rule = PerformerCodeInputValidationCatalog.GetRule(countryCode);
        if (rule is null)
        {
            yield return new ValidationResult(
                "Le pays d'exercice doit etre l'un des suivants: fr, en, pt.",
                new[] { nameof(ExerciseCountryCode) });
            yield break;
        }

        if (!string.IsNullOrWhiteSpace(SIREN)
            && !Regex.IsMatch(SIREN.Trim(), rule.RegularExpression, RegexOptions.CultureInvariant))
        {
            yield return new ValidationResult(rule.ErrorMessage, new[] { nameof(SIREN) });
        }
    }
}
