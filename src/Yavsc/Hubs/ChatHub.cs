//
//  ChatHub.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016-2019 GNU GPL
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

using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace Yavsc
{
    using Models;
    using Models.Chat;

    public partial class ChatHub : Hub, IDisposable
    {
        ApplicationDbContext _dbContext;
        private IStringLocalizer _localizer;
        ILogger _logger;
        public static ConcurrentDictionary<string, string> ChatUserNames = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, ChatRoomInfo> Channels = new ConcurrentDictionary<string, ChatRoomInfo>();

        public ChatHub()
        {
            var scope = Startup.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

            _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();

            var stringLocFactory = scope.ServiceProvider.GetService<IStringLocalizerFactory>();
            _localizer = stringLocFactory.Create(typeof(ChatHub));
            _logger = loggerFactory.CreateLogger<ChatHub>();
        }

        public override async Task OnConnected()
        {
            bool isAuth = false;
            string userName = setUserName();
            if (Context.User != null)
            {
                isAuth = Context.User.Identity.IsAuthenticated;

                var group = isAuth ?
                 Constants.HubGroupAuthenticated : Constants.HubGroupAnonymous;
                // Log ("Cx: " + group);
                await Groups.Add(Context.ConnectionId, group);
                if (isAuth)
                {
                    _logger.LogInformation(_localizer.GetString(Constants.LabAuthChatUser));

                    var userId = _dbContext.Users.First(u => u.UserName == userName).Id;

                    var userHadConnections = _dbContext.ChatConnection.Any(accx => accx.ConnectionId == Context.ConnectionId);

                    if (userHadConnections)
                    {
                        var ccx = _dbContext.ChatConnection.First(c => c.ConnectionId == Context.ConnectionId);
                        ccx.Connected = true;
                    }
                    else
                        _dbContext.ChatConnection.Add(new ChatConnection
                        {
                            ApplicationUserId = userId,
                            ConnectionId = Context.ConnectionId,
                            UserAgent = Context.Request.Headers["User-Agent"],
                            Connected = true
                        });
                    _dbContext.SaveChanges();
                    Clients.Group(Constants.HubGroupFollowingPrefix + userId).notifyuser(NotificationTypes.Connected, userName, null);

                    foreach (var uid in _dbContext.CircleMembers.Select(m => m.MemberId))
                    {
                        await Groups.Add(Context.ConnectionId, Constants.HubGroupFollowingPrefix + uid);
                    }
                }
                else
                {
                    // this line isn't reached: Context.User != null <=> Context.User.Identity.IsAuthenticated
                    throw new NotSupportedException("Context.User != null && no auth");
                }
            }
            else
            {
                await Groups.Add(Context.ConnectionId, Constants.HubGroupAnonymous);
            }
            await base.OnConnected();
        }
        string setUserName()
        {
            if (Context.User != null)
                if (Context.User.Identity.IsAuthenticated)
                {
                    ChatUserNames[Context.ConnectionId] = Context.User.Identity.Name;
                    return Context.User.Identity.Name;
                }
            anonymousSequence++;

            var queryUname = Context.Request.QueryString[Constants.KeyParamChatUserName];

            var aname = $"{Constants.AnonymousUserNamePrefix}{queryUname}{anonymousSequence}";
            ChatUserNames[Context.ConnectionId] = aname;
            return aname;
        }

        static long anonymousSequence = 0;

        public override Task OnDisconnected(bool stopCalled)
        {
            string userName = Context.User?.Identity.Name;
            if (userName != null)
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.UserName == userName);
                var userId = user.Id;
                Clients.Group(Constants.HubGroupFollowingPrefix + userId).notifyuser(NotificationTypes.DisConnected, userName, null);

                var cx = _dbContext.ChatConnection.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);
                if (cx != null)
                {
                    _dbContext.ChatConnection.Remove(cx);
                    _dbContext.SaveChanges();
                }
                else
                    _logger.LogError($"Could not remove user cx {Context.ConnectionId}");
            }
            Abort();
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            if (Context.User != null) if (Context.User.Identity.IsAuthenticated)
                {
                    var userName = Context.User.Identity.Name;
                    var user = _dbContext.Users.FirstOrDefault(u => u.UserName == userName);
                    var userId = user.Id;
                    var userHadConnections = _dbContext.ChatConnection.Any(accx => accx.ConnectionId == Context.ConnectionId);

                    if (userHadConnections)
                    {
                        var ccx = _dbContext.ChatConnection.First(c => c.ConnectionId == Context.ConnectionId);
                        ccx.Connected = true;
                    }
                    else
                        _dbContext.ChatConnection.Add(new ChatConnection
                        {
                            ApplicationUserId = userId,
                            ConnectionId = Context.ConnectionId,
                            UserAgent = Context.Request.Headers["User-Agent"],
                            Connected = true
                        });
                    _dbContext.SaveChanges();
                    Clients.Group("authenticated").notifyUser(NotificationTypes.Reconnected, userName, "reconnected");
                }
            return base.OnReconnected();
        }

        public void Nick(string nickName)
        {
            var candidate = "?" + nickName;
            if (ChatUserNames.Any(u => u.Value == candidate))
            {
                Clients.Caller.notifyUser(NotificationTypes.ExistingUserName, nickName, "aborting");
                return;
            }
            ChatUserNames[Context.ConnectionId] = "?" + nickName;
        }

        public void JoinAsync(string roomName)
        {
            var info = Join(roomName);
            Clients.Caller.joint(info);
        }

        public ChatRoomInfo Join(string roomName)
        {
            var userName = ChatUserNames[Context.ConnectionId];
            var roomGroupName = Constants.HubGroupRomsPrefix + roomName;

            ChatRoomInfo chanInfo;
            if (Channels.ContainsKey(roomName))
            {
                if (Channels.TryGetValue(roomName, out chanInfo))
                {
                    if (chanInfo.Users.ContainsKey(Context.ConnectionId))
                        _logger.LogWarning("user already joint.");
                    else
                    {
                        chanInfo.Users.Add(Context.ConnectionId, userName);
                        Groups.Add(Context.ConnectionId, roomGroupName);
                        Clients.Caller.joint(chanInfo);
                        Clients.Group(Constants.HubGroupRomsPrefix + roomName).notifyRoom(NotificationTypes.UserJoin, roomName, userName);
                        return chanInfo;
                    }
                    return null;
                }
                else
                {
                    _logger.LogError("room seemd to be avaible ... but we could get no info on it.");
                    Clients.Caller.notifyRoom(NotificationTypes.Error, roomName, "join get chan failed ...");
                    return null;
                }
            }
            var room = _dbContext.ChatRoom.FirstOrDefault(r => r.Name == roomName);
            chanInfo = new ChatRoomInfo();
            chanInfo.Users.Add(Context.ConnectionId, userName);

            if (room != null)
            {
                chanInfo.Topic = room.Topic;
                chanInfo.Name = room.Name;
            }
            else
            { // a first join, we create it.
                chanInfo.Name = roomName;
                chanInfo.Topic = "<just created>";
            }

            if (Channels.TryAdd(roomName, chanInfo))
            {
                Groups.Add(Context.ConnectionId, roomGroupName);
                return (chanInfo);
            }
            else _logger.LogError("Chan create failed unexpectly...");
            return null;
        }

        [Authorize]
        public void Register(string room)
        {
            var existent = _dbContext.ChatRoom.Any(r => r.Name == room);
            if (existent)
            {
                Clients.Caller.notifyUser(NotificationTypes.Error, room, "already registered.");
                return;
            }
            string userName = Context.User.Identity.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.UserName == userName);
            var newroom = new ChatRoom { Name = room, OwnerId = user.Id };
            ChatRoomInfo chanInfo;
            if (Channels.TryGetValue(room, out chanInfo))
            {
                // TODO get and require some admin status for current user on this chan
                newroom.Topic = chanInfo.Topic;
            }
            newroom.LatestJoinPart = DateTime.Now;

            _dbContext.ChatRoom.Add(newroom);
            _dbContext.SaveChanges(user.Id);
        }

        /** TODO chan register on server command 
               room = new ChatRoom { Name = roomName, OwnerId = uid  };
               _dbContext.ChatRoom.Add(room);
               _dbContext.SaveChanges(uid);
               room.LatestJoinPart = DateTime.Now;
               chanInfo.Topic = room.Topic;
                */


        public void Part(string roomName, string reason)
        {
            ChatRoomInfo chanInfo;
            if (Channels.TryGetValue(roomName, out chanInfo))
            {
                var roomGroupName = Constants.HubGroupRomsPrefix + roomName;
                if (!chanInfo.Users.ContainsKey(Context.ConnectionId))
                {
                    NotifyRoomError(roomName, "you didn't join.");
                    return;
                }
                Groups.Remove(Context.ConnectionId, roomGroupName);
                var group = Clients.Group(roomGroupName);
                var username = ChatUserNames[Context.ConnectionId];
                group.notifyRoom(NotificationTypes.UserPart, roomName, $"{username}: {reason}");

                chanInfo.Users.Remove(Context.ConnectionId);
                ChatRoomInfo deadchanInfo;
                if (chanInfo.Users.Count == 0)
                    if (Channels.TryRemove(roomName, out deadchanInfo))
                    {
                        var room = _dbContext.ChatRoom.FirstOrDefault(r => r.Name == roomName);
                        room.LatestJoinPart = DateTime.Now;
                        _dbContext.SaveChanges();
                    }
            }
            else
            {
                NotifyRoomError(roomName, $"could not join: no such room");
            }
        }

        void NotifyRoomError(string room, string reason)
        {
            Clients.Caller.notifyUser(NotificationTypes.Error, room, reason);
        }


        public void Send(string roomName, string message)
        {
            var groupname = Constants.HubGroupRomsPrefix + roomName;
            ChatRoomInfo chanInfo;
            if (Channels.TryGetValue(roomName, out chanInfo))
            {
                if (!chanInfo.Users.ContainsKey(Context.ConnectionId))
                {
                    var notSentMsg = $"could not send to channel ({roomName}) (not joint)";
                    Clients.Caller.notifyUser(NotificationTypes.Error, roomName, notSentMsg);
                    return;
                }
                string uname = ChatUserNames[Context.ConnectionId];
                Clients.Group(groupname).addMessage(uname, roomName, message);
            }
            else
            {
                var noChanMsg = $"could not send to channel ({roomName}) (no such chan)";
                Clients.Caller.notifyUser(NotificationTypes.Error, roomName, noChanMsg);
                return;
            }

        }

        [Authorize]
        public void SendPV(string userName, string message)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                Clients.Caller.notifyUser(NotificationTypes.Error, "none!", "specify an user.");
                return;
            }

            if (userName[0] != '?')
                if (!Context.User.IsInRole(Constants.AdminGroupName))
                {
                    var bl = _dbContext.BlackListed
                    .Include(r => r.User)
                    .Include(r => r.Owner)
                    .Where(r => r.User.UserName == Context.User.Identity.Name && r.Owner.UserName == userName)
                    .Select(r => r.OwnerId);

                    if (bl.Count() > 0)
                    {
                        Clients.Caller.notifyUser(NotificationTypes.PrivateMessageDenied, userName, "you are black listed.");
                        return;
                    }
                }
            var cxIds = ChatUserNames.Where(name => name.Value == userName).Select(name => name.Key);

            foreach (var connectionId in cxIds)
            {
                var cli = Clients.Client(connectionId);
                cli.addPV(Context.User.Identity.Name, message);
            }
        }

        [Authorize]

        public void SendStream(string connectionId, long streamId, string message)
        {
            var sender = Context.User.Identity.Name;
            var cli = Clients.Client(connectionId);
            cli.addStreamInfo(sender, streamId, message);
        }

        void Abort()
        {
            string cxId;
            if (!ChatUserNames.TryRemove(Context.ConnectionId, out cxId))
                _logger.LogError($"Could not remove user cx {Context.ConnectionId}");

        }
    }
}
