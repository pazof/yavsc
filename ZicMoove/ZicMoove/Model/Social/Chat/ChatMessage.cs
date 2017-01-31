using ZicMoove.Data;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ZicMoove.Model.Social.Messaging
{
    public class ChatMessage
    {
        public DateTime Date { get; set; }
        public string SenderId { get; set; }
        public string Message { get; set; }
        public bool Read { get; set; }
    }
}
