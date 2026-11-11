using System.ComponentModel.DataAnnotations;

namespace Yavsc.Abstract.Resources;

/// <summary>Erases existing consumption points without granting future credit.</summary>
public sealed record EraseResourceUsageRequest
{
    [Range(typeof(decimal), "0.00000001", "99999999999999999999.99999999", ParseLimitsInInvariantCulture = true)]
    public decimal Points { get; init; }

    [Range(typeof(decimal), "0", "99999999999999999999.99999999", ParseLimitsInInvariantCulture = true)]
    public decimal ExpectedCredits { get; init; }
}
