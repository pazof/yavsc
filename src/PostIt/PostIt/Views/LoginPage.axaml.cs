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

    public Settings Settings { get; }

    public LoginPage()
    {
        this.Settings = new Settings();
        InitializeComponent();
    }

    public async Task<LoginResult?> StartLoginAsync(OidcClientOptions options)
    {
        try
        {
            Settings.Load().Wait();
            
            var client = new OidcClient(options);
            var loginResult = await client.LoginAsync(new LoginRequest());
            return loginResult;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        // This is where you specify your actual login auth logic
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
