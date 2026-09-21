using System;

namespace Yavsc.Api.Client;

/// <summary>
/// Body of <c>POST api/v1/front/query/accept</c> and
/// <c>.../query/reject</c>, mirroring the server-side
/// <c>Yavsc.ViewModels.FrontOffice.QueryAcceptanceRequest</c>.
/// <see cref="Strokes"/> is optional: when non-empty on accept it is
/// the PostIt wire-format signature captured with the acceptance; null
/// or empty means no signature (the only valid value on reject).
/// </summary>
public sealed class QueryAcceptanceRequestDto
{
    public int[]? Strokes { get; set; }

    public int CoordinateMax { get; set; } = 10_000;

    public DateTime? CapturedAtUtc { get; set; }
}

/// <summary>
/// Response of <c>POST .../query/accept</c> / <c>.../query/reject</c>
/// (<c>Ok(new { queryId, status, signature = ... })</c>). The
/// <see cref="Signature"/> ref is non-null only when a signature was
/// staged on accept.
/// </summary>
public sealed class QueryAcceptanceResponseDto
{
    public long QueryId { get; set; }

    public string? Status { get; set; }

    public QuerySignatureRefDto? Signature { get; set; }
}

public sealed class QuerySignatureRefDto
{
    public long Id { get; set; }

    public long EstimateId { get; set; }

    public string? Type { get; set; }
}