using System;

namespace PostIt.ViewModels;

public sealed class VideoPageViewModel(Uri source, string mimeType) : ViewModelBase
{
    public Uri Source { get; } = source;
    public string MimeType { get; } = mimeType;
    public string Title => Uri.UnescapeDataString(System.IO.Path.GetFileName(Source.AbsolutePath));
    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => true; protected set { } }
}
