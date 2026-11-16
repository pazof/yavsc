using Yavsc.Abstract.Streaming;

namespace Yavsc.Services;

/// <summary>
/// After the live of a refined emission, its original segments and draft are reserved to its owner
/// until the edited MP4 is published; the published MP4 is the only listener-visible replay.
/// </summary>
public static class PodcastVisibility
{
    public static bool OriginalsArePrivate(PodcastResponse podcast)
        => podcast.RefineReplay && podcast.State != PodcastState.Live;

    public static PodcastResponse ForViewer(PodcastResponse podcast, string? viewerId)
        => podcast.OwnerId == viewerId ? podcast : podcast with { Draft = null };

    public static PodcastManifestResponse ForViewer(PodcastManifestResponse manifest, string? viewerId)
        => manifest.Podcast.OwnerId == viewerId ? manifest : manifest with
        {
            Podcast = ForViewer(manifest.Podcast, viewerId),
            Segments = OriginalsArePrivate(manifest.Podcast) ? [] : manifest.Segments
        };

    public static void EnsureOriginalsVisible(PodcastResponse podcast, string? viewerId)
    {
        if (OriginalsArePrivate(podcast) && podcast.OwnerId != viewerId)
            throw new PodcastException(403, "Les originaux de cette emission sont reserves a son auteur.");
    }
}
