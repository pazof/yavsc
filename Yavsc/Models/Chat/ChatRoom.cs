
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Abstract.Streaming;

namespace Yavsc.Models.Chat
{
    public class ChatRoom: IChatRoom<ChatRoomPresence>
    {
        [StringLengthAttribute(1023,MinimumLength=1)]
        public string Topic { get; set; }

        [Key]
        [StringLengthAttribute(255,MinimumLength=1)]
        public string Name { get; set;}

        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser Owner { get; set; }

        [InverseProperty("Room")]
        public virtual List<ChatRoomPresence> UserList { get; set;}

    }
}