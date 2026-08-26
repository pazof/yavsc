
using Android.App;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.Provider;
using AndroidX.Emoji2.Text;
using Avalonia.Android;
using PostIt.Droid.Services;

namespace PostIt.Android;

[Activity(
    Name = "PostIt.Android.PostItMainActivity",
    Label = "PostIt.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    /// <summary>
    /// The current MainActivity instance.
    /// </summary>
    public static MainActivity? Current { get; private set; }

    protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState)
    {
        FontRequest fontRequest = new FontRequest(
                "com.google.android.gms.fonts",
                "com.google.android.gms",
                "Noto Color Emoji Compat",
                Yavsc.Resource.Array.com_google_android_gms_fonts_certs); //com_google_android_gms_fonts_certs
        EmojiCompat.Config config = new FontRequestEmojiCompatConfig(this, fontRequest);
        EmojiCompat.Init(config);
        PlatformBootstrap.InitPlatform();
        base.OnCreate(savedInstanceState);
        Current = this;
    }
     /// <summary>
    /// Receives the deep-link Intent fired by the system browser after the
    /// user completes the OIDC login on https://yavsc.pschneider.fr. The
    /// Intent URI has the shape <c>android://postit-signin?code=...&amp;state=...</c>.
    ///
    /// IdentityModel.OidcClient.Browser.SystemBrowser is set up to await this
    /// callback via a TaskCompletionSource; expose the received Intent here
    /// through a static sink so the browser can resolve the pending login.
    /// </summary>
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

         var url = intent?.DataString;
        if (!string.IsNullOrEmpty(url) && url.StartsWith("postit://callback"))
        {
            OidcCallbackManager.SetResult(url);
        }

    }

    internal static class AndroidOidcCallbackSink
    {
        private static System.Threading.Tasks.TaskCompletionSource<string>? _pending;

        public static System.Threading.Tasks.Task<string> AwaitNextCallbackAsync()
        {
            _pending = new System.Threading.Tasks.TaskCompletionSource<string>(
                System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);
            return _pending.Task;
        }

        public static void Handle(Intent intent)
        {
            var tcs = System.Threading.Interlocked.Exchange(ref _pending, null);
            tcs?.TrySetResult(intent?.Data?.ToString() ?? string.Empty);
        }
    }
}
