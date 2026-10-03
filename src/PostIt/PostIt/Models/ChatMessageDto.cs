namespace PostIt.ViewModels.Chat
{
    /// <summary>
    /// DTO minimal calqué sur l'objet anonyme émis par le hub
    /// (<c>{ Name, Room, Message }</c>) via l'événement
    /// <c>ReceiveMessage</c>.
    /// </summary>
    public class ChatMessageDto
    {
        public string? Name { get; set; }
        public string? Room { get; set; }
        public string? Message { get; set; }
    }
}
