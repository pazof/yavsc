using Avalonia.Controls;

namespace PostIt;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Greeting.Content="Hello Joe!";
    }
}