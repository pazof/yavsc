using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PostIt.Views;

public partial class BillingCommandPage : ContentPage
{
    public BillingCommandPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}