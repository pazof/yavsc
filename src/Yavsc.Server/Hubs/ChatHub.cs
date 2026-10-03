#nullable enable annotations

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

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;

#pragma warning disable CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel

namespace Yavsc.Server.Hubs
{
    using System.Diagnostics;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;
    using Yavsc.Abstract.Chat;
    using Yavsc.Models;
    using Yavsc.Models.Chat;
    using Yavsc.Server.Helpers;
    using Yavsc.Services;

    [Authorize]
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

            _cxManager = connexionManager;
            _cxManager.SetErrorHandler((context, error) =>
            {
                NotifyUser(NotificationTypes.Error, context, error);
            });
            _logger = loggerFactory.CreateLogger<ChatHub>();
            InputValidator = new HubInputValidator(_localizer)
            {

                NotifyUser = async (type, target, msg) => await this.NotifyUser(type, target, msg)
            };
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            bool isCop = false;
            string userName = user.GetUserName();

            _logger.LogInformation(_localizer.GetString(ChatHubConstants.LabAuthChatUser));

            var userId = _dbContext.Users.First(u => u.UserName == userName).Id;

            await Clients.Group(ChatHubConstants.HubGroupFollowingPrefix + userId).SendAsync("notifyUser", NotificationTypes.Connected, userName, null);
            isCop = Context.User.IsInMsRole(Constants.AdminGroupName);
            if (isCop)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ChatHubConstants.HubGroupCops);
            }

            foreach (var uid in _dbContext.CircleMembers.Select(m => m.MemberId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ChatHubConstants.HubGroupFollowingPrefix + uid);
            }
            _cxManager.OnConnected(Context.ConnectionId, userName, isCop);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string userName = Context.User?.GetUserName();
            if (userName != null)
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.UserName == userName);
                if (user != null)
                {
                    var userId = user.Id;
                    await Clients.Group(ChatHubConstants.HubGroupFollowingPrefix + userId).SendAsync("notifyUser", NotificationTypes.DisConnected, userName, null);
                }

                _cxManager.OnDisconnected(Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(ex);
        }

        /// <summary>
        /// Renvoie le login résolu côté serveur (claim <c>name</c> de
        /// l'organisation Yavsc.Org, via <see cref="UserHelpers.GetUserName"/>).
        /// Permet au client d'afficher l'identité réellement authentifiée,
        /// sans jamais faire confiance à un pseudo fourni par le client.
        /// </summary>
        public string WhoAmI()
        {
            return Context.User?.GetUserName() ?? string.Empty;
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

            var roomGroupName = ChatHubConstants.HubGroupRoomsPrefix + roomName;
            var user = _cxManager.GetUserName(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomGroupName);
            ChatRoomInfo channelInfo;
            if (!_cxManager.IsPresent(roomName, user))
            {
                _logger.LogInformation($"Joining");
                channelInfo = _cxManager.Join(roomName, Context.ConnectionId);
                await Clients.Group(roomGroupName).SendAsync("notifyRoom", NotificationTypes.UserJoin, roomName, user);
            }
            else
            {
                _logger.LogInformation($"already present");
                // in case in an additional connection,
                // one only send info on room without
                // warning any other user.
                if (!_cxManager.TryGetChanInfo(roomName, out channelInfo))
                {
                    _logger.LogError($"Failed to retrieve channel info for room: {roomName}");
                    return null;
                }
            }

            _logger.LogInformation($"returning channel info");
            await Clients.Caller.SendAsync("joint", channelInfo);
            return channelInfo;
        }

        public void Register(string room)
        {
            if (!InputValidator.ValidateRoomName(room)) return;
            var existent = _dbContext.ChatRoom.Any(r => r.Name == room);
            if (existent)
            {
                NotifyUserInRoom(NotificationTypes.Error, room, "already registered.");
                return;
            }
            Debug.Assert(Context.User != null);
            string userName = Context.User.GetUserName();
            var user = _dbContext.Users.FirstOrDefault(u => u.UserName == userName);

            var newRoom = new ChatRoom { Name = room, OwnerId = Context.User.GetUserId() };
            ChatRoomInfo channelInfo;
            if (_cxManager.TryGetChanInfo(room, out channelInfo))
            {
                // TODO get and require some admin status for current user on this channel
                newRoom.Topic = channelInfo.Topic;
            }
            newRoom.LatestJoinPart = DateTime.UtcNow;

            _dbContext.ChatRoom.Add(newRoom);
            _dbContext.SaveChanges(user.Id);
        }

        public void KickBan(string roomName, string userName, string reason)
        {
            if (!InputValidator.ValidateRoomName(roomName)) return;
            if (!InputValidator.ValidateUserName(userName)) return;
            if (!InputValidator.ValidateReason(reason)) return;
            Kick(roomName, userName, reason);
            Ban(roomName, userName, reason);
        }

        public async Task Kick(string roomName, string userName, string reason)
        {
            if (!InputValidator.ValidateRoomName(roomName)) return;
            if (!InputValidator.ValidateUserName(userName)) return;
            if (!InputValidator.ValidateReason(reason)) return;
            ChatRoomInfo channelInfo;
            var roomGroupName = ChatHubConstants.HubGroupRoomsPrefix + roomName;
            if (_cxManager.TryGetChanInfo(roomName, out channelInfo))
            {
                if (!_cxManager.IsPresent(roomName, userName))
                {
                    NotifyErrorToCallerInRoom(roomName, $"{userName} was not found in {roomName}.");
                    return;
                }
                // in case of Kick returned false, being not allowed to, or for what ever other else failure,
                // the error handler will send an error message while handling the error.
                if (!_cxManager.Kick(Context.ConnectionId, userName, roomName, reason)) return;
            }
            var cxIds = _cxManager.GetConnexionIds(userName);
            if (cxIds != null) foreach (var cx in cxIds)
                await Groups.RemoveFromGroupAsync(cx, roomGroupName);
            await Clients.Group(roomGroupName).SendAsync("notifyRoom", NotificationTypes.Kick, roomName, $"{userName}: {reason}");
        }

        public void Ban(string roomName, string userName, string reason)
        {
            if (!InputValidator.ValidateRoomName(roomName)) return;
            if (!InputValidator.ValidateUserName(userName)) return;
            if (!InputValidator.ValidateReason(reason)) return;
            var cxIds = _cxManager.GetConnexionIds(userName);
            throw new NotImplementedException();
        }

        [Authorize(Constants.AdminGroupName)]
        public void GLine(string userName, string reason)
        {
            if (!InputValidator.ValidateUserName(userName)) return;
            if (!InputValidator.ValidateReason(reason)) return;
            throw new NotImplementedException();
        }

        public void Part(string roomName, string reason)
        {
            if (!InputValidator.ValidateRoomName(roomName)) return;
            if (!InputValidator.ValidateReason(reason)) return;
            if (_cxManager.Part(Context.ConnectionId, roomName, reason))
            {
                var roomGroupName = ChatHubConstants.HubGroupRoomsPrefix + roomName;
                var group = Clients.Group(roomGroupName);
                var userName = _cxManager.GetUserName(Context.ConnectionId);
                group.SendAsync("notifyRoom", NotificationTypes.UserPart, roomName, $"{userName}: {reason}");
                Groups.RemoveFromGroupAsync(Context.ConnectionId, roomGroupName);
            }
            else
            {
                _logger.LogError("Could not part");
            }
        }

        void NotifyErrorToCallerInRoom(string room, string reason)
        {
            NotifyUserInRoom(NotificationTypes.Error, room, reason);
            _logger.LogError($"NotifyErrorToCallerInRoom: {room}, {reason}");
        }

        public async Task Send(string roomName, string message)
        {
            _logger.LogInformation($"Send {roomName} {message}");
            if (!InputValidator.ValidateRoomName(roomName))
            {
                _logger.LogError($"Invalid roomName : {roomName}");
                return;
            }
            if (!InputValidator.ValidateMessage(message))
            {
                _logger.LogError($"Invalid message : {message}");
                return;
            }
            var groupName = ChatHubConstants.HubGroupRoomsPrefix + roomName;
            ChatRoomInfo channelInfo;
            if (!_cxManager.TryGetChanInfo(roomName, out channelInfo))
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
                var notSentMsg = _localizer.GetString(ChatHubConstants.LabNoJoinNoSend).ToString();
                NotifyUserInRoom(NotificationTypes.Error, roomName, notSentMsg);
                return;
            }
            var group = Clients.Group(groupName);
            var msg = new { Name = userName, Room = roomName, Message = message };
            await group.SendAsync("ReceiveMessage", msg);
        }

        async Task NotifyUser(string type, string targetId, string message)
        {
            _logger.LogInformation($"notifying user  {type} {targetId} : {message}");
            await Clients.Caller.SendAsync("notifyUser", type, targetId, message);
        }

        async Task NotifyUserInRoom(string type, string room, string message)
        {
            await Clients.Caller.SendAsync("notifyUserInRoom", type, room, message);
        }

        public async Task SendPV(string userName, string message)
        {
            // Authorized code
            Debug.Assert(Context.User != null);
            _logger.LogInformation($"Sending pv to {userName}");

            if (!InputValidator.ValidateUserName(userName))
            {
                _logger.LogError($"Invalid username : {userName}");
                return;
            }
            if (!InputValidator.ValidateMessage(message))
            {
                _logger.LogError($"Invalid message : {message}");
                return;
            }
            _logger.LogInformation($"Message form is validated.");
            var identityUserName = Context.User.GetUserName();

            if (userName[0] != '?' && Context.User != null)
                if (!Context.User.IsInMsRole(Constants.AdminGroupName))
                {

                    var bl = _dbContext.BlackListed
                        .Include(r => r.User)
                        .Include(r => r.Owner)
                        .Where(r => r.User.UserName == identityUserName && r.Owner.UserName == userName)
                        .Select(r => r.OwnerId);

                    if (bl.Count() > 0)
                    {
                        _logger.LogError($"Black listed : {identityUserName}");
                        await NotifyUser(NotificationTypes.PrivateMessageDenied, userName, "you are black listed.");
                        return;
                    }
                    _logger.LogInformation("Sender is no black listed");
                }

            _logger.LogInformation("getting cx id´s");
            var cxIds = _cxManager.GetConnexionIds(userName);
            if (cxIds == null || cxIds.Count() == 0)
                _logger.LogError($"No such connected user : {userName}");
            else foreach (var connectionId in cxIds)
            {
                _logger.LogInformation($"cx: {connectionId}");
                var cli = Clients.Client(connectionId);
                _logger.LogInformation($"cli: {cli.ToString()}");
                await cli.SendAsync("addPV", identityUserName, message);
                _logger.LogInformation($"Sent pv to cx {connectionId}");
            }
        }

        public async Task SendStream(string connectionId, long streamId, string message)
        {
            // Authorized code
            Debug.Assert(Context.User != null);
            Debug.Assert(Context.User.Identity != null);
            if (!InputValidator.ValidateMessage(message)) return;
            var sender = Context.User.GetUserName();
            var cli = Clients.Client(connectionId);
            await cli.SendAsync("addStreamInfo", sender, streamId, message);
        }

    }
}

#pragma warning restore CS4014
