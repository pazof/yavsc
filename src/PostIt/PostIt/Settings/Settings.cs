using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using IdentityModel.OidcClient;
using PostIt.Services;
using System;
using System.IO;
using System.Text.Json;

[assembly: InternalsVisibleTo("PostIt.Tests")]

namespace PostIt;

public partial class Settings : ObservableObject
{
    const string SettingsFileName = "postit-settings.json";
    IStorageFolder? folder = null;

    /// <summary>
    /// Legacy loopback redirect URI. The post-2026.6 production flow
    /// uses the custom URI scheme (<see cref="DefaultDesktopRedirectUri"/>
    /// on desktop, <see cref="AndroidRedirectUri"/> on Android) so the
    /// OS hands the callback to the running instance without a TCP
    /// listener. The loopback constant stays here so test fixtures
    /// (which spin up an in-process OidcStubAuthority) keep working,
    /// but it is no longer used as a default anywhere in production.
    /// If you are still pointing your production <c>postit-settings.json</c>
    /// at this URI, switch to <c>postit://callback</c> and remove the
    /// matching entry from the Yavsc.Org server's allowed redirect URIs.
    /// </summary>
    public const string DefaultLoopbackRedirectUri = "http://127.0.0.1:7890/";

    /// <summary>
    /// Redirect URI used by the Android app. The corresponding IntentFilter
    /// in <c>PostIt.Android/Properties/AndroidManifest.xml</c> must match.
    /// </summary>
    public const string AndroidRedirectUri = "android://postit-signin";

    /// <summary>
    /// Default custom-scheme redirect URI on Desktop. The OS routes the
    /// callback to the running PostIt instance via the named-pipe
    /// hand-off in <see cref="PostIt.Services.SingleInstance"/>
    /// (RFC 8252 §7.1). Production Desktop builds use this.
    /// </summary>
    public const string DefaultDesktopRedirectUri = "postit://callback";

    [ObservableProperty]
    public partial AuthenticationSettings Authentication { get; set; } = new();

    [ObservableProperty]
    public partial bool DarkMode { get; set; } = false;

    [ObservableProperty]
    public partial string ApiUrl { get; set; } = "https://blogs.pschneider.fr/api/v1/";

    /// <summary>
    /// OAuth redirect URI. Defaults to <see cref="DefaultDesktopRedirectUri"/>
    /// (custom URI scheme) which is the right answer for desktop
    /// production builds. Mobile platforms must set this to
    /// <see cref="AndroidRedirectUri"/> before calling <c>LoginAsync</c>.
    /// </summary>
    [ObservableProperty]
    public partial string RedirectUri { get; set; } = DefaultDesktopRedirectUri;


    [ObservableProperty]
    public partial string[] Scopes { get; set; }
    public bool Loaded { get; private set; } = false;

    /// <summary>
    /// Build OidcClient options configured for Authorization Code + PKCE
    /// (no client secret). The browser implementation should be supplied
    /// per-platform by the caller.
    /// </summary>
    internal OidcClientOptions GetOidcClientOptions(IdentityModel.OidcClient.Browser.IBrowser? browser = null)
    {
        if (!Loaded) Load();
        var options = new OidcClientOptions
        {
            Authority = Authentication.Authority,
            ClientId = Authentication.ClientId,
            RedirectUri = RedirectUri,
            Scope = string.Join(' ', this.Scopes),
            TokenClientCredentialStyle = IdentityModel.Client.ClientCredentialStyle.PostBody
            // PKCE is enabled by default when no client_secret is provided.
        };

        if (browser is not null)
            options.Browser = browser;

        return options;
    }

    internal void Load()
    {
        if (Loaded) return;

        // Trust an already-populated Authority: tests pre-fill Settings
        // with the OIDC stub's random loopback port, and programmatic
        // callers (CLI flags, integration tests) wire their own. If we
        // fall through to the disk / embedded read here we'd silently
        // overwrite their value with the bundled default
        // (yavsc.pschneider.fr), break the stubbed discovery URL, and
        // turn a passing login into an invalid_grant.
        if (!string.IsNullOrWhiteSpace(Authentication?.Authority))
        {
            Loaded = true;
            return;
        }

        string configDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "PostIt"
);
        Directory.CreateDirectory(configDir);

        string configPath = Path.Combine(configDir, SettingsFileName);

        FileInfo configFileInfo = new FileInfo(configPath);

        if (!configFileInfo.Exists)
        {
            Console.Error.WriteLine($"🩎 Settings file not found at {configFileInfo.FullName}");
            // No user-level config: fall back to the embedded default.
            // We only get here when Authentication.Authority is empty
            // (the early-return above) so the redundant guard is gone.
            if (!TryLoadEmbeddedFallback())
            {
                Console.Error.WriteLine("🩎 No embedded default settings; running with empty configuration.");
            }
            return;
        }

        Console.WriteLine($"🔎 Loading settings from {configFileInfo.FullName}");

        try
        {
            // Synchronous read on purpose: Settings.Load() is called from
            // synchronous startup paths (App.axaml.cs, ViewModel ctors,
            // tests) and bridging to async here with .Wait() / .GetAwaiter()
            // .GetResult() deadlocks the Avalonia UI thread because the
            // continuation can't resume on the same thread. The settings
            // file is a few KiB at most; async I/O gains nothing here.
            using var stream = configFileInfo.OpenRead();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            ApplyJson(json, $"user file {configFileInfo.FullName}");
            Loaded = true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"🩎 Error loading settings: {ex.Message}");
        }
    }

    private bool TryLoadEmbeddedFallback()
    {
        const string ResourceName = "PostIt.postit-settings.json";
        var assembly = typeof(Settings).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            Console.Error.WriteLine($"🩎 Embedded resource {ResourceName} not found.");
            return false;
        }
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(json))
        {
            Console.Error.WriteLine("🩎 Embedded settings resource is empty.");
            return false;
        }
        Console.WriteLine($"🔎 Loading embedded default settings ({ResourceName}).");
        ApplyJson(json, $"embedded resource {ResourceName}");
        return true;
    }

    private void ApplyJson(string json, string source)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Console.Error.WriteLine($"🩎 Settings payload is empty (source: {source}).");
            return;
        }
        try
        {
            var settings = JsonSerializer.Deserialize<Settings>(json);
            if (settings is null)
            {
                Console.Error.WriteLine($"🩎 Settings payload is invalid (source: {source}).");
                return;
            }
            this.Authentication = settings.Authentication;
            this.DarkMode = settings.DarkMode;
            this.ApiUrl = settings.ApiUrl;
            this.RedirectUri = string.IsNullOrWhiteSpace(settings.RedirectUri) ? DefaultDesktopRedirectUri : settings.RedirectUri;
            this.Scopes = settings.Scopes;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"🩎 Error applying settings from {source}: {ex.Message}");
        }
    }
}
