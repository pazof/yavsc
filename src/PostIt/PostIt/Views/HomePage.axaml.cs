
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        var vm = (HomePageViewModel)DataContext!;
        // Resolve the next view-model from the same DI container that
        // produced vm.Settings. Constructing them with `new` would
        // instantiate a second Settings and reintroduce the
        // postit://callback crash we just fixed in Settings.cs.
        var services = (App.Current as App)?.Services
            ?? throw new System.InvalidOperationException(
                "App.Services is not bound. OnFrameworkInitializationCompleted must run before any view handler.");
        var loginVm = services.GetRequiredService<LoginPageViewModel>();
        loginVm.LoginSucceeded += () =>
        {
            var client = services.GetRequiredService<BlogApiClient>();
            Navigation?.PushAsync(new MainPage
            {
                DataContext = services.GetRequiredService<MainPageViewModel>()
            });
        };
        Navigation?.PushAsync(new LoginPage { DataContext = loginVm });
    }
}
