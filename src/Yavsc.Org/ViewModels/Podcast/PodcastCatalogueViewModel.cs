#nullable enable

using Yavsc.Abstract.Streaming;

namespace Yavsc.ViewModels.Podcast;

public sealed record PodcastCatalogueViewModel(
    IReadOnlyList<PodcastResponse> Podcasts,
    IReadOnlySet<string> DonationPerformerIds,
    string? Error = null);
