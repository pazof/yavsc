using System;
using System.Collections.Generic;

namespace Yavsc.Api.Client;

/// <summary>
/// A single billable line of an estimate, mirroring the JSON shape of
/// the server-side <c>Yavsc.Models.Billing.CommandLine</c> entity.
/// </summary>
public sealed class EstimateLineDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
    public decimal UnitaryCost { get; set; }
    public long EstimateId { get; set; }
    public string Currency { get; set; } = "EUR";
}

/// <summary>
/// Estimate payload exchanged with the <c>api/v1/estimate</c> routes
/// (<c>EstimateApiController</c>). <see cref="AttachedGraphics"/> and
/// <see cref="AttachedFiles"/> are always initialised: the server-side
/// entity reads them from non-nullable string properties and a null
/// list would break its serialisation.
/// </summary>
public sealed class EstimateDto
{
    public long Id { get; set; }
    public long? CommandId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<EstimateLineDto> Bill { get; set; } = new();
    public List<string> AttachedGraphics { get; set; } = new();
    public List<string> AttachedFiles { get; set; } = new();
    public string? OwnerId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public DateTime ProviderValidationDate { get; set; }
    public DateTime ClientValidationDate { get; set; }

    /// <summary>
    /// Signatures already captured against this estimate (Pro and/or
    /// Client), as returned with the estimate payload. Each carries the
    /// PostIt wire-format <see cref="EstimateSignatureDto.Strokes"/>
    /// so the pad can repaint a previously drawn signature on reopen
    /// without a second round-trip. Null/empty when nobody has signed.
    /// </summary>
    public List<EstimateSignatureDto>? Signatures { get; set; }
}

/// <summary>
/// One stored signature on an estimate, mirroring the JSON shape of
/// the server-side <c>Yavsc.Models.Billing.Signature</c> entity. The
/// <see cref="Strokes"/> are the same PostIt wire-format payload that
/// was captured and are loaded back into the pad unchanged.
/// </summary>
public sealed class EstimateSignatureDto
{
    public long Id { get; set; }
    public long EstimateId { get; set; }
    /// <summary>
    /// Who signed, as the integer value of the server
    /// <c>SignatureType</c> enum: <c>0</c> = Pro (provider),
    /// <c>1</c> = Client. Matched by value on the client so PostIt
    /// need not reference the server enum.
    /// </summary>
    public int Type { get; set; }
    public int CoordinateMax { get; set; }
    public DateTime CapturedAtUtc { get; set; }
    public int[]? Strokes { get; set; }
}

/// <summary>
/// Response of a successful estimate creation
/// (<c>Ok(new { estimate.Id, estimate.Bill })</c>).
/// </summary>
public sealed class EstimateCreatedDto
{
    public long Id { get; set; }
    public List<EstimateLineDto> Bill { get; set; } = new();
}
