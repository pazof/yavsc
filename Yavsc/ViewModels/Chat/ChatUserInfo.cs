using System.Collections.Generic;
using Yavsc.Model.Chat;

namespace Yavsc.ViewModels.Chat { 
public class ChatUserInfo
        {

            public List<Connection> Connections { get; set; }

            public string UserId { get; set; }

            public string UserName { get; set; }

            public string Avatar { get; set; }

        }
}
