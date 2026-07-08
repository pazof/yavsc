using IdentityModel.OidcClient.Browser;
using PostIt.Services;

namespace PostIt.Desktop;

/// <summary>
/// One-shot platform bootstrap. Called from <c>Program.Main</c> so that
/// the shared OIDC login path sees a working <c>IBrowser</c> — the
/// custom-scheme browser that hands the OIDC callback off to the
/// running instance through the named pipe. Desktop builds do NOT use
/// a loopback HTTP listener: the <c>postit://</c> scheme is registered
/// with the OS at install time and the browser is whatever the user
/// has configured to open it.
/// </summary>
internal static class PlatformBootstrap
{
    private static int _initialized;

    internal static void EnsureInitialized()
    {
        if (System.Threading.Interlocked.Exchange(ref _initialized, 1) != 0)
            return;

        // Use the custom-scheme redirect on Desktop. Loopback is only
        // a fallback for platforms that cannot register postit://
        // (see Settings.DefaultLoopbackRedirectUri for that path).
        Platform.DefaultRedirectUri = AuthenticationSettings.DefaultDesktopRedirectUri;
        Platform.CustomScheme = "postit";
    }
}
