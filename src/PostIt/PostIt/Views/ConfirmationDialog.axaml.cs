using Avalonia.Markup.Xaml;

namespace PostIt.Views;

public partial class ConfirmationDialog : Avalonia.Controls.ContentPage
{
    public ConfirmationDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
