using System.Runtime.CompilerServices;
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
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("PostIt.Tests")]

namespace PostIt;

public partial class Settings : ObservableObject
{
    const string SettingsFileName = "postit-settings.json";
    IStorageFolder? folder = null;

    /// <summary>
    /// Default loopback redirect URI used for interactive PKCE login on desktop
    /// platforms. The corresponding <c>RedirectUri</c> must be registered for
    /// the PostIt client in IdentityServer.
    /// </summary>
    public const string DefaultLoopbackRedirectUri = "http://127.0.0.1:7890/";

    /// <summary>
    /// Redirect URI used by the Android app. The corresponding IntentFilter
    /// in <c>PostIt.Android/Properties/AndroidManifest.xml</c> must match.
    /// </summary>
    public const string AndroidRedirectUri = "android://postit-signin";

    [ObservableProperty]
    public partial AuthenticationSettings Authentication { get; set; } = new();

    [ObservableProperty]
    public partial bool DarkMode { get; set; } = false;

    [ObservableProperty]
    public partial string ApiUrl { get; set; } = "https://blogs.pschneider.fr/api/v1/";

    /// <summary>
    /// OAuth redirect URI. Defaults to a loopback URI suitable for desktop
    /// apps; mobile platforms must set this to <see cref="AndroidRedirectUri"/>
    /// before calling <c>LoginAsync</c>.
    /// </summary>
    [ObservableProperty]
    public partial string RedirectUri { get; set; } = DefaultLoopbackRedirectUri;


    [ObservableProperty]
    public partial string[] Scopes { get; set; }

    /// <summary>
    /// Build OidcClient options configured for Authorization Code + PKCE
    /// (no client secret). The browser implementation should be supplied
    /// per-platform by the caller.
    /// </summary>
    internal OidcClientOptions GetOidcClientOptions(IdentityModel.OidcClient.Browser.IBrowser? browser = null)
    {
        var options = new OidcClientOptions
        {
            Authority = Authentication.Authority,
            ClientId = Authentication.ClientId,
            RedirectUri = RedirectUri,
            Scope = string.Join(' ', this.Scopes),
            // PKCE is enabled by default when no client_secret is provided.
        };

        if (browser is not null)
            options.Browser = browser;

        return options;
    }

    internal async Task Load()
    {
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
            // Only fall back to the embedded default when the in-memory
            // settings haven't been populated yet. This protects callers
            // (notably tests) that pre-load Settings with explicit values
            // from being silently overwritten by the bundled default.
            if (string.IsNullOrWhiteSpace(this.Authentication?.Authority)
                && !TryLoadEmbeddedFallback())
            {
                Console.Error.WriteLine("🩎 No embedded default settings; running with empty configuration.");
            }
            return; // no user settings file
        }

        Console.WriteLine($"🔎 Loading settings from {configFileInfo.FullName}");

        try
        {
            using var stream = configFileInfo.OpenRead();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            ApplyJson(json, $"user file {configFileInfo.FullName}");
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
            this.RedirectUri = string.IsNullOrWhiteSpace(settings.RedirectUri) ? DefaultLoopbackRedirectUri : settings.RedirectUri;
            this.Scopes = settings.Scopes;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"🩎 Error applying settings from {source}: {ex.Message}");
        }
    }
}