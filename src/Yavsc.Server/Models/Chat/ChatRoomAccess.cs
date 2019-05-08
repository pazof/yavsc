
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Abstract.Chat;

namespace Yavsc.Models.Chat
{
    public class ChatRoomAccess: IChatRoomAccess
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id
        {
            get; set;
        }
        
        public string ChannelName { get; set; }
        [ForeignKey("ChannelName")]
        public virtual ChatRoom Room { get; set; }

        [Required]
        public string UserId { get; set; }

        public ChatRoomAccessLevel Level
        {
            get; set;
        }



        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}