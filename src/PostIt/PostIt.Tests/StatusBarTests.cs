using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using PostIt.Controls;
using PostIt.ViewModels;

namespace PostIt.Tests;

/// <summary>
/// The <see cref="StatusBar"/> surfaces the page's action status
/// (success / warning / error message). The user reported being
/// unable to copy these messages — the status text was a
/// <c>TextBlock</c>, which Avalonia does not make selectable. It is
/// now a <see cref="SelectableTextBlock"/> so the message can be
/// selected and copied (Ctrl+C / context menu). This test pins that:
/// if the message control regresses to a plain <c>TextBlock</c>,
/// selectability is lost and the test fails.
/// </summary>
public class StatusBarTests
{
    [AvaloniaFact]
    public void Status_message_is_rendered_by_a_SelectableTextBlock()
    {
        var bar = new StatusBar { DataContext = StatusNotice.Error("Échec de la suppression : boom") };
        var window = new Window { Content = bar };
        window.Show();

        var message = bar.GetVisualDescendants()
            .OfType<SelectableTextBlock>()
            .FirstOrDefault();

        Assert.NotNull(message);
        Assert.Equal("Échec de la suppression : boom", message!.Text);
    }
}