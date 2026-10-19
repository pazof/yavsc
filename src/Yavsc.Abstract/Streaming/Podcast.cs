using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Yavsc.Abstract.Streaming;

/// <summary>The lifecycle of a retained live broadcast.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PodcastState>))]
public enum PodcastState { Live, Completed, Interrupted }

/// <summary>Creates an authenticated live broadcast with a retained replay.</summary>
public sealed record CreatePodcastRequest
{
    [Required, StringLength(160, MinimumLength = 1)]
    public required string Title { get; init; }
}

/// <summary>A broadcast available to authenticated listeners.</summary>
public sealed record PodcastResponse(
    Guid Id, string Title, string OwnerId, string OwnerName,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, PodcastState State,
    int SegmentCount, long Bytes, double DurationSeconds);

/// <summary>An independently decodable MPEG-TS HLS segment.</summary>
public sealed record PodcastSegmentResponse(int Sequence, double DurationSeconds, int Bytes);

/// <summary>The ordered, committed segments of a live broadcast or replay.</summary>
public sealed record PodcastManifestResponse(
    PodcastResponse Podcast, IReadOnlyList<PodcastSegmentResponse> Segments);
