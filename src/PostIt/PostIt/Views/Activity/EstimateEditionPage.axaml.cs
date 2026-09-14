using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PostIt.Views;

public partial class EstimateEditionPage : ContentPage
{
    public EstimateEditionPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
