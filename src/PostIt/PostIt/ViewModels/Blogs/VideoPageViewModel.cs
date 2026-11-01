using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class VideoPageViewModel(Uri source, string mimeType) : ViewModelBase
{
    public Uri Source { get; } = source;
    public string MimeType { get; } = mimeType;
    public PodcastApiClient? PodcastClient { get; init; }
    public PodcastResponse? Podcast { get; set; }

    public bool CanOpenExternal => CanLaunchExternally(Source);
    public string Title => Podcast?.Title ?? Uri.UnescapeDataString(System.IO.Path.GetFileName(Source.AbsolutePath));
    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => true; protected set { } }

    private static bool CanLaunchExternally(Uri source)
        => source is { IsAbsoluteUri: true }
            && (string.Equals(source.Scheme, "http", StringComparison.OrdinalIgnoreCase)
                || string.Equals(source.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            && !string.Equals(source.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source.AbsolutePath, "/", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source.Scheme, "about", StringComparison.OrdinalIgnoreCase);
}
