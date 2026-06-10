
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace PostIt.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

     private async void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        if (Navigation is not null)
        
            await Navigation.PushModalAsync(new LoginPage());
    }

}