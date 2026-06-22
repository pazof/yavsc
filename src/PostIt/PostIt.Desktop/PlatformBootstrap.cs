using IdentityModel.OidcClient.Browser;
using PostIt.Services;

namespace PostIt.Desktop;

/// <summary>
/// One-shot platform bootstrap. Called from <c>Program.Main</c> so that
/// the shared <c>LoginPageViewModel</c> sees a working <c>IBrowser</c>
/// (the loopback listener that captures the OIDC redirect) without
/// referencing any platform-specific API from the shared library.
/// </summary>
internal static class PlatformBootstrap
{
    private static int _initialized;

    internal static void EnsureInitialized()
    {
        if (System.Threading.Interlocked.Exchange(ref _initialized, 1) != 0)
            return;

        Platform.DefaultRedirectUri = Settings.DefaultLoopbackRedirectUri;

    }
}
