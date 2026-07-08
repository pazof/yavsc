using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdentityModel.OidcClient;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

[assembly: InternalsVisibleTo("PostIt.Tests")]

namespace PostIt.ViewModels;

public partial class Settings : ViewModelBase
{
    const string SettingsFileName = "postit-settings.json";

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
    /// Process-wide canonical <see cref="Settings"/> instance, wired up
    /// at application boot by <see cref="App.OnFrameworkInitializationCompleted"/>
    /// through <see cref="BindToServiceProvider"/>. The hybrid pattern:
    /// <list type="bullet">
    ///   <item><description>The static <c>Current</c> reference gives
    ///   ViewModels a non-DI way to reach the same instance (and lets
    ///   the framework bindings push notifications through one stable
    ///   <see cref="ObservableObject"/>).</description></item>
    ///   <item><description>Tests that want to exercise a clean
    ///   instance still call <c>new Settings()</c>; <c>Current</c>
    ///   stays null in those contexts because <see cref="BindToServiceProvider"/>
    ///   is never invoked.</description></item>
    ///   <item><description>Reads (<see cref="GetCurrent"/>) are
    ///   thread-safe and never allocate; mutations always go through
    ///   the DI-resolved singleton so two threads cannot each register
    ///   a different "current" Settings.</description></item>
    /// </list>
    /// </summary>
    private static Settings? s_current;

    /// <summary>
    /// Wire the canonical Settings instance to a DI container. Called
    /// exactly once from <c>App.axaml.cs</c> after the singleton has
    /// been registered. Subsequent calls are no-ops: the DI container
    /// owns the instance lifetime and we don't want a stray
    /// <c>BindToServiceProvider</c> in a test fixture to silently
    /// rebind the production instance.
    /// </summary>
    public static void BindToServiceProvider(IServiceProvider services)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        Interlocked.CompareExchange(ref s_current,
            services.GetService<Settings>() ?? throw new InvalidOperationException(
                "Settings is not registered in the DI container."),
            null);
    }

    /// <summary>
    /// Returns the canonical Settings instance previously bound through
    /// <see cref="BindToServiceProvider"/>, or <c>null</c> when called
    /// outside a running Avalonia application (tests, CLI tools).
    /// </summary>
    public static Settings? GetCurrent() => Volatile.Read(ref s_current);

    /// <summary>
    /// Resolve the canonical Settings instance or throw. Use this in
    /// production code paths that must not silently fall back to a
    /// freshly-constructed <see cref="Settings"/> (which used to be
    /// the root cause of the postit://callback crash: two Settings
    /// instances racing on PropertyChanged from different threads).
    /// </summary>
    public static Settings RequireCurrent() =>
        GetCurrent() ?? throw new InvalidOperationException(
            "Settings.Current is not bound. Call App.OnFrameworkInitializationCompleted first.");

    [ObservableProperty]
    public partial AuthenticationSettings Authentication { get; set; } = new();

    [ObservableProperty]
    public partial bool DarkMode { get; set; } = false;

    [ObservableProperty]
    public partial string BlogsApiUrl { get; set; } = "https://blogs.pschneider.fr/api/v1/";

    [ObservableProperty]
    public partial string BusinessApiUrl { get; set; } = "https://business.pschneider.fr/api/v1/";

    /// <summary>
    /// Catch top-level mutations: the four ObservableProperty
    /// setters above all funnel through here, and we flip
    /// <see cref="IsDirty"/> in lock-step. Sub-property mutations
    /// (e.g. <c>Authentication.Authority</c>) are caught by the
    /// subscription wired up in <see cref="OnAuthenticationChanged"/>
    /// below. <see cref="ApplyJson"/> disables the flag during bulk
    /// hydration so the disk load itself does not count as a user
    /// edit.
    /// </summary>
    private void MarkDirty() => IsDirty = true;

    partial void OnDarkModeChanged(bool value) => MarkDirty();
    partial void OnBlogsApiUrlChanged(string value) => MarkDirty();
    partial void OnBusinessApiUrlChanged(string value) => MarkDirty();

    /// <summary>
    /// Authentication can be reassigned wholesale by
    /// <see cref="ApplyJson"/>; on each reassignment we (re)wire a
    /// <c>PropertyChanged</c> listener so sub-property edits
    /// (Authority, ClientId, RedirectUri, Scopes) are picked up
    /// by the dirty tracker. We don't filter on PropertyName: any
    /// nested setter is treated as a user edit, which matches the
    /// user's mental model ("I typed in a field, the page is now
    /// dirty").
    /// </summary>
    partial void OnAuthenticationChanged(AuthenticationSettings value)
    {
        if (value is not null)
        {
            value.PropertyChanged += (_, _) => MarkDirty();
        }
        MarkDirty();
    }

    public bool Loaded { get; private set; } = false;

    /// <summary>
    /// True when the in-memory state has drifted from the last
    /// <see cref="Load"/> or <see cref="Save"/> snapshot. The
    /// Settings page binds the Sauver button's <c>IsEnabled</c> to
    /// this flag, so it only enables when the user has actually
    /// touched something since the last load / save. Cleared by
    /// <see cref="Load"/> (and by <see cref="ApplyJson"/>), set by
    /// every successful setter on the four top-level mutable
    /// properties and on the sub-properties of
    /// <see cref="Authentication"/>.
    /// </summary>
    [ObservableProperty]
    public partial bool IsDirty { get; private set; } = false;

    /// <summary>
    /// Guards every mutation of the observable state. <c>[ObservableProperty]</c>
    /// generates setters that call <c>SetProperty(...)</c> which fires
    /// <c>PropertyChanged</c>. Avalonia bindings consume that event on
    /// the UI thread, and a stray background-thread update is exactly
    /// what crashed <c>DataValidationErrors.SetErrors</c> on
    /// <c>postit://callback</c> re-launches. The lock makes mutations
    /// atomic; <see cref="OnPropertyChanged(PropertyChangedEventArgs)"/>
    /// then marshals the notification onto the UI thread so bindings
    /// observe the change on the right thread.
    /// </summary>
    private readonly object _mutationGate = new();

    /// <summary>
    /// Build OidcClient options configured for Authorization Code + PKCE
    /// (no client secret). The browser implementation should be supplied
    /// per-platform by the caller.
    /// </summary>
    internal OidcClientOptions GetOidcClientOptions(IdentityModel.OidcClient.Browser.IBrowser? browser = null)
    {
        if (!Loaded) Load();
        // Snapshot under the gate so the caller observes a consistent
        // view of all six properties; without this, a concurrent
        // Load() could swap Authentication mid-method and we would
        // build options from a torn read.
        lock (_mutationGate)
        {
            var options = new OidcClientOptions
            {
                Authority = Authentication.Authority,
                ClientId = Authentication.ClientId,
                RedirectUri = Authentication.RedirectUri,
                Scope = string.Join(' ', MergeScopes(this.Authentication.Scopes)),
                TokenClientCredentialStyle = IdentityModel.Client.ClientCredentialStyle.PostBody,
                PostLogoutRedirectUri = "https//yavsc.pschneider.fr",
                // PKCE is enabled by default when no client_secret is provided.
            };

            if (browser is not null)
                options.Browser = browser;

            return options;
        }
    }

    /// <summary>
    /// Scopes the PostIt client always requires from the OIDC provider,
    /// regardless of what the user has in their settings file.
    ///
    /// <para>PostIt calls into the Blog API (and any other Yavsc API
    /// gated by an <c>[Authorize("…Scope")]</c> policy) and is silent
    /// about the contract: a missing scope here surfaces as a 401
    /// on the very first API call after login, with no obvious link
    /// to the settings. The "feature" scopes the user must opt into
    /// (e.g. <c>blogs</c>) are still their choice — we only force the
    /// structural ones that OIDC itself needs.</para>
    /// </summary>
    private static readonly string[] BuiltInScopes = new[]
    {
        "openid",        // OIDC: required for the id_token
        "profile",       // OIDC: standard profile claims
        "offline_access" // OIDC: required to receive a refresh_token
    };

    /// <summary>
    /// Merge user-configured scopes with the built-in ones. User scopes
    /// come first (preserves author intent), then the built-ins, with
    /// duplicates removed case-sensitively. <c>null</c> or empty input
    /// is fine — we still emit the built-ins.
    /// </summary>
    internal static IEnumerable<string> MergeScopes(string[]? userScopes)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        if (userScopes is not null)
        {
            foreach (var s in userScopes)
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                if (seen.Add(s)) yield return s;
            }
        }
        foreach (var s in BuiltInScopes)
        {
            if (seen.Add(s)) yield return s;
        }
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
        lock (_mutationGate)
        {
            if (Loaded) return; // double-check after taking the gate
            if (!string.IsNullOrWhiteSpace(Authentication?.Authority))
            {
                Loaded = true;
                return;
            }
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
            // Apply under the gate so concurrent Load() callers cannot
            // see half the new values / half the old ones. The actual
            // PropertyChanged fan-out is handled by [ObservableProperty]'s
            // setters which we route through SetProperty → OnPropertyChanged
            // → our overridden dispatcher-safe marshaller below.
            lock (_mutationGate)
            {
                this.Authentication = settings.Authentication;
                this.DarkMode = settings.DarkMode;
                if (!(settings.Authentication is null))
                {
                    this.Authentication = new AuthenticationSettings();
                    this.Authentication.Authority = string.IsNullOrWhiteSpace(settings.Authentication.Authority) ?
                    AuthenticationSettings.DefaultAuthority : settings.Authentication.Authority;
                    this.Authentication.ClientId = string.IsNullOrWhiteSpace(settings.Authentication.ClientId) ?
                     AuthenticationSettings.DefaultClientId : settings.Authentication.ClientId;
                    this.Authentication.RedirectUri = string.IsNullOrWhiteSpace(settings.Authentication.RedirectUri) ?
                     AuthenticationSettings.DefaultDesktopRedirectUri : settings.Authentication.RedirectUri;
                    this.Authentication.Scopes = settings.Authentication.Scopes;
                }
            }
            // A disk load (or an embedded-resource fallback) is the
            // baseline, not a user edit. Clear the dirty flag last
            // so the OnAuthenticationChanged / sub-property fan-out
            // triggered by the assignments above doesn't leave it
            // stuck at true.
            IsDirty = false;
            // Re-notify the command in case the button was bound
            // before Load finished and the CanExecute cache is
            // stale.
            SaveCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"🩎 Error applying settings from {source}: {ex.Message}");
        }
    }

    /// <summary>
    /// Persist the current in-memory state to
    /// <c>~/.config/PostIt/postit-settings.json</c> (Linux) /
    /// equivalent <c>%APPDATA%\PostIt\postit-settings.json</c>
    /// (Windows). Symmetrical to <see cref="Load"/>: same path,
    /// same directory creation, same <c>0600</c> file mode (POSIX)
    /// as <c>TokenStore.Save</c>. Clears <see cref="IsDirty"/>
    /// on success.
    ///
    /// <para>Synchronous on purpose: matches <see cref="Load"/>'s
    /// contract (the file is a few KiB at most, and the Avalonia
    /// UI thread cannot await here without risking the same
    /// deadlock <see cref="Load"/>'s docstring describes).
    /// </para>
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PostIt");
        Directory.CreateDirectory(configDir);
        var configPath = Path.Combine(configDir, SettingsFileName);

        lock (_mutationGate)
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true,
                });
                File.WriteAllText(configPath, json);
                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    File.SetUnixFileMode(configPath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite);
                IsDirty = false;
                Console.WriteLine($"💾 Settings saved to {configPath}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"🩎 Error saving settings to {configPath}: {ex.Message}");
                throw;
            }
        }
    }

    private bool CanSave() => IsDirty;

    /// <summary>
    /// Re-notify the <c>SaveCommand</c> (generated by
    /// <c>[RelayCommand]</c> on <see cref="Save"/>) so XAML
    /// re-evaluates <c>CanExecute</c> when the dirty flag flips
    /// outside the scope of a direct save (e.g. on <see cref="Load"/>
    /// / <see cref="ApplyJson"/>).
    /// </summary>
    partial void OnIsDirtyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    public override bool CanNavigateNext { get => false; protected set => throw new System.NotImplementedException(); }
    public override bool CanNavigatePrevious { get => true; protected set => throw new System.NotImplementedException(); }
}
