using System.Collections.Generic;
using Yavsc.Model.Chat;

namespace Yavsc.ViewModels.Chat { 
public class ChatUserInfo : IChatUserInfo
        {

            public List<Connection> Connections { get; set; }

            public string UserId { get; set; }

            public string UserName { get; set; }

            public string Avatar { get; set; }

            public string[] Roles { get; set; }

        }

    public interface IChatUserInfo
    {
        List<Connection> Connections { get; set; }
        string UserId { get; set; }

        string UserName { get; set; }

        string Avatar { get; set; }

        string[] Roles { get; set; }
    }
}
