using Avalonia;
using Avalonia.Controls;
using PostIt.ViewModels.Chat;
namespace PostIt.Views.Chat;
public partial class ChatPage : ContentPage
{

    public ChatPage()
    {
        InitializeComponent();
    }

    protected override async void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is ChatViewModel { VideoCall: { } call })
            await call.HangupAsync();
    }
}
