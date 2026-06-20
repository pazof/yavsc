using CommunityToolkit.Mvvm.Input;
using IdentityModel.OidcClient;
using PostIt.Services;
using System;
using System.Threading.Tasks;

namespace PostIt.ViewModels;

public partial class LoginPageViewModel : ViewModelBase
{
    public string UserEmail { get; set; }
    public string Password { get; set; }

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

    public LoginPageViewModel()
    {
        Settings = new Settings();
        StatusMessage = "Ready";
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        try
        {
            Settings.Load().Wait();

            // The platform project picks the right redirect URI and browser
            // implementation; we don't reference any UI toolkit from here.
            Settings.RedirectUri = string.IsNullOrWhiteSpace(Settings.RedirectUri)
                ? Platform.DefaultRedirectUri
                : Settings.RedirectUri;

            var browser = Platform.CreateBrowser?.Invoke();
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