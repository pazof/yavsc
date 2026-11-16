using Avalonia.Controls;
using PostIt.Models;
using PostIt.Views;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
[Collection(AvaloniaUiCollection.Name)]
public class NotificationModalTests(AvaloniaUiFixture ui)
{
    [Fact]
    public Task Error_message_is_selectable_in_the_notification_modal()
        => ui.RunAsync(() =>
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
    }, TestContext.Current.CancellationToken);
}
