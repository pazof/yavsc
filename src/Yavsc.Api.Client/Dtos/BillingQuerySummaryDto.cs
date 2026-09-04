using System;
using Yavsc;

namespace Yavsc.Api.Client;

/// <summary>
/// Lightweight billing-query projection consumed by PostIt list views.
/// Extra JSON fields from concrete query types are ignored.
/// </summary>
public sealed class BillingQuerySummaryDto
{
    public long Id { get; set; }
    public string BillingCode { get; set; } = string.Empty;
    public string ActivityCode { get; set; } = string.Empty;
    public string PerformerId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public QueryStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = string.Empty;
    public decimal? Provisional { get; set; }
}