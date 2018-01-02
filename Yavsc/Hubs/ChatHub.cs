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
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Yavsc
{
    using Models;
    using Models.Chat;

    public class ChatHub : Hub
    {

        public override Task OnConnected()
        {
            bool isAuth = false;
            string userName = null;
            if (Context.User != null)
            {
                isAuth = Context.User.Identity.IsAuthenticated;
                userName = Context.User.Identity.Name;
                var group = isAuth ?
                  "authenticated" : "anonymous";
                // Log ("Cx: " + group);
                Groups.Add(Context.ConnectionId, group);
                if (isAuth)
                {
                    using (var db = new ApplicationDbContext()) {
                        var user = db.Users.Single(u => u.UserName == userName);
                        if (user.Connections==null)
                            user.Connections = new List<ChatConnection>();
                        user.Connections.Add(new ChatConnection
                        {
                            ConnectionId = Context.ConnectionId,
                            UserAgent = Context.Request.Headers["User-Agent"],
                            Connected = true
                        });
                        db.SaveChanges();
                    }
                    
                }
            }
            else Groups.Add(Context.ConnectionId, "anonymous");

            Clients.Group("authenticated").notify("connected", Context.ConnectionId, userName);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            
            string userName = Context.User?.Identity.Name;
            Clients.Group("authenticated").notify("disconnected", Context.ConnectionId, userName);
            if (userName != null)
            {
                using (var db = new ApplicationDbContext()) {
                    var cx = db.Connections.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);
                    if (cx != null)
                    {
                        if (stopCalled)
                        {
                            var user = db.Users.Single(u => u.UserName == userName);
                            user.Connections.Remove(cx);
                        }
                        else
                        {
                            cx.Connected = false;
                        }
                        db.SaveChanges();
                    }
                }
            }
            
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string userName = Context.User?.Identity.Name;
            if (userName != null)
            {
                using (var db = new ApplicationDbContext()) {
                    var user = db.Users.Single(u => u.UserName == userName);

                    if (user.Connections==null) user.Connections = new List<ChatConnection>();

                    
                        var cx = user.Connections.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);
                        if (cx != null)
                        {
                            cx.Connected = true;
                            db.SaveChanges();
                        }
                        else cx = new ChatConnection { ConnectionId = Context.ConnectionId,
                            UserAgent = Context.Request.Headers["User-Agent"],
                            Connected = true };
                }
            }

            return base.OnReconnected();
        }

        public void Send(string name, string message)
        {
            string uname = (Context.User != null) ?
              $"[{Context.User.Identity.Name}]" :
              $"({name})";
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
        public void Abort()
        {
            using (var db = new ApplicationDbContext()) {
                var cx = db.Connections.SingleOrDefault(c=>c.ConnectionId == Context.ConnectionId);
                if (cx!=null) {
                    db.Connections.Remove(cx);
                    db.SaveChanges(); 
                }
            }
                
        }

    }
}
