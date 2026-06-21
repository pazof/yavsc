using CommunityToolkit.Mvvm.Input;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using PostIt.Services;
using System;
using System.Threading.Tasks;

namespace PostIt.ViewModels;

public partial class LoginPageViewModel : ViewModelBase
{
    public string UserEmail { get; set; }
    public string Password { get; set; }

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

    private string _AccessToken;
    public string AccessToken { get => _AccessToken; private set => this.SetProperty(ref _AccessToken, value); }

    public bool RememberMe { get; set; }
    public override bool CanNavigateNext { get => false; protected set => throw new System.NotImplementedException(); }
    public override bool CanNavigatePrevious { get => true; protected set => throw new System.NotImplementedException(); }

    public Settings Settings { get; }

    private string _StatusMessage;
    public string StatusMessage { get => _StatusMessage; private set => this.SetProperty(ref _StatusMessage, value); }

    private bool _IsBusy;
    public bool IsBusy { get=> _IsBusy; private set=> this.SetProperty(ref _IsBusy, value); }

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

    public LoginPageViewModel() : this(new Settings(), browserFactoryOverride: null)
    {
        // Load settings eagerly so RegisterUrl / ForgotPasswordUrl are
        // populated as soon as the page renders (XAML bindings fire
        // before the user clicks Login).
        try { Settings.Load().GetAwaiter().GetResult(); }
        catch { /* settings may be missing in tests/dev; LoginAsync will surface real errors */ }
    }

    /// <summary>
    /// Test-friendly constructor: caller supplies pre-loaded <paramref name="settings"/>
    /// and (optionally) a <paramref name="browserFactoryOverride"/> that bypasses
    /// the static <see cref="Platform"/> indirection.
    /// </summary>
    public LoginPageViewModel(Settings settings, Func<IBrowser?>? browserFactoryOverride = null)
    {
        Settings = settings;
        BrowserFactoryOverride = browserFactoryOverride;
        StatusMessage = "Ready";
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        try
        {
            this.IsBusy = true;
            (SettingsLoadOverride ?? Settings.Load)().Wait();

            // Guard: if the authority is empty (no user settings file and
            // the embedded default couldn't be loaded for any reason),
            // refuse to call OidcClient. IdentityModel would otherwise
            // build a bogus authorize URL like "http://127.0.0.1:1/"
            // from an empty Authority, which the browser then refuses to
            // open with a confusing "Cette adresse est interdite"
            // (or equivalent) message. Tell the operator exactly what
            // to fix instead.
            if (string.IsNullOrWhiteSpace(Settings.Authentication?.Authority))
            {
                this.IsBusy = false;
                StatusMessage = $"Configuration manquante — édite {SettingsFileHint()} et renseigne Authentication.Authority";
                return;
            }

            // The platform project picks the right redirect URI and browser
            // implementation; we don't reference any UI toolkit from here.
            Settings.RedirectUri = string.IsNullOrWhiteSpace(Settings.RedirectUri)
                ? Platform.DefaultRedirectUri
                : Settings.RedirectUri;

            // Surface the discovery URL the client is about to call, so a
            // failure (DNS, TLS, 404) can be diagnosed by pasting the URL
            // straight into a browser. OidcClient computes the discovery
            // URL as `Authority + /.well-known/openid-configuration`; we
            // normalise the trailing slash here so the printed URL is
            // exactly what IdentityModel will fetch.
            if (!string.IsNullOrEmpty(DiscoveryUrl))
                StatusMessage = $"Discovering {DiscoveryUrl}";

            var browser = BrowserFactoryOverride is not null
                ? BrowserFactoryOverride.Invoke()
                : Platform.CreateBrowser?.Invoke();
            if (browser is null)
            {
                StatusMessage = $"No browser is available on this platform. (discovery: {DiscoveryUrl})";
                return;
            }

            var client = new OidcClient(Settings.GetOidcClientOptions(browser));
            var loginResult = await client.LoginAsync(new LoginRequest());

            if (loginResult.IsError)
            {
                StatusMessage = $"{loginResult.Error} (discovery: {DiscoveryUrl})";
                return;
            }

            StatusMessage = "Interactive token acquired.";

            this.IsBusy = false;
            AccessToken = loginResult.AccessToken;
            /* TODO save or not user log and password
            await Task.Run(() =>
            {
                Settings.Save().Wait();
            });*/

        }
        catch (Exception ex)
        {
            this.IsBusy = false;
            var suffix = !string.IsNullOrEmpty(DiscoveryUrl) ? $" (discovery: {DiscoveryUrl})" : string.Empty;
            StatusMessage = $"Error: {ex.Message}{suffix}";
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
        return System.IO.Path.Combine(appData, "PostIt", "postit-settings.json");
    }
}
