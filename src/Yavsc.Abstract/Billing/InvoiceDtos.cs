#nullable enable

using System.ComponentModel.DataAnnotations;

namespace Yavsc.Billing;

/// <summary>A frozen invoice line copied from the signed estimate.</summary>
public sealed record InvoiceLineResponse(string Name, string Description, int Count, decimal UnitaryCost);

/// <summary>An invoice and its cumulative settlement balance.</summary>
public sealed record InvoiceResponse(
    long CommandId, long EstimateId, string Reference, string ClientId, string PerformerId,
    DateTimeOffset CreatedAtUtc, string Currency, decimal AmountDue, decimal AmountPaid,
    decimal AmountRemaining, string PaymentStatus, bool IsAcquitted,
    IReadOnlyList<InvoiceLineResponse> Lines, IReadOnlyList<InvoicePaymentResponse> Payments);

/// <summary>An immutable recorded receipt, not an instruction to debit the client.</summary>
public sealed record InvoicePaymentResponse(
    long Id, string Source, string Reference, decimal Amount, string Currency,
    DateTimeOffset ReceivedAtUtc, DateTimeOffset RecordedAtUtc, string RecordedBy);

/// <summary>A receipt attested by the invoice's performer.</summary>
public sealed record RecordInvoicePaymentRequest
{
    [Required, MaxLength(128)]
    public required string Reference { get; init; }
    public required decimal Amount { get; init; }
    [Required, StringLength(3, MinimumLength = 3)]
    public required string Currency { get; init; }
    public required DateTimeOffset ReceivedAtUtc { get; init; }
}

/// <summary>A PayPal transaction identifier to verify on the server; no client amount is trusted.</summary>
public sealed record VerifyInvoicePayPalRequest
{
    [Required, MaxLength(128)]
    public required string TransactionId { get; init; }
}
