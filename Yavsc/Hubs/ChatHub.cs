//
//  MyHub.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Linq;

namespace Yavsc
{

    public class ChatHub : Hub
    {
        public override Task OnConnected()
        {
            bool isAuth = false;
            string userId = null;
            if (Context.User != null)
            {
                isAuth = Context.User.Identity.IsAuthenticated;
                userId = Context.User.Identity.Name;
                var group = isAuth ?
                  "authenticated" : "anonymous";
                // Log ("Cx: " + group);
                Groups.Add(Context.ConnectionId, group);
            }
            else Groups.Add(Context.ConnectionId, "anonymous");

            Clients.Group("authenticated").notify("connected", Context.ConnectionId, userId);

              list.Add(new UserInfo
                {
                    ConnectionId = Context.ConnectionId,
                    UserName = userId
                });

            return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            string userId = Context.User?.Identity.Name;
            Clients.Group("authenticated").notify("disconnected", Context.ConnectionId, userId);
            list.Remove(list.Single(c=>c.ConnectionId==Context.ConnectionId));
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public void Send(string name, string message)
        {
            string uname = (Context.User != null) ?
              $"[{Context.User.Identity.Name}]" :
              $"(anony{name})";
            Clients.All.addMessage(uname, message);
        }


        [Authorize]
        public void SendPV(string connectionId, string message)
        {
            var sender = Context.User.Identity.Name;
            // TODO personal black|white list +
            // Contact list allowed only + 
            // only pro
            var hubCxContext = Clients.User(connectionId);
            var cli = Clients.Client(connectionId);
            cli.addPV(sender, message);
        }
        public class UserInfo
        {

            public string ConnectionId { get; set; }

            public string UserId { get; set; }

            public string UserName { get; set; }

        }

       static List<UserInfo> list = new List<UserInfo>();
        [Authorize]
        public IEnumerable<UserInfo> GetUserList()
        {
            return list;
        }
    }
}
