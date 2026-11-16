using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using PostIt.Models;
using PostIt.Views;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
public class NotificationModalTests
{
    [AvaloniaFact]
    public void Error_message_is_selectable_in_the_notification_modal()
    {
        var root = new MainView();
        var window = new Window { Content = root };
        try
        {
            window.Show();
            root.ShowNotificationModal("Échec de la suppression : boom", StatusSeverity.Error);

            var message = root.FindControl<SelectableTextBlock>("NotificationModalMessage");
            Assert.NotNull(message);
            Assert.True(root.NotificationModal.IsVisible);
            Assert.Equal("Échec de la suppression : boom", message.Text);
        }
        finally
        {
            window.Close();
        }
    }
}
