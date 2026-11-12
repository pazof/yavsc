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
        if (DataContext is ChatViewModel chat)
        {
            if (chat.VideoCall is { } video) await video.HangupAsync();
            if (chat.LanCall is { } lan) await lan.HangupAsync();
        }
    }
}
