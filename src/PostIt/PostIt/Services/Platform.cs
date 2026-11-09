using System;
using IdentityModel.OidcClient.Browser;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using LibVLCSharp.Shared;

namespace PostIt.Services;

/// <summary>
/// Native integration points populated by platform projects at startup.
/// </summary>
public static class Platform
{
    public static Action? ShowSoftKeyboard { get; set; }
    public static ScreenAwakeRequests ScreenAwake { get; } = new();
    private static bool _isApplicationActive = !OperatingSystem.IsAndroid();
    public static event Action? ApplicationActivityChanged;
    public static bool IsApplicationActive
    {
        get => Volatile.Read(ref _isApplicationActive);
        set
        {
            if (value == _isApplicationActive) return;
            Volatile.Write(ref _isApplicationActive, value);
            ApplicationActivityChanged?.Invoke();
        }
    }
    public static IBackgroundChatService? BackgroundChatService { get; set; }
    public static Func<IServiceProvider>? CreateServiceProvider { get; set; }
    public static Func<PodcastCaptureDevices>? GetLanCallDevices { get; set; }
    public static Func<string, int, ILanCallMedia>? CreateLanCallMedia { get; set; }
    public static Func<PodcastCaptureDevices>? GetPodcastCaptureDevices { get; set; }
    public static Func<string?, CancellationToken, Task>? RequestPodcastPermissionsAsync { get; set; }
    public static Func<string, string?, string, CancellationToken, Task<IPodcastCapture>>? CreatePodcastCaptureAsync { get; set; }
    public static Func<string, CancellationToken, Task<IPodcastCapture>>? CreatePodcastPreviewAsync { get; set; }
    public static Func<string, string?, string, CancellationToken, Task<IPodcastCapture>>? CreateBlurredPodcastCaptureAsync { get; set; }
    public static Func<string, CancellationToken, Task<IPodcastCapture>>? CreateBlurredPodcastPreviewAsync { get; set; }

    /// <summary>Platform-specific video surface inside the shared video page.</summary>
    public static Func<MediaPlayer, bool, Control>? CreateVideoSurface { get; set; }

    public static event Action? PlaybackSuspending;
    public static event Action? PlaybackResuming;
    public static void SuspendPlayback() => PlaybackSuspending?.Invoke();
    public static void ResumePlayback() => PlaybackResuming?.Invoke();

    /// <summary>
    /// Default redirect URI for the running platform. The shared
    /// default uses a custom URI scheme (<c>postit://callback</c>)
    /// so the OAuth2 redirect is delivered to the running instance
    /// through the OS scheme handler — no loopback HTTP listener
    /// required (see RFC 8252 §7.1). Platform projects may still
    /// override this property at startup (e.g. PostIt.Android sets
    /// it to <c>android://postit-signin</c>).
    /// </summary>
    public const string RedirectUri = "postit://callback";

    /// <summary>
    /// Scheme prefix the <see cref="CustomSchemeBrowser"/> matches
    /// against <c>BrowserOptions.EndUrl</c>. Overridable for apps
    /// that want to register their own scheme.
    /// </summary>
    public const string CustomScheme = "postit";

    /// <summary>
    /// Constructs a fresh <see cref="IBrowser"/> for the running platform.
    /// May return <c>null</c> if no browser is wired up; in that case
    /// <c>LoginAsync</c> will surface a clear error.
    /// </summary>
    public static System.Func<IBrowser?>? CreateBrowser { get; set; } =
        () => null;

    /// <summary>
    /// Optional platform hook used by the shared billing form to request a
    /// current device position. Platforms that do not expose a native
    /// location provider can leave the default delegate in place.
    /// </summary>
    public static Func<CancellationToken, Task<CurrentLocationResult>> TryGetCurrentLocationAsync { get; set; } =
        _ => Task.FromResult(CurrentLocationResult.Unavailable());

    public static bool IsPodcastSupported { get; set; } = false;
}
