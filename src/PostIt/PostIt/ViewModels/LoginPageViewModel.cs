using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using IdentityModel.OidcClient.Browser;
using PostIt.Services;

namespace PostIt.ViewModels;

public partial class LoginPageViewModel : ViewModelBase
{
    private const string SettingsFileName = "postit-settings.json";

    [Obsolete("Password grant is not used; IdentityModel.OidcClient performs PKCE.")]
    public string Password { get; set; } = string.Empty;

    [Obsolete("User-entered email is not used; the IdP login UI collects it.")]
    public string UserEmail { get; set; } = string.Empty;

    [Obsolete("No local credential persistence in the current build.")]
    public bool RememberMe { get; set; }

    /// <summary>
    /// URL of the Yavsc.Org account-registration page.
    /// Derived from <see cref="Settings.Authentication"/>'s Authority.
    /// Empty when the authority is not configured.
    /// </summary>
    public string RegisterUrl =>
        BuildExternalUrl("/Account/Register");

    /// <summary>
    /// URL of the Yavsc.Org password-reset page (open to anonymous users).
    /// Derived from <see cref="Settings.Authentication"/>'s Authority.
    /// Empty when the authority is not configured.
    /// </summary>
    public string ForgotPasswordUrl =>
        BuildExternalUrl("/Account/ForgotPassword");

    public bool HasRegisterUrl => !string.IsNullOrEmpty(RegisterUrl);
    public bool HasForgotPasswordUrl => !string.IsNullOrEmpty(ForgotPasswordUrl);

    /// <summary>
    /// Canonical <see cref="Settings.Authentication"/> authority with any trailing
    /// slash removed. Used as the base for both the OIDC discovery URL and the
    /// human-facing Account URLs (Register / Forgot password). Empty when the
    /// authority is not configured.
    /// </summary>
    public string ExternalUrl => BuildExternalUrl(string.Empty);

    /// <summary>
    /// OIDC discovery URL the client actually calls during login:
    /// <c>ExternalUrl + "/.well-known/openid-configuration"</c>. Surfaced in
    /// <see cref="StatusMessage"/> on failure so the operator can copy it
    /// verbatim and verify reachability from a browser.
    /// </summary>
    public string DiscoveryUrl =>
        string.IsNullOrEmpty(ExternalUrl) ? string.Empty : ExternalUrl + "/.well-known/openid-configuration";

    /// <summary>
    /// True when the settings file is missing or <c>Authentication.Authority</c>
    /// is empty. The LoginPage surfaces a banner in that case and disables
    /// the Register / Forgot password buttons.
    /// </summary>
    public bool ConfigMissing =>
        string.IsNullOrWhiteSpace(Settings.Authentication?.Authority);

    /// <summary>
    /// Localised banner shown when <see cref="ConfigMissing"/> is true.
    /// The path follows the XDG spec on Linux (where PostIt.Desktop runs):
    /// the file is expected at <c>~/.config/PostIt/postit-settings.json</c>.
    /// </summary>
    public string ConfigMissingMessage =>
        $"Configuration PostIt manquante — voir ~/.config/PostIt/postit-settings.json";

    private string BuildExternalUrl(string path)
    {
        var authority = Settings.Authentication?.Authority?.TrimEnd('/');
        return string.IsNullOrEmpty(authority)
            ? string.Empty
            : authority + path;
    }

    /// <summary>
    /// The access token of the most recent successful login, or null.
    /// Kept on the VM so views can show "logged in as …" feedback; the
    /// authoritative copy lives in the <see cref="TokenStore"/>.
    /// </summary>
    private string? _accessToken;
    public string? AccessToken
    {
        get => _accessToken;
        private set => this.SetProperty(ref _accessToken, value);
    }

    public override bool CanNavigateNext { get => false; protected set => throw new NotImplementedException(); }
    public override bool CanNavigatePrevious { get => true; protected set => throw new NotImplementedException(); }

    public Settings Settings { get; }

    private string _statusMessage = "Ready";
    public string StatusMessage
    {
        get => _statusMessage;
        private set => this.SetProperty(ref _statusMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set => this.SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Optional override used by tests. When set, this factory is called
    /// instead of <see cref="Platform.CreateBrowser"/> to obtain the
    /// <see cref="IBrowser"/> instance.
    /// </summary>
    public Func<IBrowser?>? BrowserFactoryOverride { get; set; }

    /// <summary>
    /// Optional override used by tests. When set, this delegate replaces
    /// the call to <see cref="Settings.Load"/> at the start of
    /// <see cref="LoginAsync"/>, so tests can inject a Settings object
    /// without it being overwritten by the user/embedded default.
    /// </summary>
    public Func<Task>? SettingsLoadOverride { get; set; }

    /// <summary>
    /// Optional override used by tests. When set, the VM hands this
    /// pre-built <see cref="YavscApiClient"/> to itself instead of
    /// constructing a fresh one.
    /// </summary>
    public YavscApiClient? ApiClientOverride { get; set; }

    private YavscApiClient? _api;

    public LoginPageViewModel() : this(new Settings(), apiClient: null, browserFactoryOverride: null)
    {
        // Load settings eagerly so RegisterUrl / ForgotPasswordUrl are
        // populated as soon as the page renders (XAML bindings fire
        // before the user clicks Login). Settings.Load is synchronous
        // on purpose; calling .GetAwaiter().GetResult() on it would
        // deadlock the UI thread on the await inside the file read.
        try { Settings.Load(); }
        catch { /* settings may be missing in tests/dev; LoginAsync will surface real errors */ }
    }

    /// <summary>
    /// Test-friendly constructor: caller supplies pre-loaded
    /// <paramref name="settings"/>, an optional
    /// <paramref name="browserFactoryOverride"/> that bypasses the
    /// static <see cref="Platform"/> indirection, and an optional
    /// pre-built <paramref name="apiClient"/> for end-to-end
    /// scenarios where the test owns the wiring.
    /// </summary>
    public LoginPageViewModel(
        Settings settings,
        Func<IBrowser?>? browserFactoryOverride = null,
        YavscApiClient? apiClient = null)
    {
        Settings = settings;
        BrowserFactoryOverride = browserFactoryOverride;
        ApiClientOverride = apiClient;
        StatusMessage = "Ready";
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        try
        {
            IsBusy = true;

            if (SettingsLoadOverride is not null)
                await SettingsLoadOverride().ConfigureAwait(false);
            else
                Settings.Load();

            // Guard: refuse to call OidcClient when the authority is
            // empty. IdentityModel would otherwise build a bogus
            // authorize URL like "http://127.0.0.1:1/" from an empty
            // Authority, which the browser refuses with a confusing
            // "Cette adresse est interdite"-style message. Tell the
            // operator exactly what to fix instead.
            if (string.IsNullOrWhiteSpace(Settings.Authentication?.Authority))
            {
                IsBusy = false;
                StatusMessage =
                    $"Configuration manquante — édite {SettingsFileHint()} et renseigne Authentication.Authority";
                return;
            }

            // The platform project picks the right redirect URI and
            // browser implementation; we don't reference any UI
            // toolkit from here.
            Settings.RedirectUri = string.IsNullOrWhiteSpace(Settings.RedirectUri)
                ? Platform.DefaultRedirectUri
                : Settings.RedirectUri;

            // Surface the discovery URL the client is about to call,
            // so a failure (DNS, TLS, 404) can be diagnosed by
            // pasting the URL straight into a browser. OidcClient
            // computes the discovery URL as
            // `Authority + /.well-known/openid-configuration`; we
            // normalise the trailing slash here so the printed URL is
            // exactly what IdentityModel will fetch.
            if (!string.IsNullOrEmpty(DiscoveryUrl))
                StatusMessage = $"Discovering {DiscoveryUrl}";

            // Build (or reuse) the API client. The browser override
            // takes precedence: tests want to inject a fake browser
            // and the production path uses Platform.CreateBrowser.
            _api ??= ApiClientOverride ?? new YavscApiClient(Settings, BuildTokenStore());

            // Platform.CreateBrowser may still want to be customised
            // per-call (e.g. between desktop and android), so route
            // the interactive login through a callback that reuses
            // BrowserFactoryOverride when present.
            await LoginInteractiveCoreAsync(_api).ConfigureAwait(false);

            IsBusy = false;
            AccessToken = _api.CurrentAccessToken;
            StatusMessage = "Interactive token acquired.";
        }
        catch (Exception ex)
        {
            IsBusy = false;
            var suffix = !string.IsNullOrEmpty(DiscoveryUrl) ? $" (discovery: {DiscoveryUrl})" : string.Empty;
            StatusMessage = $"Error: {ex.Message}{suffix}";
        }
    }

    /// <summary>
    /// Single entry point for the OIDC login: YavscApiClient owns the
    /// browser choice, the OidcClient instance, the token persistence
    /// and the refresh path. The VM is just a thin coordinator.
    /// </summary>
    private async Task LoginInteractiveCoreAsync(YavscApiClient api)
    {
        var original = Platform.CreateBrowser;
        try
        {
            if (BrowserFactoryOverride is not null)
                Platform.CreateBrowser = BrowserFactoryOverride;

            await api.LoginInteractiveAsync().ConfigureAwait(false);
        }
        finally
        {
            Platform.CreateBrowser = original;
        }
    }

    /// <summary>
    /// XDG-compliant path to the user settings file. Surfaced in the
    /// "Configuration manquante" message so the operator knows exactly
    /// which file to edit without having to dig through docs.
    /// </summary>
    private static string SettingsFileHint()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "PostIt", "postit-settings.json");
    }

    /// <summary>
    /// Build the on-disk <see cref="TokenStore"/> used by
    /// <see cref="YavscApiClient"/>. The token bundle lives in
    /// <c>~/.config/PostIt/tokens.json</c> on Linux; the same path
    /// layout is used on every platform for predictability.
    /// </summary>
    private static TokenStore BuildTokenStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var path = Path.Combine(appData, "PostIt", "tokens.json");
        return new TokenStore(path);
    }
}
