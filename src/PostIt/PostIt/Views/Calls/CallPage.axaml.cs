using Avalonia;
using Avalonia.Controls;
using PostIt.ViewModels.Chat;

namespace PostIt.Views.Chat;

public partial class CallPage : ContentPage
{
    public CallPage() => InitializeComponent();

    protected override async void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is CallPageViewModel call) await call.HangupAsync();
    }
}
