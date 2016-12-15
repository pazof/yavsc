using System;

namespace BookAStar.Model.Social.Messaging
{
    public class ChatMessage
    {
        public DateTime Date { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
    }
}
