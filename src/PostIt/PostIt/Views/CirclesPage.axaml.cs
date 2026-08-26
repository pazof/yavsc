using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
