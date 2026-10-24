using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Yavsc.Abstract.Streaming;

/// <summary>The lifecycle of a retained live broadcast.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PodcastState>))]
public enum PodcastState { Live, Completed, Interrupted }

/// <summary>Preparation state of the seekable MP4 copy of a completed broadcast.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PodcastReplayState>))]
public enum PodcastReplayState { Pending, Processing, Ready, Failed }

/// <summary>Persistent MP4 preparation result; source segments are retained.</summary>
public sealed record PodcastReplayResponse(PodcastReplayState State, long Bytes = 0, string? Error = null);

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
    int SegmentCount, long Bytes, double DurationSeconds)
{
    public PodcastReplayResponse? Replay { get; init; }
}

/// <summary>An independently decodable MPEG-TS HLS segment.</summary>
public sealed record PodcastSegmentResponse(int Sequence, double DurationSeconds, int Bytes);

/// <summary>The ordered, committed segments of a live broadcast or replay.</summary>
public sealed record PodcastManifestResponse(
    PodcastResponse Podcast, IReadOnlyList<PodcastSegmentResponse> Segments);
