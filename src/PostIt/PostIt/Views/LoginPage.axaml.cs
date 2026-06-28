using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        // HomePage pushes LoginPage via PushModalAsync(new LoginPage())
        // without supplying a DataContext. Attach a freshly-built
        // LoginPageViewModel whenever the caller hasn't wired one up,
        // so XAML bindings and LoginAsyncCommand resolve. We resolve
        // through the DI container (not `new LoginPageViewModel()`)
        // so the LoginPageViewModel shares the canonical Settings
        // singleton with the rest of the app — constructing a fresh
        // VM here was the original source of the two-Settings
        // postit://callback crash.
        if (DataContext is null)
        {
            var services = (App.Current as App)?.Services
                ?? throw new InvalidOperationException(
                    "App.Services is not bound. OnFrameworkInitializationCompleted must run before any view handler.");
            DataContext = services.GetRequiredService<LoginPageViewModel>();
        }
    }

    private async void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        // Cancel button dismisses all open modals
        if (Navigation is not null)
            await Navigation.PopAllModalsAsync();
    }

    private void OnRegisterClick(object? sender, RoutedEventArgs e)
    {
        OpenExternalUrl((DataContext as LoginPageViewModel)?.RegisterUrl);
    }

    private void OnForgotPasswordClick(object? sender, RoutedEventArgs e)
    {
        OpenExternalUrl((DataContext as LoginPageViewModel)?.ForgotPasswordUrl);
    }

    private static void OpenExternalUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        // Desktop launcher: shell-execute the URL so the OS picks the right handler.
        // Platform projects (PostIt.Android, PostIt.Browser) override this behavior
        // when they plug into the LoginPage lifecycle.
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open external URL {url}: {ex.Message}");
        }
    }
}