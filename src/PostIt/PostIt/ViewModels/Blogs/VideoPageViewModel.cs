using System;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public sealed class VideoPageViewModel(Uri source, string mimeType) : ViewModelBase
{
    public Uri Source { get; } = source;
    public string MimeType { get; } = mimeType;
    public PodcastApiClient? PodcastClient { get; init; }
    public PodcastResponse? Podcast { get; init; }
    public bool CanOpenExternal => Podcast is null;
    public string Title => Podcast?.Title ?? Uri.UnescapeDataString(System.IO.Path.GetFileName(Source.AbsolutePath));
    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => true; protected set { } }
}
