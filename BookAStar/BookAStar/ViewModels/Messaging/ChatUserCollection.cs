using BookAStar.Data;
using BookAStar.Model.Social.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BookAStar.ViewModels.Messaging
{
    public class ChatUserCollection : RemoteEntityRO<ChatUserInfo, string>
    {
        public ChatUserCollection() : base ("chat/users", u=>u.UserId)
        {

        }

        public override void Merge(ChatUserInfo item)
        {
            var key = GetKey(item);
            var existent = this.FirstOrDefault(u => u.UserId == key);
            if (existent != null) {
                existent.UserName = item.UserName;
                existent.Roles = item.Roles;
                existent.Avatar = item.Avatar;
                existent.Connections = item.Connections;
            }
            else Add(item);
        }

    }
}
