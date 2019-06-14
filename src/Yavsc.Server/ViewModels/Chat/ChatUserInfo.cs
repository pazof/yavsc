using System.Collections.Generic;
using Yavsc.Models.Chat;

namespace Yavsc.ViewModels.Chat { 

        public class ChannelShortInfo {
            public string RoomName {get; set;}
            public string Topic { get; set; }
        }
        
public class ChatUserInfo : IChatUserInfo
        {

            public List<ChatConnection> Connections { get; set; }

            public string UserId { get; set; }

            public string UserName { get; set; }

            public string Avatar { get; set; }

            public string[] Roles { get; set; }

        }

    public interface IChatUserInfo
    {
        List<ChatConnection> Connections { get; set; }
        string UserId { get; set; }

        string UserName { get; set; }

        string Avatar { get; set; }

        string[] Roles { get; set; }
    }
}
