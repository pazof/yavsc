using CommunityToolkit.Mvvm.Input;
using IdentityModel.OidcClient;
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
            
            var client = new OidcClient(Settings.GetOidcClientOptions());
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