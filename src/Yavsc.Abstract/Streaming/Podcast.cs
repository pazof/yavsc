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

/// <summary>
/// Owner-only state of a refined replay: the private source is being prepared, can be edited,
/// is being rendered for publication, or could not be prepared.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PodcastDraftState>))]
public enum PodcastDraftState { Preparing, Ready, Publishing, Failed }

/// <summary>A removed half-open range [StartMs, EndMs) of the original replay timeline.</summary>
public sealed record PodcastDraftRange(long StartMs, long EndMs);

/// <summary>
/// A named position of a replay. In a draft, it names a mark of the original timeline; in a published
/// replay, it is converted to the timeline of the published MP4.
/// </summary>
public sealed record PodcastChapter(long TimeMs, string Title);

/// <summary>
/// Private editing state of a refined replay, visible to its owner only. Marks and ranges use the
/// timeline of the original source. <c>Revision</c> detects concurrent edits.
/// </summary>
public sealed record PodcastDraftResponse(
    PodcastDraftState State, int Revision, IReadOnlyList<long> Marks,
    IReadOnlyList<PodcastDraftRange> RemovedRanges, long SourceBytes = 0, string? Error = null)
{
    /// <summary>Revision of the currently published MP4, if any.</summary>
    public int? PublishedRevision { get; init; }
    /// <summary>Named marks, published as chapters; each time is also one of the marks.</summary>
    public IReadOnlyList<PodcastChapter> Chapters { get; init; } = [];
}

/// <summary>Replaces the marks and removed ranges of a refined replay draft.</summary>
public sealed record SavePodcastDraftRequest
{
    /// <summary>Revision read by the client; a different current revision is a conflict.</summary>
    [Range(0, int.MaxValue)]
    public required int Revision { get; init; }
    [MaxLength(500)]
    public IReadOnlyList<long> Marks { get; init; } = [];
    [MaxLength(200)]
    public IReadOnlyList<PodcastDraftRange> RemovedRanges { get; init; } = [];
    /// <summary>Titles of named marks; their times are added to the marks if missing.</summary>
    [MaxLength(500)]
    public IReadOnlyList<PodcastChapter> Chapters { get; init; } = [];
}

/// <summary>Publishes the saved draft revision as the replay MP4.</summary>
public sealed record PublishPodcastDraftRequest
{
    [Range(0, int.MaxValue)]
    public required int Revision { get; init; }
}

/// <summary>Creates an authenticated live broadcast with a retained replay.</summary>
public sealed record CreatePodcastRequest
{
    [Required, StringLength(160, MinimumLength = 1)]
    public required string Title { get; init; }
    /// <summary>Keeps the replay private until its owner edits and publishes it.</summary>
    public bool RefineReplay { get; init; }
    /// <summary>Days the originals and draft of a refined replay are kept after the live; 0 deletes them on publication.</summary>
    [Range(0, PodcastRetention.MaxDays)]
    public int RetentionDays { get; init; } = PodcastRetention.DefaultDays;
}

/// <summary>Retention bounds of the originals and draft of a refined replay.</summary>
public static class PodcastRetention
{
    public const int DefaultDays = 7;
    public const int MaxDays = 365;
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
    /// <summary>Keeps the replay private until its owner edits and publishes it. Ignored for OBS.</summary>
    public bool RefineReplay { get; init; }
    /// <summary>Days the originals and draft of a refined replay are kept after the live; 0 deletes them on publication.</summary>
    [Range(0, PodcastRetention.MaxDays)]
    public int RetentionDays { get; init; } = PodcastRetention.DefaultDays;
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
    /// <summary>The replay is published only after its owner edits and validates it.</summary>
    public bool RefineReplay { get; init; }
    /// <summary>Private draft of a refined replay; present for its owner only.</summary>
    public PodcastDraftResponse? Draft { get; init; }
    /// <summary>Days the originals and draft of a refined replay are kept after the live.</summary>
    public int RetentionDays { get; init; } = PodcastRetention.DefaultDays;
    /// <summary>
    /// When the originals and draft of a refined replay will be deleted; an emission never published by then
    /// is deleted entirely.
    /// </summary>
    public DateTimeOffset? OriginalsExpireAt { get; init; }
    /// <summary>The originals and draft were deleted; only the published MP4 remains and it cannot be re-edited.</summary>
    public bool OriginalsDeleted { get; init; }
    /// <summary>Chapters of the published refined replay, in the timeline of its MP4.</summary>
    public IReadOnlyList<PodcastChapter> Chapters { get; init; } = [];
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
