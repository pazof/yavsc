#nullable enable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Yavsc.Models.Payment;

namespace Yavsc.ViewModels.Podcast;

public sealed class CreateDonationViewModel
{
    [Required]
    public string PerformerId { get; set; } = "";
    [BindNever]
    public string PerformerName { get; set; } = "";
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; } = 5m;
}

public sealed class DonationSettingsViewModel
{
    [EmailAddress, StringLength(127)]
    public string? PayPalEmail { get; set; }
}

public sealed record DonationDetailsViewModel(PerformerDonation Donation, string PerformerName);
