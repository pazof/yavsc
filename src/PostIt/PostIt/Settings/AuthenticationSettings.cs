using CommunityToolkit.Mvvm.ComponentModel;
using System;

public partial class AuthenticationSettings : ObservableObject
{
    /// <summary>
    /// Default custom-scheme redirect URI on Desktop. The OS routes the
    /// callback to the running PostIt instance via the named-pipe
    /// hand-off in <see cref="PostIt.Services.SingleInstance"/>
    /// (RFC 8252 §7.1). Production Desktop builds use this.
    /// </summary>
    public const string DefaultDesktopRedirectUri = "postit://callback";

    /// <summary>
    /// Redirect URI used by the Android app. The corresponding IntentFilter
    /// in <c>PostIt.Android/Properties/AndroidManifest.xml</c> must match.
    /// </summary>
    public const string AndroidRedirectUri = "android://postit-signin";

    public static string DefaultAuthority { get; internal set; } = "https://yavsc.pschneider.fr";

    public static string DefaultClientId { get; internal set; } = "postit";
    [ObservableProperty]
    public partial string Authority { get; set; }

    [ObservableProperty]
    public partial string ClientId { get; set; }


    [ObservableProperty]
    public partial string[] Scopes { get; set; }


    /// <summary>
    /// OAuth redirect URI. Defaults to <see cref="DefaultDesktopRedirectUri"/>
    /// (custom URI scheme) which is the right answer for desktop
    /// production builds. Mobile platforms must set this to
    /// <see cref="AndroidRedirectUri"/> before calling <c>LoginAsync</c>.
    /// </summary>
    [ObservableProperty]
    public partial string RedirectUri { get; set; } = DefaultDesktopRedirectUri;

}
