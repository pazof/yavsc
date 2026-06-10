using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IdentityModel.OidcClient;
using PostIt.Services;

namespace PostIt.Views;

public partial class LoginWindow : Window
{
    private TaskCompletionSource<LoginResult?> _tcs = new();

    public LoginWindow()
    {
        InitializeComponent();
        CancelButton.Click += (_, __) => Close(null);
    }

    public async Task<LoginResult?> StartLoginAsync(OidcClientOptions options)
    {
        try
        {
            var client = new OidcClient(options);
            var loginResult = await client.LoginAsync(new LoginRequest());
            return loginResult;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
