using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PostIt.ViewModels.Commands;

namespace PostIt.Views.Commands;

public partial class RdvPage : ContentPage
{
    public RdvPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnEditAddressClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not RdvViewModel vm)
            return;

        if (Application.Current is App app)
            await app.PushPageAsync(vm.PhysicalAddress);
    }
}
