using Android.App;
using Android.Content.PM;
using Android.Content;
using Avalonia;
using Avalonia.Android;

namespace PostIt.Android;

[Activity(
    Name = "PostIt.Android.PostItMainActivity",
    Label = "PostIt.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
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
        if (intent is not null) AndroidOidcCallbackSink.Handle(intent);
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