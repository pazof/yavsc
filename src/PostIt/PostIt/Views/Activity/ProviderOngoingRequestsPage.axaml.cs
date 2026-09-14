using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PostIt.Views;

public partial class ProviderOngoingRequestsPage : ContentPage
{
    public ProviderOngoingRequestsPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
