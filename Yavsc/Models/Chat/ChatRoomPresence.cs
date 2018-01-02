
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Abstract.Streaming;

namespace Yavsc.Models.Chat
{
    public class ChatRoomPresence: IChatRoomUsage
    {
        public string ChannelName { get; set; }
        [ForeignKey("ChannelName")]
        public virtual ChatRoom Room { get; set; }

        public string ChatUserConnectionId { get; set; }
        
        [ForeignKey("ChatUserConnectionId")]
        public virtual ChatConnection ChatUserConnection { get; set; }

        public ChatRoomUsageLevel Level
        {
            get; set;
        }
    }
}