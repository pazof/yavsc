#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Payment;

public sealed class ResourceSubscription
{
    public Guid Id { get; set; }
    [MaxLength(450)]
    public string UserId { get; set; } = "";
    [MaxLength(32)]
    public string Pack { get; set; } = "";
    [MaxLength(64)]
    public string PlanId { get; set; } = "";
    [MaxLength(64)]
    public string? PayPalId { get; set; }
    [MaxLength(32)]
    public string State { get; set; } = "CREATING";
    public decimal MonthlyPoints { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal PriceExcludingTax { get; set; }
    public decimal VatPercent { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal PriceIncludingTax { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? NextBillingAt { get; set; }
    public int LastCreditedCycle { get; set; } = -1;
}

public sealed class ResourceSubscriptionPayment
{
    [MaxLength(64)]
    public string Id { get; set; } = "";
    public Guid SubscriptionId { get; set; }
    public int Cycle { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public decimal GrantedPoints { get; set; }
    public bool Reversed { get; set; }
}
