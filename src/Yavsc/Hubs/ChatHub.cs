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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Models;
    using Models.Chat;
    using Yavsc.Abstract.Chat;
    using Yavsc.Services;

    public partial class ChatHub : Hub, IDisposable
    {
        // TODO externalize connexion and room presence management

        ApplicationDbContext _dbContext;
        private IConnexionManager _cxManager;
        private IStringLocalizer _localizer;
        ILogger _logger;

        public ChatHub()
        {
            var scope = Startup.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

            _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();

            var stringLocFactory = scope.ServiceProvider.GetService<IStringLocalizerFactory>();
            _localizer = stringLocFactory.Create(typeof(ChatHub));
            
            _cxManager =  scope.ServiceProvider.GetService<IConnexionManager>();
             _cxManager.SetErrorHandler ((context, error) => 
             {
                 Clients.Caller.notifyUser(NotificationTypes.Error, context, error);
             });
            _logger = loggerFactory.CreateLogger<ChatHub>();
        }

        void SetUserName(string cxId, string userName)
        {
            _cxManager.SetUserName( cxId,  userName);
        }

        public override async Task OnConnected()
        {
            bool isAuth = false;
            bool isCop = false;
            string userName = setUserName();
            if (Context.User != null)
            {
                isAuth = Context.User.Identity.IsAuthenticated;

                var group = isAuth ?
                 ChatHubConstants.HubGroupAuthenticated : ChatHubConstants.HubGroupAnonymous;
                // Log ("Cx: " + group);
                await Groups.Add(Context.ConnectionId, group);
                if (isAuth)
                {
                    _logger.LogInformation(_localizer.GetString(ChatHubConstants.LabAuthChatUser));

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
                    Clients.Group(ChatHubConstants.HubGroupFollowingPrefix + userId).notifyuser(NotificationTypes.Connected, userName, null);
                    isCop = Context.User.IsInRole(Constants.AdminGroupName) ;
                    if (isCop)
                    {
                        await Groups.Add(Context.ConnectionId, ChatHubConstants.HubGroupCops);
                    }

                    foreach (var uid in _dbContext.CircleMembers.Select(m => m.MemberId))
                    {
                        await Groups.Add(Context.ConnectionId, ChatHubConstants.HubGroupFollowingPrefix + uid);
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
                await Groups.Add(Context.ConnectionId, ChatHubConstants.HubGroupAnonymous);
            }
            _cxManager.OnConnected(userName, isCop);
            await base.OnConnected();
        }
        string setUserName()
        {
            if (Context.User != null)
                if (Context.User.Identity.IsAuthenticated)
                {
                    SetUserName(Context.ConnectionId, Context.User.Identity.Name);
                    return Context.User.Identity.Name;
                }
            anonymousSequence++;

            var queryUname = Context.Request.QueryString[ChatHubConstants.KeyParamChatUserName];

            var aname = $"{ChatHubConstants.AnonymousUserNamePrefix}{queryUname}{anonymousSequence}";
            SetUserName(Context.ConnectionId, aname);
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
                Clients.Group(ChatHubConstants.HubGroupFollowingPrefix + userId).notifyuser(NotificationTypes.DisConnected, userName, null);

                var cx = _dbContext.ChatConnection.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);
                if (cx != null)
                {
                    _dbContext.ChatConnection.Remove(cx);
                    _dbContext.SaveChanges();
                }
                else
                    _logger.LogError($"Could not remove user cx {Context.ConnectionId}");
            }
            _cxManager.Abort(Context.ConnectionId);
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
            if (_cxManager.IsConnected(candidate))
            {
                Clients.Caller.notifyUser(NotificationTypes.ExistingUserName, nickName, "aborting");
                return;
            }
            _cxManager.SetUserName( Context.ConnectionId,  candidate);
        }

        bool IsPresent(string roomName, string userName)
        {
            return _cxManager.IsPresent(roomName, userName);
        }

        public ChatRoomInfo Join(string roomName)
        {
            var roomGroupName = ChatHubConstants.HubGroupRomsPrefix + roomName;
            var user = _cxManager.GetUserName(Context.ConnectionId);
            Groups.Add(Context.ConnectionId, roomGroupName);
            ChatRoomInfo chanInfo;
            if (!_cxManager.IsPresent(roomName, user))
            {
                chanInfo = _cxManager.Join(roomName, user);
                Clients.Group(roomGroupName).notifyRoom(NotificationTypes.UserJoin, roomName, user);
            } else{
                // in case in an additional connection, 
                // one only send info on room without 
                // warning any other user.
                _cxManager.TryGetChanInfo(roomName, out chanInfo);
            } 

            return chanInfo;
        }

        [Authorize]
        public void Register([Required] string room)
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
            if (_cxManager.TryGetChanInfo(room, out chanInfo))
            {
                // TODO get and require some admin status for current user on this chan
                newroom.Topic = chanInfo.Topic;
            }
            newroom.LatestJoinPart = DateTime.Now;

            _dbContext.ChatRoom.Add(newroom);
            _dbContext.SaveChanges(user.Id);
        }
        public void KickBan([Required] string roomName, [Required]  string userName, [Required] string reason)
        {
            Kick(roomName, userName, reason);
            Ban(roomName, userName, reason);
        }
        public void Kick([Required] string roomName, [Required]  string userName, [Required]  string reason)
        {
            ChatRoomInfo chanInfo;
            var roomGroupName = ChatHubConstants.HubGroupRomsPrefix + roomName;
            if (_cxManager.TryGetChanInfo(roomName, out chanInfo))
            {
                if (!_cxManager.IsPresent(roomName,userName))
                {
                    NotifyErrorToCallerInRoom(roomName, $"{userName} was not found in {roomName}.");
                    return;
                }
                // in case of Kick returned false, being not allowed to, or for what ever other else failure, 
                // the error handler will send an error message while handling the error.
                if (!_cxManager.Kick(Context.ConnectionId, userName, roomName, reason)) return;
            }
            var ukeys = _cxManager.GetConnexionIds(userName);
            
            foreach(var ukey in ukeys)
                Groups.Remove(ukey, roomGroupName);
            Clients.Group(roomGroupName).notifyRoom(NotificationTypes.Kick, roomName, $"{userName}: {reason}");
        }

        public void Ban([Required] string roomName, [Required]  string userName, [Required]  string reason)
        {
            var cxIds = _cxManager.GetConnexionIds(userName);
            throw new NotImplementedException();
        }
        public void Gline([Required] string userName, [Required]  string reason)
        {
            throw new NotImplementedException();
        }
       

        public void Part([Required] string roomName, [Required]  string reason)
        {
            if (_cxManager.Part(Context.ConnectionId,  roomName,   reason))
             {
                var roomGroupName = ChatHubConstants.HubGroupRomsPrefix + roomName;
                var group = Clients.Group(roomGroupName);
                var userName = _cxManager.GetUserName(Context.ConnectionId);
                group.notifyRoom(NotificationTypes.UserPart, roomName, $"{userName}: {reason}");
                Groups.Remove(Context.ConnectionId, roomGroupName);
             }
             else {
                 _logger.LogError("Could not part");
             }
        }

        void NotifyErrorToCallerInRoom(string room, string reason)
        {
            Clients.Caller.notifyUser(NotificationTypes.Error, room, reason);
            _logger.LogError($"NotifyErrorToCallerInRoom: {room}, {reason}");
        }

        public void Send([Required] string roomName, [Required]  string message)
        {
            var groupname = ChatHubConstants.HubGroupRomsPrefix + roomName;
            ChatRoomInfo chanInfo ;
            if (!_cxManager.TryGetChanInfo(roomName, out chanInfo))
            {
                var noChanMsg = _localizer.GetString(ChatHubConstants.LabNoSuchChan).ToString();
                Clients.Caller.notifyUser(NotificationTypes.Error, roomName, noChanMsg);
                return;
            }
            var userName = _cxManager.GetUserName(Context.ConnectionId);
            if (!_cxManager.IsPresent(roomName, userName))
            {
                var notSentMsg = _localizer.GetString(ChatHubConstants.LabnoJoinNoSend).ToString();
                Clients.Caller.notifyUser(NotificationTypes.Error, roomName, notSentMsg);
                return;
            }
            Clients.Group(groupname).addMessage(userName, roomName, message);
        }

        [Authorize]
        public void SendPV([Required] string userName, [Required]  string message)
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
            var cxIds = _cxManager.GetConnexionIds(userName);

            foreach (var connectionId in cxIds)
            {
                var cli = Clients.Client(connectionId);
                cli.addPV(Context.User.Identity.Name, message);
            }
        }

        [Authorize]

        public void SendStream([Required] string connectionId, long streamId, [Required]  string message)
        {
            var sender = Context.User.Identity.Name;
            var cli = Clients.Client(connectionId);
            cli.addStreamInfo(sender, streamId, message);
        }

        
    }
}
