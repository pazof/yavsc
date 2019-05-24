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

using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Concurrent;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Yavsc
{
    using Models;
    using Models.Chat;

    public class ChatHub : Hub, IDisposable
    {
        ApplicationDbContext _dbContext;
        ILogger _logger;
        public static ConcurrentDictionary<string, string> ChatUserNames = new ConcurrentDictionary<string, string>();

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
                    _logger.LogInformation("Authenticated chat user");

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
                    // TODO ChatHubConnectioinFlags
                }
                else
                {
                    // FIXME is this line reached ?
                    _logger.LogInformation("Anonymous chat user (first use case)");
                    throw new NotSupportedException();
                }
            }
            else
            {

                await Groups.Add(Context.ConnectionId, Constants.HubGroupAnonymous);
            }
            // TODO only notify followers
            Clients.Group(Constants.HubGroupAuthenticated).notify(NotificationTypes.Connected, userName);
            await base.OnConnected();
        }
        string setUserName()
        {
            if (Context.User != null)
                if (Context.User.Identity.IsAuthenticated)
                {
                    ChatUserNames[Context.ConnectionId] = Context.User.Identity.Name;
                    _logger.LogInformation($"chat user name set to : {Context.User.Identity.Name}");
                    return Context.User.Identity.Name;
                }
            anonymousSequence++;

            var queryUname = Context.Request.QueryString[Constants.KeyParamChatUserName];

            var aname = $"{Constants.AnonymousUserNamePrefix}{queryUname}{anonymousSequence}";
            ChatUserNames[Context.ConnectionId] = aname;
            _logger.LogInformation($"Anonymous chat user name set to : {aname}");
            return aname;
        }

        static long anonymousSequence = 0;

        public override Task OnDisconnected(bool stopCalled)
        {
            string userName = Context.User?.Identity.Name;
            Clients.Group("authenticated").notify(NotificationTypes.DisConnected, userName);
            if (userName != null)
            {
                var cx = _dbContext.ChatConnection.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);
                if (cx != null)
                {
                    if (stopCalled)
                    {
                        var user = _dbContext.Users.Single(u => u.UserName == userName);
                        user.Connections.Remove(cx);
                        ChatUserNames[Context.ConnectionId] = null;
                    }
                    else
                    {
                        cx.Connected = false;
                    }
                    _dbContext.SaveChanges();
                }
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            if (Context.User != null) if (Context.User.Identity.IsAuthenticated)
                {
                    var userName = Context.User.Identity.Name;
                    var user = _dbContext.Users.FirstOrDefault(u => u.UserName == userName);
                    if (user == null)
                        _logger.LogWarning($"null user with <{userName}> & Context.User.Identity.IsAuthenticated");
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
                    Clients.Group("authenticated").notify(NotificationTypes.Reconnected, userName);
                }
            return base.OnReconnected();
        }

        static ConcurrentDictionary<string, ChatRoomInfo> Channels = new ConcurrentDictionary<string, ChatRoomInfo>();

        public class ChatRoomInfo
        {
            public string Name;
            public Dictionary<string, string> Users = new Dictionary<string, string>();
            public string Topic;
        }

        public void Nick(string nickName)
        {
            var candidate = "?" + nickName;
            if (ChatUserNames.Any(u => u.Value == candidate))
            {
                Clients.Caller.notify(NotificationTypes.ExistingUserName, nickName);
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
            _logger.LogInformation("a client for " + roomName);
            var userName = ChatUserNames[Context.ConnectionId];
            _logger.LogInformation($" chat user : {userName}");
            var roomGroupName = "room_" + roomName;

            ChatRoomInfo chanInfo;
            if (Channels.ContainsKey(roomName))
            {
                if (Channels.TryGetValue(roomName, out chanInfo))
                {
                    _logger.LogInformation("room is avaible.");
                    if (chanInfo.Users.ContainsKey(Context.ConnectionId))
                        _logger.LogWarning("user already joint.");
                    else
                    {
                        chanInfo.Users.Add(Context.ConnectionId, userName);

                        Groups.Add(Context.ConnectionId, roomGroupName);
                    }
                    Clients.Caller.joint(chanInfo);
                    Clients.Group("room_" + roomName).notify(NotificationTypes.UserJoin, userName);

                    _logger.LogInformation("exiting ok.");
                    return chanInfo;
                }
                else
                {
                    _logger.LogInformation("room seemd to be avaible ... but we could get no info on it.");
                    Clients.Caller.notify(NotificationTypes.Error, "join get chan failed ...");
                    return null;
                }
            }
            // chan was almost empty
            _logger.LogInformation("joining empty chan.");

            var room = _dbContext.ChatRoom.FirstOrDefault(r => r.Name == roomName);

            chanInfo = new ChatRoomInfo();
            chanInfo.Users.Add(Context.ConnectionId, userName);

            if (room != null)
            {
                _logger.LogInformation("existent room.");
                chanInfo.Topic = room.Topic;
                chanInfo.Name = room.Name;
            }
            else
            { // a first join, we create it.
                _logger.LogInformation("room creation.");
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
                Clients.Caller.notify(NotificationTypes.Error, "already registered.");
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
                var roomGroupName = "room_" + roomName;
                Groups.Remove(Context.ConnectionId, roomGroupName);
                var group = Clients.Group(roomGroupName);
                var username = ChatUserNames[Context.ConnectionId];
                group.notify(NotificationTypes.UserPart, $"{roomName} {username} ({reason})");

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
                NotifyNotJoint(roomName, "no such room");
            }
        }

        void NotifyNotJoint(string room, string reason)
        {
            Clients.Caller.notify(NotificationTypes.Error, $"{room} not joint: {reason}");
        }


        public void Send(string roomName, string message)
        {
            var groupname = "room_" + roomName;
            ChatRoomInfo chanInfo;
            if (Channels.TryGetValue(roomName, out chanInfo))
            {
                if (!chanInfo.Users.ContainsKey(Context.ConnectionId))
                {
                    var notSentMsg = $"could not send to channel ({roomName}) (not joint)";
                    Clients.Caller.notify(NotificationTypes.Error, notSentMsg);
                    return;
                }
                string uname = ChatUserNames[Context.ConnectionId];
                Clients.Group(groupname).addMessage(uname, roomName, message);
                _logger.LogInformation($"{uname} sent message {message} to {roomName}");
            }
            else
            {
                var noChanMsg = $"could not send to channel ({roomName}) (no such chan)";
                Clients.Caller.notify(NotificationTypes.Error, noChanMsg);
                _logger.LogWarning(noChanMsg);
                return;
            }

        }

        [Authorize]
        public void SendPV(string userName, string message)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return;

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
                        Clients.Caller.notify(NotificationTypes.PrivateMessageDenied, userName);
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

        public void Abort()
        {
            var cx = _dbContext.ChatConnection.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (cx != null)
            {
                _dbContext.ChatConnection.Remove(cx);
                _dbContext.SaveChanges();
            }
        }
    }
}
