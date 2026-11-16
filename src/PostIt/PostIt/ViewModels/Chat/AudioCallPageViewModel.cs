namespace PostIt.ViewModels.Chat;

public sealed class AudioCallPageViewModel(ChatViewModel chat) : CallPageViewModel(chat)
{
    public override string Header => "Appel audio";
    public override bool IsAudioPage => true;
    public override string Description => Chat.LanCall?.SupportsAudioOnly == true
        ? "Appel audio WebRTC ou SRTP direct. Les deux participants doivent etre presents dans le meme salon. Quitter cette page termine l'appel."
        : "Appel audio WebRTC. Les deux participants doivent etre presents dans le meme salon. Quitter cette page termine l'appel.";
}
