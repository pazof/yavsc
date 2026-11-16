using System.Threading.Tasks;

namespace PostIt.ViewModels.Chat;

public class CallPageViewModel(ChatViewModel chat) : ViewModelBase
{
    public virtual string Header => "Appel audio-video";
    public virtual bool IsAudioPage => false;
    public bool ShowLanCalls => !IsAudioPage && Chat.SupportsLanCalls;
    public virtual string Description => "Choisissez WebRTC ou SRTP direct. Les deux participants doivent etre presents dans le meme salon. Quitter cette page termine l'appel.";
    public ChatViewModel Chat { get; } = chat;
    public override bool CanNavigateNext { get; protected set; } = false;
    public override bool CanNavigatePrevious { get; protected set; } = true;

    public Task HangupAsync() => Chat.HangupCallsAsync();
}
