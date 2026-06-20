using PostIt.Services;
using PostIt.Android.Services;

namespace PostIt.Android;

/// <summary>
/// One-shot platform bootstrap. Called from
/// <see cref="MainActivity.OnCreate"/> so that the shared
/// <c>LoginPageViewModel</c> sees the Android-specific redirect URI and a
/// working <c>IBrowser</c> (Chrome Custom Tabs) without referencing
/// Android APIs from the shared library.
/// </summary>
internal static class PlatformBootstrap
{
    private static int _initialized;

    internal static void EnsureInitialized()
    {
        if (System.Threading.Interlocked.Exchange(ref _initialized, 1) != 0)
            return;

        Platform.DefaultRedirectUri = Settings.AndroidRedirectUri;
        Platform.CreateBrowser = () =>
        {
            var activity = MainActivity.Current;
            return activity is null ? null : new AndroidSystemBrowser(activity);
        };
    }
}