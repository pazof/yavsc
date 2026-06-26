
using Avalonia.Controls;
using Avalonia.Interactivity;
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
        var loginVm = new LoginPageViewModel(vm.Settings, apiClient: vm.Api);
        loginVm.LoginSucceeded += () =>
        {
            var client = new BlogApiClient(vm.Api);
            Navigation?.PushAsync(new MainPage
            {
                DataContext = new MainPageViewModel(client, vm.Settings)
            });
        };
        Navigation?.PushAsync(new LoginPage { DataContext = loginVm });
    }
}
