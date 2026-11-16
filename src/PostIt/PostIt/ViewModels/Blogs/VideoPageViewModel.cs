using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

/// <summary>A chapter of a published replay, in the timeline of its MP4.</summary>
public sealed record PodcastChapterItem(long TimeMs, string Title)
{
    public string Label => $"{ReplayControlsState.FormatTime(TimeMs)}  {Title}";
}

public partial class VideoPageViewModel(Uri source, string mimeType, Uri? externalUri = null) : ViewModelBase
{
    public Uri Source { get; } = source;
    public Uri ExternalUri { get; } = externalUri ?? source;
    public string MimeType { get; } = mimeType;
    public PodcastApiClient? PodcastClient { get; init; }
    public PodcastResponse? Podcast { get; set; }

    public ObservableCollection<PodcastChapterItem> Chapters { get; } = new();
    public bool HasChapters => Chapters.Count > 0;
    /// <summary>Requests the player to show this time of the published replay.</summary>
    public event Action<long>? SeekRequested;

    /// <summary>Shows the chapters of the replay actually played; other sources have none.</summary>
    public void ShowChapters(IEnumerable<PodcastChapter> chapters)
    {
        Chapters.Clear();
        foreach (var chapter in chapters) Chapters.Add(new(chapter.TimeMs, chapter.Title));
        OnPropertyChanged(nameof(HasChapters));
    }

    [RelayCommand]
    private void GoToChapter(PodcastChapterItem? chapter)
    {
        if (chapter is not null) SeekRequested?.Invoke(chapter.TimeMs);
    }

    public bool CanOpenExternal => CanLaunchExternally(ExternalUri);
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
