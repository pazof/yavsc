#nullable enable annotations

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Workflow
{
    /// <summary>
    /// Validation rule for performer business code input by country.
    /// </summary>
    public class PerformerCodeInputValidation
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(2)]
        [MinLength(2)]
        public string CountryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string RegularExpression { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string ErrorMessage { get; set; } = string.Empty;

        [ForeignKey(nameof(CountryCode))]
        public Country Country { get; set; }
    }

    public static class PerformerCodeInputValidationCatalog
    {
        public static readonly IReadOnlyList<Country> Countries = new List<Country>
        {
            new() { Code = "fr", DisplayName = "France" },
            new() { Code = "en", DisplayName = "England" },
            new() { Code = "pt", DisplayName = "Portugal" },
        };

        public static readonly IReadOnlyList<PerformerCodeInputValidation> Rules = new List<PerformerCodeInputValidation>
        {
            new()
            {
                Id = 1,
                CountryCode = "fr",
                RegularExpression = "^[0-9]{9,14}$",
                ErrorMessage = "Le code FR doit contenir entre 9 et 14 chiffres."
            },
            new()
            {
                Id = 2,
                CountryCode = "en",
                RegularExpression = "^[A-Za-z0-9]{8,14}$",
                ErrorMessage = "Le code EN doit contenir entre 8 et 14 caracteres alphanumeriques."
            },
            new()
            {
                Id = 3,
                CountryCode = "pt",
                RegularExpression = "^[0-9]{9}$",
                ErrorMessage = "Le code PT doit contenir exactement 9 chiffres."
            },
        };

        public static string NormalizeCountryCode(string? countryCode)
        {
            return (countryCode ?? string.Empty).Trim().ToLowerInvariant();
        }

        public static PerformerCodeInputValidation? GetRule(string? countryCode)
        {
            var normalized = NormalizeCountryCode(countryCode);
            foreach (var rule in Rules)
            {
                if (string.Equals(rule.CountryCode, normalized, StringComparison.Ordinal))
                {
                    return rule;
                }
            }

            return null;
        }
    }
}