using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class CirclesPage : ContentPage
{
    public CirclesPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
