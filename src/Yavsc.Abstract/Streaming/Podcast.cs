using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Yavsc.Abstract.Streaming;

/// <summary>The lifecycle of a retained live broadcast.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PodcastState>))]
public enum PodcastState { Live, Completed, Interrupted, Planned }

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

/// <summary>Prepares or edits an emission before its manual start.</summary>
public sealed record PreparePodcastRequest
{
    [Required, StringLength(160, MinimumLength = 1)]
    public required string Title { get; init; }
    public required DateTimeOffset ScheduledStartAt { get; init; }
    [Range(1, int.MaxValue)]
    public required int PlannedDurationMinutes { get; init; }
    [StringLength(1023)]
    public string? PublicAnnouncement { get; init; }
}

/// <summary>Public presentation only; never includes playback addresses or private metadata.</summary>
public sealed record PodcastAnnouncementResponse(
    Guid Id, string Title, string OwnerName, DateTimeOffset ScheduledStartAt,
    int PlannedDurationMinutes, string Announcement);

/// <summary>A broadcast available to authenticated listeners.</summary>
public sealed record PodcastResponse(
    Guid Id, string Title, string OwnerId, string OwnerName,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, PodcastState State,
    int SegmentCount, long Bytes, double DurationSeconds)
{
    public long? FlowId { get; init; }
    public DateTimeOffset? ScheduledStartAt { get; init; }
    public int? PlannedDurationMinutes { get; init; }
    public string? PublicAnnouncement { get; init; }
    public PodcastReplayResponse? Replay { get; init; }
}

/// <summary>An independently decodable MPEG-TS HLS segment.</summary>
public sealed record PodcastSegmentResponse(int Sequence, double DurationSeconds, int Bytes);

/// <summary>The ordered, committed segments of a live broadcast or replay.</summary>
public sealed record PodcastManifestResponse(
    PodcastResponse Podcast, IReadOnlyList<PodcastSegmentResponse> Segments);

/// <summary>One-time OBS connection credentials for an activated planned emission.</summary>
public sealed record PodcastObsCredentials(string ServerUrl, string StreamKey, DateTimeOffset ExpiresAt);

/// <summary>Owner-only state of the external broadcasting activation.</summary>
public sealed record PodcastObsStatus(bool Enabled, DateTimeOffset? ExpiresAt, string? Error);

/// <summary>MediaMTX external authentication request; credentials are never logged or echoed.</summary>
public sealed record PodcastRelayAuthentication
{
    public string User { get; init; } = "";
    public string Password { get; init; } = "";
    public string Action { get; init; } = "";
    public string Path { get; init; } = "";
    public string Protocol { get; init; } = "";
    public string Query { get; init; } = "";
    public string Id { get; init; } = "";
}

/// <summary>Maximum lifetime of an authenticated media relay session.</summary>
public sealed record PodcastRelayAuthorization(DateTimeOffset? ExpireAt);
