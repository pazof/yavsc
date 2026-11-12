using System.Threading.Tasks;

namespace PostIt.ViewModels.Chat;

public sealed class CallPageViewModel(ChatViewModel chat) : ViewModelBase
{
    public ChatViewModel Chat { get; } = chat;
    public override bool CanNavigateNext { get; protected set; } = false;
    public override bool CanNavigatePrevious { get; protected set; } = true;

    public Task HangupAsync() => Chat.HangupCallsAsync();
}
