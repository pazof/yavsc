using Yavsc.Abstract.Streaming;

namespace PostIt.Services;

public enum PodcastPlaybackSource { Originals, PublishedReplay, Pending, Unavailable }

public readonly record struct PodcastPlaybackChoice(PodcastPlaybackSource Source, string? Message = null);

/// <summary>Chooses what a viewer can play: the published MP4, the HLS originals, or nothing yet.</summary>
public static class PodcastPlayback
{
    public static PodcastPlaybackChoice Choose(PodcastResponse podcast)
    {
        if (podcast.State != PodcastState.Live && podcast.Replay is { State: PodcastReplayState.Ready, Bytes: > 0 })
            return new(PodcastPlaybackSource.PublishedReplay);
        if (podcast.OriginalsDeleted)
            return new(PodcastPlaybackSource.Unavailable, "Les originaux de cette emission ont ete supprimes.");
        if (podcast.RefineReplay && podcast.State != PodcastState.Live)
            // Only the owner receives the draft; other viewers wait for the publication.
            return podcast.Draft is null
                ? new(PodcastPlaybackSource.Unavailable, "Le replay est en cours de finalisation par son auteur.")
                : new(PodcastPlaybackSource.Originals);
        if (podcast.State == PodcastState.Completed)
            return podcast.Replay is { State: PodcastReplayState.Failed } failed
                ? new(PodcastPlaybackSource.Unavailable, failed.Error ?? "Preparation du MP4 en echec.")
                : new(PodcastPlaybackSource.Pending, "Preparation du replay MP4...");
        return new(PodcastPlaybackSource.Originals);
    }
}
