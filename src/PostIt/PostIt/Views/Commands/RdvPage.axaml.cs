using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
}
