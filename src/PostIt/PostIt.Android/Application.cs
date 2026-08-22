using Android.App;

namespace PostIt.Android;

/// <summary>
/// Bare <see cref="Android.App.Application"/> shell for Android. Avalonia
/// 11 initialises its platform services from <see cref="MainActivity"/>
/// (which extends <c>AvaloniaMainActivity&lt;App&gt;</c>); no per-Application
/// Avalonia setup is needed here. Kept only so that the [Application] entry
/// stays present in the merged manifest, which the Android runtime expects
/// when the manifest declares a custom android:name in the application tag.
/// </summary>
[Application]
public class Application : Android.App.Application
{
    public Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }
}