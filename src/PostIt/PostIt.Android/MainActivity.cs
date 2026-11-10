
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Views;
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
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)
    ]
public class MainActivity : AvaloniaMainActivity
{
    /// <summary>
    /// The current MainActivity instance.
    /// </summary>
    public static MainActivity? Current { get; private set; }
    internal static bool IsForeground { get; private set; }

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
        Current = this;
        base.OnCreate(savedInstanceState);
        PostIt.Services.Platform.ScreenAwake.Changed += OnScreenAwakeChanged;
    }

    private void OnScreenAwakeChanged() => RunOnUiThread(UpdateScreenAwake);

    internal static void ShowSoftKeyboard()
    {
        if (Current is not { } activity || !IsForeground)
        {
            global::Android.Util.Log.Warn("PostIt", "Cannot show the keyboard without a foreground activity.");
            return;
        }

        activity.RunOnUiThread(() =>
        {
            if (activity.CurrentFocus is not { } view
                || activity.GetSystemService(InputMethodService) is not global::Android.Views.InputMethods.InputMethodManager inputMethod)
            {
                global::Android.Util.Log.Warn("PostIt", "Cannot show the keyboard without a focused native view and input method.");
                return;
            }

            if (!inputMethod.ShowSoftInput(view, global::Android.Views.InputMethods.ShowFlags.Implicit))
                global::Android.Util.Log.Warn("PostIt", "The input method did not accept the keyboard activation request.");
        });
    }

    private void UpdateScreenAwake()
    {
        if (Window is not { } window) return;
        if (ReferenceEquals(Current, this) && IsForeground && PostIt.Services.Platform.ScreenAwake.IsRequested)
            window.AddFlags(WindowManagerFlags.KeepScreenOn);
        else
            window.ClearFlags(WindowManagerFlags.KeepScreenOn);
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

    protected override void OnStop()
    {
        PostIt.Services.Platform.SuspendPlayback();
        base.OnStop();
    }

    protected override void OnResume()
    {
        base.OnResume();
        IsForeground = true;
        PostIt.Services.Platform.IsApplicationActive = true;
        UpdateScreenAwake();
    }

    protected override void OnPause()
    {
        IsForeground = false;
        PostIt.Services.Platform.IsApplicationActive = false;
        UpdateScreenAwake();
        base.OnPause();
    }

    protected override void OnDestroy()
    {
        PostIt.Services.Platform.ScreenAwake.Changed -= OnScreenAwakeChanged;
        Window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
        if (ReferenceEquals(Current, this))
        {
            Current = null;
            IsForeground = false;
            PostIt.Services.Platform.IsApplicationActive = false;
            PostIt.Android.Services.NotificationPermission.OnActivityDestroyed();
        }
        base.OnDestroy();
    }

    protected override void OnRestart()
    {
        base.OnRestart();
        PostIt.Services.Platform.ResumePlayback();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        if (PostIt.Android.Services.NotificationPermission.HandleResult(requestCode, permissions, grantResults)
            || PostIt.Android.Services.PodcastPermissions.HandleResult(requestCode, permissions, grantResults)
            || PostIt.Android.Services.AndroidCurrentLocationProvider
            .HandlePermissionResult(requestCode, grantResults))
        {
            return;
        }

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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
