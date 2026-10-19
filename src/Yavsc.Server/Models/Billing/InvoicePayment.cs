#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Yavsc.Models.Billing;

public sealed class InvoicePayment
{
    public long Id { get; set; }
    public long CommandId { get; set; }
    [ForeignKey(nameof(CommandId)), JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public NominativeServiceCommand Command { get; set; } = null!;
    [MaxLength(16)]
    public string Source { get; set; } = "";
    [MaxLength(450)]
    public string ReferenceScope { get; set; } = "";
    [MaxLength(128)]
    public string Reference { get; set; } = "";
    public decimal Amount { get; set; }
    [MaxLength(3)]
    public string Currency { get; set; } = "";
    public DateTimeOffset ReceivedAtUtc { get; set; }
    public DateTimeOffset RecordedAtUtc { get; set; }
    [MaxLength(450)]
    public string RecordedBy { get; set; } = "";
}
