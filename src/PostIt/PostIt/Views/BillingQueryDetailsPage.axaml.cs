using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PostIt.Views;

public partial class BillingQueryDetailsPage : ContentPage
{
    public BillingQueryDetailsPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
