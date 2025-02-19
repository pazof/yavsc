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
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace Yavsc
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Models.Chat;
    using Yavsc.Abstract.Chat;
    using Yavsc.Services;
    public partial class ChatHub : Hub, IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConnexionManager _cxManager;
        private readonly IStringLocalizer _localizer;
        private readonly ILogger _logger;

        public HubInputValidator InputValidator { get; }

        public ChatHub(ApplicationDbContext dbContext, 
        ILoggerFactory loggerFactory, 
        IStringLocalizerFactory stringLocalizerFactory, 
        IConnexionManager connexionManager)
        {
            _dbContext = dbContext;
            _localizer = stringLocalizerFactory.Create(typeof(ChatHub));
            
            _cxManager =  connexionManager;
             _cxManager.SetErrorHandler ((context, error) => 
             {
                 NotifyUser(NotificationTypes.Error, context, error);
             });
            _logger = loggerFactory.CreateLogger<ChatHub>();
            InputValidator = new HubInputValidator { NotifyUser = async (type, target, msg) => await this.NotifyUser(type, target, msg) };
        }

        void SetUserName(string cxId, string userName)
        {
            _cxManager.SetUserName(cxId,  userName);
        }

        public override async Task OnConnectedAsync()
        {
            bool isAuth = Context.User?.Identity?.IsAuthenticated ?? false;
            bool isCop = false;
            string userName = setUserName();
            if (isAuth)
            {

                var group = isAuth ?
                 ChatHubConstants.HubGroupAuthenticated : ChatHubConstants.HubGroupAnonymous;
                // Log ("Cx: " + group);
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
                _logger.LogInformation(_localizer.GetString(ChatHubConstants.LabAuthChatUser));

                var userId = _dbContext.Users.First(u => u.UserName == Context.User.Identity.Name).Id;

                await Clients.Group(ChatHubConstants.HubGroupFollowingPrefix + userId).SendAsync("notifyUser",  NotificationTypes.Connected, userName, null);
                isCop = Context.User.IsInRole(Constants.AdminGroupName) ;
                if (isCop)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, ChatHubConstants.HubGroupCops);
                }

                foreach (var uid in _dbContext.CircleMembers.Select(m => m.MemberId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, ChatHubConstants.HubGroupFollowingPrefix + uid);
                }
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ChatHubConstants.HubGroupAnonymous);
            }
            _cxManager.OnConnected(Context.ConnectionId, isCop);
            await base.OnConnectedAsync();
        }

        string setUserName(string queryUname = "anon")
        {
            if (Context.User != null)
                if (Context.User.Identity.IsAuthenticated)
                {
                    SetUserName(Context.ConnectionId, Context.User.Identity.Name);
                    return Context.User.Identity.Name;
                }
            anonymousSequence++;
            var aname = $"{ChatHubConstants.AnonymousUserNamePrefix}{queryUname}{anonymousSequence}";
            SetUserName(Context.ConnectionId, aname);
            return aname;
        }

        static long anonymousSequence = 0;

        public override async Task OnDisconnectedAsync(Exception ?ex)
        {
            string userName = Context.User?.Identity.Name;
            if (userName != null)
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.UserName == userName);
                var userId = user.Id;
                await Clients.Group(ChatHubConstants.HubGroupFollowingPrefix + userId).SendAsync("notifyUser", NotificationTypes.DisConnected, userName, null);

                _cxManager.OnDisctonnected(Context.ConnectionId);
            }
              await base.OnDisconnectedAsync(ex);
        }

     
        public async Task Nick(string nickName)
        {
            if (!InputValidator.ValidateUserName(nickName)) return;

            var candidate = "?" + nickName;
            if (_cxManager.IsConnected(candidate))
            {
                await NotifyUser(NotificationTypes.ExistingUserName, nickName, "aborting");
                return;
            }
            _cxManager.SetUserName( Context.ConnectionId,  candidate);
        }

        bool IsPresent(string roomName, string userName)
        {
            return _cxManager.IsPresent(roomName, userName);
        }

        public async Task<ChatRoomInfo> Join(string roomName)
        {
            _logger.LogInformation($"Join:{roomName}"); 
            if (!InputValidator.ValidateRoomName(roomName))
            {
               _logger.LogError("!InputValidator.ValidateRoomName(roomName)"); 
                return null;
            }

            var roomGroupName = ChatHubConstants.HubGroupRomsPrefix + roomName;
            var user = _cxManager.GetUserName(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomGroupName);
            ChatRoomInfo chanInfo;
            if (!_cxManager.IsPresent(roomName, user))
            {
                _logger.LogInformation($"Joining"); 
                chanInfo = _cxManager.Join(roomName, Context.ConnectionId);
                await Clients.Group(roomGroupName).SendAsync("notifyRoom", NotificationTypes.UserJoin, roomName, user);
            } else {
                _logger.LogInformation($"already present"); 
                // in case in an additional connection, 
                // one only send info on room without 
                // warning any other user.
                _cxManager.TryGetChanInfo(roomName, out chanInfo);
            } 

            _logger.LogInformation($"returning chan info"); 
            await Clients.Caller.SendAsync("joint", chanInfo);
            return chanInfo;
        }

        [Authorize]
        public void Register(string room)
        {
            if (!InputValidator.ValidateRoomName(room)) return ;
            var existent = _dbContext.ChatRoom.Any(r => r.Name == room);
            if (existent)
            {
                NotifyUserInRoom(NotificationTypes.Error, room, "already registered.");
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

        [Authorize]
        public void KickBan(string roomName,  string userName, string reason)
        {
            if (!InputValidator.ValidateRoomName(roomName)) return ;
            if (!InputValidator.ValidateUserName(userName)) return ;
            if (!InputValidator.ValidateReason(reason)) return;
            Kick(roomName, userName, reason);
            Ban(roomName, userName, reason);
        }

        [Authorize]
        public async Task Kick(string roomName,  string userName,  string reason)
        {
            if (!InputValidator.ValidateRoomName(roomName)) return ;
            if (!InputValidator.ValidateUserName(userName)) return ;
            if (!InputValidator.ValidateReason(reason)) return;
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
            if (ukeys!=null) foreach(var ukey in ukeys)
                await Groups.RemoveFromGroupAsync(ukey, roomGroupName);
            await Clients.Group(roomGroupName).SendAsync("notifyRoom", NotificationTypes.Kick, roomName, $"{userName}: {reason}");
        }

        [Authorize]
        public void Ban(string roomName,  string userName,  string reason)
        {
            if (!InputValidator.ValidateRoomName(roomName)) return ;
            if (!InputValidator.ValidateUserName(userName)) return ;
            if (!InputValidator.ValidateReason(reason)) return;
            var cxIds = _cxManager.GetConnexionIds(userName);
            throw new NotImplementedException();
        }

        [Authorize]
        public void Gline(string userName,  string reason)
        {
            if (!InputValidator.ValidateUserName(userName)) return ;
            if (!InputValidator.ValidateReason(reason)) return;
            throw new NotImplementedException();
        }
       
        public void Part(string roomName,  string reason)
        {
            if (!InputValidator.ValidateRoomName(roomName)) return ;
            if (!InputValidator.ValidateReason(reason)) return;
            if (_cxManager.Part(Context.ConnectionId,  roomName,   reason))
             {
                var roomGroupName = ChatHubConstants.HubGroupRomsPrefix + roomName;
                var group = Clients.Group(roomGroupName);
                var userName = _cxManager.GetUserName(Context.ConnectionId);
                group.SendAsync("notifyRoom", NotificationTypes.UserPart, roomName, $"{userName}: {reason}");
                Groups.RemoveFromGroupAsync(Context.ConnectionId, roomGroupName);
             }
             else {
                 _logger.LogError("Could not part");
             }
        }

        void NotifyErrorToCallerInRoom(string room, string reason)
        {
            NotifyUserInRoom(NotificationTypes.Error, room, reason);
            _logger.LogError($"NotifyErrorToCallerInRoom: {room}, {reason}");
        }

        public async Task Send(string roomName,  string message)
        {
            _logger.LogInformation($"Send {roomName} {message}");
            if (!InputValidator.ValidateRoomName(roomName))  {
                _logger.LogError($"Invalid roomName : {roomName}");
                return ;
            }
            if (!InputValidator.ValidateMessage(message))  {
                _logger.LogError($"Invalid message : {message}");
                return ;
            }
            var groupname = ChatHubConstants.HubGroupRomsPrefix + roomName;
            ChatRoomInfo chanInfo ;
            if (!_cxManager.TryGetChanInfo(roomName, out chanInfo))
            {
                _logger.LogError($"No such room : {roomName}");
                var noChanMsg = _localizer.GetString(ChatHubConstants.LabNoSuchChan).ToString();
                NotifyUserInRoom(NotificationTypes.Error, roomName, noChanMsg);
                return;
            }
            var userName = _cxManager.GetUserName(Context.ConnectionId);
            if (!_cxManager.IsPresent(roomName, userName))
            {
                _logger.LogError($"{userName} Not present in room : {roomName}");
                var notSentMsg = _localizer.GetString(ChatHubConstants.LabnoJoinNoSend).ToString();
                NotifyUserInRoom(NotificationTypes.Error, roomName, notSentMsg);
                return;
            }
            var group = Clients.Group(groupname);
            var msg = new { Name = userName, Room = roomName, Message = message};
            await group.SendAsync("ReceiveMessage", msg);
        }

        async Task NotifyUser(string type, string targetId, string message)
        {
            _logger.LogInformation("notifying user  {type} {targetId} : {message}");
            await Clients.Caller.SendAsync("notifyUser", type, targetId, message);
        }

        async Task NotifyUserInRoom(string type, string room, string message)
        {
            await Clients.Caller.SendAsync("notifyUserInRoom", type, room, message);
        }

        [Authorize]
        public async Task SendPV(string userName,  string message)
        {
            _logger.LogInformation($"Sending pv to {userName}");

            if (!InputValidator.ValidateUserName(userName)) 
            {
                _logger.LogError($"Invalid username : {userName}");
                return ;
            }
            if (!InputValidator.ValidateMessage(message)) 
            {
                _logger.LogError($"Invalid message : {message}");
                return ;
            }
            _logger.LogInformation($"Message form is validated.");

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
                        _logger.LogError($"Black listed : {Context.User.Identity.Name}");
                        await NotifyUser(NotificationTypes.PrivateMessageDenied, userName, "you are black listed.");
                        return;
                    }
                    _logger.LogInformation("Sender is no black listed");
                }

            _logger.LogInformation("getting cx id´s");
            var cxIds = _cxManager.GetConnexionIds(userName);
            if (cxIds==null || cxIds.Count()==0)
                _logger.LogError($"No such connected user : {userName}");
            else foreach (var connectionId in cxIds)
            {
                _logger.LogInformation($"cx: {connectionId}");
                var cli = Clients.Client(connectionId);
                _logger.LogInformation($"cli: {cli.ToString()}");
                await cli.SendAsync("addPV", Context.User.Identity.Name, message);
                _logger.LogInformation($"Sent pv to cx {connectionId}");
            }
        }

        [Authorize]
        public async Task SendStream(string connectionId, long streamId,  string message)
        {
            if (!InputValidator.ValidateMessage(message)) return;
            var sender = Context.User.Identity.Name;
            var cli = Clients.Client(connectionId);
            await cli.SendAsync("addStreamInfo", sender, streamId, message);
        }

    }
}
