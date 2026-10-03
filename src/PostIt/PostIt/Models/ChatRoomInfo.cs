using System.Collections.Generic;

namespace PostIt.ViewModels.Chat
{
    // DTO minimal calqué sur votre logique serveur ChatRoomInfo
    public class ChatRoomInfo
    {
        public string? Name { get; set; }
        public List<string> Users { get; set; } = new();
    }
}
