using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// Space-separated view of <see cref="Scopes"/>. Exists for the
    /// <c>SettingsPage</c> TextBox binding — a <c>string[]</c> does not
    /// round-trip through XAML binding to <c>TextBox.Text</c>, so we
    /// expose the array as a string here and re-parse on assignment.
    /// <para>
    /// <c>[JsonIgnore]</c> on purpose: <see cref="Scopes"/> is the
    /// persisted shape (matches the on-disk format in
    /// <c>postit-settings.json</c> and the runtime contract in
    /// <see cref="PostIt.ViewModels.Settings.GetOidcClientOptions"/>).
    /// Writing this property back to disk would duplicate the
    /// information and confuse the deserializer.
    /// </para>
    /// </summary>
    [JsonIgnore]
    [ObservableProperty]
    public partial string ScopeListText { get; set; } = string.Empty;

    /// <summary>
    /// Refresh <see cref="ScopeListText"/> from <see cref="Scopes"/> so
    /// the TextBox shows the current persisted state after a Load().
    /// Called from <c>Settings.ApplyJson</c> on each disk / embedded
    /// hydration; the source generator's <c>OnScopesChanged</c> partial
    /// below keeps the two in sync in the other direction (edits made
    /// in the TextBox).
    /// </summary>
    public void RefreshScopeListText()
    {
        ScopeListText = Scopes is null ? string.Empty : string.Join(' ', Scopes);
    }

    partial void OnScopeListTextChanged(string value)
    {
        if (Scopes is null)
        {
            Scopes = Array.Empty<string>();
        }
        // Split on any whitespace, drop empties. Matches what
        // string.Join(' ', Scopes) produces when Scopes is null-free,
        // so a round-trip (Display → Edit → Display) is lossless
        // for sane inputs.
        var parts = value?.Split(
            new[] { ' ', '\t', '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        // Skip the write if the parsed array is equal to the current
        // one — avoids a PropertyChanged loop between OnScopesChanged
        // and OnScopeListTextChanged when RefreshScopeListText runs.
        if (Scopes is not null && Scopes.Length == parts.Length)
        {
            var same = true;
            for (var i = 0; i < parts.Length; i++)
            {
                if (!string.Equals(Scopes[i], parts[i], StringComparison.Ordinal))
                {
                    same = false;
                    break;
                }
            }
            if (same) return;
        }
        Scopes = parts;
    }

    partial void OnScopesChanged(string[] value)
    {
        // Keep ScopeListText in sync when Scopes is reassigned from
        // outside (JSON hydration, MergeScopes, programmatic
        // updates). Compute the new value and only fire if it
        // differs from what's already shown, otherwise the TextBox
        // would briefly flicker / re-set the caret on every load.
        var newText = value is null ? string.Empty : string.Join(' ', value);
        if (!string.Equals(ScopeListText, newText, StringComparison.Ordinal))
        {
            ScopeListText = newText;
        }
    }

}
