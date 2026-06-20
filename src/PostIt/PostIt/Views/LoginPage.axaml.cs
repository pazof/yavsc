using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IdentityModel.OidcClient;
using PostIt.Services;

namespace PostIt.Views;

public partial class LoginPage : ContentPage
{
    private TaskCompletionSource<LoginResult?> _tcs = new();
    private LoginResult loginResult;


    public Settings Settings { get; }

    public LoginPage()
    {
        this.Settings = new Settings();
        InitializeComponent();
    }

    private async void OnLoginClickAsync(object? sender, RoutedEventArgs e)
    {
        // This is where you specify your actual login auth logic
        try
        {
            Settings.Load().Wait();
            
            var client = new OidcClient(Settings.GetOidcClientOptions());
            var loginResult = await client.LoginAsync(new LoginRequest());
            this.loginResult = loginResult;
        }
        catch (Exception)
        {
            this.loginResult = null;
        }

        if (Navigation is not null)
            await Navigation.PopModalAsync();
    }

    private async void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        // Cancel button dismisses all open modals
        if (Navigation is not null)
            await Navigation.PopAllModalsAsync();
    }
}
