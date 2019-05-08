
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Abstract.Chat;

namespace Yavsc.Models.Chat
{
    public class ChatRoom: IChatRoom<ChatRoomAccess>
    {
        public string Topic { get; set; }

        [Key]
        [StringLengthAttribute(Constants.MaxChanelName, MinimumLength=3)]
        public string Name { get; set;}

        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }

        [InverseProperty("Room")]
        public virtual List<ChatRoomAccess> Moderation { get; set; }

    }
}