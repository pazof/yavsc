
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Abstract.Chat;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Chat
{
    public class ChatRoom: IChatRoom<ChatRoomAccess>, ITrackedEntity
    {
        public string Topic { get; set; }

        [Key]
        [YaStringLength(ChatHubConstants.MaxChanelName, MinimumLength=3)]
        public string Name { get; set;}

        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")][JsonIgnore]
        public virtual ApplicationUser Owner { get; set; }

        [InverseProperty("Room")][JsonIgnore]
        public virtual List<ChatRoomAccess> Moderation { get; set; }
        public DateTime LatestJoinPart { get; set;}

        public DateTime DateCreated { get; set; }

        public string UserCreated { get; set; }
        public DateTime DateModified { get; set;}
        public string UserModified { get; set; }
    }
}
