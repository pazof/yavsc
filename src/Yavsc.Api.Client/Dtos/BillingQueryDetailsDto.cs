using System;
using System.Collections.Generic;
using Yavsc;

namespace Yavsc.Api.Client;

/// <summary>
/// Normalized billing-query shape used by PostIt when opening an existing
/// command from history.
/// </summary>
public sealed class BillingQueryDetailsDto
{
    public long Id { get; set; }
    public string BillingCode { get; set; } = string.Empty;
    public string ActivityCode { get; set; } = string.Empty;
    public string PerformerId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Consent { get; set; } = true;
    public DateTime? EventDate { get; set; }
    public QueryStatus Status { get; set; } = QueryStatus.Inserted;
    public string Reason { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = string.Empty;
    public decimal? Provisional { get; set; }
    public BillingLocationDto? Location { get; set; }
    public long? PrestationId { get; set; }
    public List<long> PrestationIds { get; set; } = new();
}

public sealed class BillingLocationDto
{
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}