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
    using System;
    using Microsoft.AspNet.Authorization;
    using Microsoft.Data.Entity;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Models;
    using Models.Chat;

    public class ChatHub : Hub, IDisposable
    {
        ApplicationDbContext _dbContext;
        ILogger _logger;

        public ChatHub()
        { 
            var scope = Startup.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<ChatHub>();
        }

        public override async Task OnConnected()
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
                await Groups.Add(Context.ConnectionId, group);
                if (isAuth)
                {
                    var user = _dbContext.Users.Include(u=>u.Connections).Single(u => u.UserName == userName);
                    if (user.Connections==null)
                        user.Connections = new List<ChatConnection>();
                    var ucx = user.Connections.FirstOrDefault(c=>c.ConnectionId == Context.ConnectionId);
                    if (ucx==null)
                        user.Connections.Add(new ChatConnection
                        {
                            ConnectionId = Context.ConnectionId,
                            UserAgent = Context.Request.Headers["User-Agent"],
                            Connected = true
                        });
                    else {
                        ucx.Connected = true;
                    }
                    _dbContext.SaveChanges(); 

                    Clients.CallerState.BlackListedBy = await _dbContext.BlackListed.Where(r=>r.UserId == user.Id).Select(r=>r.OwnerId).ToArrayAsync();
                    
                }
            }
            else await Groups.Add(Context.ConnectionId, "anonymous");

            Clients.Group("authenticated").notify("connected", Context.ConnectionId, userName);

            await base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            
            string userName = Context.User?.Identity.Name;
            Clients.Group("authenticated").notify("disconnected", Context.ConnectionId, userName);
            if (userName != null)
            {
                using (var db = new ApplicationDbContext()) {
                    var cx = db.ChatConnection.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);
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
                
                var user = _dbContext.Users.Single(u => u.UserName == userName);

                if (user.Connections==null) user.Connections = new List<ChatConnection>();

                
                    var cx = user.Connections.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);
                    if (cx != null)
                    {
                        cx.Connected = true;
                        _dbContext.SaveChanges();
                    }
                    else cx = new ChatConnection { ConnectionId = Context.ConnectionId,
                        UserAgent = Context.Request.Headers["User-Agent"],
                        Connected = true };
            }

            return base.OnReconnected();
        }

        public void Send(string name, string message)
        {
            string uname = (Context.User != null) ?
              $"[{Context.User.Identity.Name}]" :
              $"?{name}";
            Clients.All.addMessage(uname, message);
        }



        [Authorize]
        public void SendPV(string connectionId, string message)
        {
            if (Clients.CallerState.BlackListedBy!=null) 
                foreach (string destId in Clients.CallerState.BlackListedBy)
                {
                    if (_dbContext.ChatConnection.Any(c => c.ConnectionId == connectionId && c.ApplicationUserId == destId ))
                    {
                        _logger.LogInformation($"PV aborted by black list");
                        Clients.Caller.send("denied");
                        return ;
                    }
                }
            var cli = Clients.Client(connectionId);
            cli.addPV(Context.User.Identity.Name, message);
        }

        private async Task<bool> AllowPv(string destConnectionId)
        {
            if (Context.User.IsInRole(Constants.BlogModeratorGroupName))

            if (Context.User.IsInRole(Constants.BlogModeratorGroupName)
            || Context.User.IsInRole(Constants.AdminGroupName))
                return true;
            if (!Context.User.Identity.IsAuthenticated)
                return false;
            string senderId = (await _dbContext.ChatConnection.SingleAsync (c=>c.ConnectionId == Context.ConnectionId)).ApplicationUserId;
            
             
            if (_dbContext.Ban.Any(b=>b.TargetId == senderId)) return false;
            var destChatUser = await _dbContext.ChatConnection.SingleAsync (c=>c.ConnectionId == destConnectionId);

            if (_dbContext.BlackListed.Any(b=>b.OwnerId == destChatUser.ApplicationUserId && b.UserId == senderId)) return false;
            var destUser = await _dbContext.Performers.FirstOrDefaultAsync( u=> u.PerformerId == destChatUser.ApplicationUserId);
            return destUser?.AcceptPublicContact ?? true;
        }

        public void SendStream(string connectionId, long streamId, string message)
        {
            var sender = Context.User.Identity.Name;
            // TODO personal black|white list +
            // Contact list allowed only + 
            // only pro
            var hubCxContext = Clients.User(connectionId);
            var cli = Clients.Client(connectionId);
            cli.addStreamInfo(sender, streamId, message);
        }

        public void Abort()
        {
            var cx = _dbContext .ChatConnection.SingleOrDefault(c=>c.ConnectionId == Context.ConnectionId);
            if (cx!=null) {
                _dbContext.ChatConnection.Remove(cx);
                _dbContext.SaveChanges(); 
            }
        }
    }
}
