using IdentityModel.OidcClient.Browser;

namespace PostIt.Services;

/// <summary>
/// Per-platform access to native integration points used by the OIDC
/// Authorization Code + PKCE flow. The shared <c>PostIt</c> library does
/// not reference any UI framework; platform projects (PostIt.Android,
/// PostIt.Desktop, PostIt.Browser) populate this class once at startup so
/// the shared <c>LoginPageViewModel</c> can drive a native browser without
/// taking a hard dependency on any specific UI toolkit.
/// </summary>
public static class Platform
{
    /// <summary>
    /// Default redirect URI for the running platform. The desktop loopback
    /// default is set here; platform projects override this property at
    /// startup (e.g. PostIt.Android sets it to <c>android://postit-signin</c>).
    /// </summary>
    public static string DefaultRedirectUri { get; set; } = "http://127.0.0.1:7890/";

    /// <summary>
    /// Constructs a fresh <see cref="IBrowser"/> for the running platform.
    /// May return <c>null</c> if no browser is wired up; in that case
    /// <c>LoginAsync</c> will surface a clear error.
    /// </summary>
    public static System.Func<IBrowser?>? CreateBrowser { get; set; }
}