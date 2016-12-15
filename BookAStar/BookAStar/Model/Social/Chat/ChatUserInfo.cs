
using YavscLib;

namespace BookAStar.Model.Social.Chat
{
    public class ChatUserInfo : IChatUserInfo
    {
        public string Avatar
        {
            get; set;
        }

        public IConnection[] Connections
        {
            get; set;
        }

        public string[] Roles
        {
            get; set;
        }

        public string UserId
        {
            get; set;
        }

        public string UserName
        {
            get; set;
        }
    }
}
