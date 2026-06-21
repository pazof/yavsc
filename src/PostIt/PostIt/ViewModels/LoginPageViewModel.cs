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
            Settings.Load().Wait();

            // The platform project picks the right redirect URI and browser
            // implementation; we don't reference any UI toolkit from here.
            Settings.RedirectUri = string.IsNullOrWhiteSpace(Settings.RedirectUri)
                ? Platform.DefaultRedirectUri
                : Settings.RedirectUri;

            var browser = BrowserFactoryOverride is not null
                ? BrowserFactoryOverride.Invoke()
                : Platform.CreateBrowser?.Invoke();
            if (browser is null)
            {
                StatusMessage = "No browser is available on this platform.";
                return;
            }

            var client = new OidcClient(Settings.GetOidcClientOptions(browser));
            var loginResult = await client.LoginAsync(new LoginRequest());

            if (loginResult.IsError)
            {
                StatusMessage = loginResult.Error;
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
            StatusMessage = "Error: "+ex.Message;
        }
    }
}
