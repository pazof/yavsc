namespace PostIt.ViewModels.Chat
{
    /// <summary>
    /// DTO de message de salon, calqué sur la charge envoyée par
    /// <c>ChatHub.Send</c> côté serveur ({ Name, Room, Message }).
    /// Le protocole JSON de SignalR sérialise en camelCase ; la
    /// désérialisation est insensible à la casse par défaut.
    /// </summary>
    public class ChatMessage
    {
        public string? Name { get; set; }
        public string? Room { get; set; }
        public string? Message { get; set; }
    }
}
