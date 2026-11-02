#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Payment;

public enum PerformerDonationState { Created, AwaitingApproval, Processing, Pending, Completed, Cancelled, Failed }

public sealed class PerformerDonation
{
    public Guid Id { get; set; }
    [Required, MaxLength(450)]
    public string DonorId { get; set; } = "";
    [Required, MaxLength(450)]
    public string PerformerId { get; set; } = "";
    [Required, EmailAddress, MaxLength(127)]
    public string RecipientEmail { get; set; } = "";
    [Column(TypeName = "numeric(18,2)")]
    public decimal Amount { get; set; }
    [Required, MaxLength(3)]
    public string Currency { get; set; } = "EUR";
    public DateTimeOffset CreatedAt { get; set; }
    public PerformerDonationState State { get; set; }
    [MaxLength(100)]
    public string? CheckoutToken { get; set; }
    [MaxLength(32)]
    public string? TransactionId { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    [ConcurrencyCheck]
    public Guid Revision { get; set; } = Guid.NewGuid();
}
