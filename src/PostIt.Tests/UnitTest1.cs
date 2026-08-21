using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using PostIt.Views;

namespace PostIt.Tests;

public class MainPageTests
{
    [AvaloniaFact]
    public void MainPage_Should_Load()
    {
        var window = new MainWindow();
        window.Show();
        Assert.NotNull(window);
    }
}