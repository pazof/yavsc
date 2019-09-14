
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Yavsc.Abstract.Chat;
using Yavsc.Models;
using Yavsc.ViewModels.Chat;

namespace Yavsc.Services
{

    /// <summary>
    /// Connexion Manager
    /// </summary>
    public class HubConnectionManager : IConnexionManager
    {
        ILogger _logger;

        Action<string, string> _errorHandler;

        /// <summary>
        /// by cx id
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>

        static ConcurrentDictionary<string, string> ChatUserNames = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// by user  name
        /// </summary>
        /// <returns></returns>
        static ConcurrentDictionary<string, List<string>> ChatCxIds = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// by user  name,
        /// the list of its chat rooms
        /// </summary>
        /// <returns></returns>
        static ConcurrentDictionary<string, List<string>> ChatRoomPresence = new ConcurrentDictionary<string, List<string>>();
        static ConcurrentDictionary<string, bool> _isCop = new ConcurrentDictionary<string, bool>();

        public static ConcurrentDictionary<string, ChatRoomInfo> Channels = new ConcurrentDictionary<string, ChatRoomInfo>();
        ApplicationDbContext _dbContext;
        IStringLocalizer _localizer;

        public HubConnectionManager()
        {
            var scope = Startup.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<HubConnectionManager>();
            var stringLocFactory = scope.ServiceProvider.GetService<IStringLocalizerFactory>();
            _localizer = stringLocFactory.Create(typeof(HubConnectionManager));
        }

        public void SetUserName(string cxId, string userName)
        {
            string oldUname;
            if (ChatUserNames.TryGetValue(cxId, out oldUname))
            {
                // this is a rename
                if (oldUname == userName) return;
                ChatCxIds[userName] = ChatCxIds[oldUname];
                ChatCxIds[oldUname] = null;
            }
            else
            {
                // this is a connexion
                ChatCxIds[userName] = new List<string>() { cxId };
            }
            ChatUserNames[cxId] = userName;
        }
        // Username must have been set before calling this method.
        public void OnConnected(string cxId,  bool isCop)
        {
            var username = ChatUserNames[cxId];
            if (!IsConnected(username)) 
                ChatRoomPresence[username] = new List<string>();
            _isCop[username] = isCop;
        }

        public bool IsConnected(string candidate)
        {
            return ChatRoomPresence.ContainsKey(candidate)
                && ChatRoomPresence[candidate] != null;
        }

        public bool IsPresent(string roomName, string userName)
        {
            return ChatRoomPresence[userName].Contains(roomName);
        }

        public bool isCop(string userName)
        {
            return _isCop[userName];
        }

        public void OnDisctonnected(string connectionId)
        {
            string uname;

            if (!ChatUserNames.TryRemove(connectionId, out uname))
                _logger.LogError($"Could not get removed user name for cx {connectionId}");
            else
            {
                List<string> cxIds;
                if (ChatCxIds.TryGetValue(uname, out cxIds))
                {
                    cxIds.Remove(connectionId);
                    foreach (var room in ChatRoomPresence[uname])
                    {
                        Part(connectionId, room, "connexion aborted");
                    }
                }
                else
                    _logger.LogError($"Could not remove user cx {connectionId}");

                ChatRoomPresence[uname] = null;
            }
        }

        public bool Part(string cxId, string roomName, string reason)
        {
            ChatRoomInfo chanInfo;
            var userName = ChatUserNames[cxId];
            if (Channels.TryGetValue(roomName, out chanInfo))
            {
                if (!chanInfo.Users.Contains(cxId))
                {
                    // TODO NotifyErrorToCaller(roomName, "you didn't join.");
                    return false;
                }
                // FIXME only remove cx, not username,
                // as long as he might be connected 
                // from another device, to the same room
                chanInfo.Users.Remove(cxId);
                if (chanInfo.Users.Count == 0)
                {
                    ChatRoomInfo deadchanInfo;
                    if (Channels.TryRemove(roomName, out deadchanInfo))
                    {
                        var room = _dbContext.ChatRoom.FirstOrDefault(r => r.Name == roomName);
                        room.LatestJoinPart = DateTime.Now;
                        _dbContext.SaveChanges();
                    }
                }
                return true;
            }
            else
            {
                return false;
                // TODO   NotifyErrorToCallerInRoom(roomName, $"user could not part: no such room");
            }
        }

        public ChatRoomInfo Join(string roomName, string cxId)
        {
            var userName = ChatUserNames[cxId];

            _logger.LogInformation($"Join: {userName}=>{roomName}");
            ChatRoomInfo chanInfo;
            // if channel already is open
            if (Channels.ContainsKey(roomName))
            {
                if (Channels.TryGetValue(roomName, out chanInfo))
                {
                    if (IsPresent(roomName, userName))
                    {
                        // TODO implement some unique connection sharing protocol
                        // between all terminals from a single user.
                        return chanInfo;
                    }
                    else
                    {
                        if (isCop(userName))
                        {
                            chanInfo.Ops.Add(cxId);
                        }
                        else{
                            chanInfo.Users.Add(cxId);
                        } 
                        _logger.LogInformation($"existing room joint: {userName}=>{roomName}");
                        if (!ChatRoomPresence[userName].Contains(roomName))
                            ChatRoomPresence[userName].Add(roomName);
                        return chanInfo;
                    }
                }
                else
                {
                    string msg = "room seemd to be avaible ... but we could get no info on it.";
                    _errorHandler(roomName, msg);
                    return null;
                }
            }
            // room was closed.
            var room = _dbContext.ChatRoom.FirstOrDefault(r => r.Name == roomName);
            chanInfo = new ChatRoomInfo();
            

            if (room != null)
            {
                chanInfo.Topic = room.Topic;
                chanInfo.Name = room.Name;
                chanInfo.Users.Add(cxId);
            }
            else
            { // a first join, we create it.
                chanInfo.Name = roomName;
                chanInfo.Topic =  _localizer.GetString(ChatHubConstants.JustCreatedBy)+userName;
                chanInfo.Ops.Add(cxId);
            }

            if (Channels.TryAdd(roomName, chanInfo))
            {
                ChatRoomPresence[userName].Add(roomName);
                _logger.LogInformation("new room joint");
                return (chanInfo);
            }
            else
            {
                string msg = "Chan create failed unexpectly...";
                _errorHandler(roomName, msg);
                return null;
            }
        }

        public bool Op(string roomName, string userName)
        {
            throw new System.NotImplementedException();
        }

        public bool Deop(string roomName, string userName)
        {
            throw new System.NotImplementedException();
        }

        public bool Hop(string roomName, string userName)
        {
            throw new System.NotImplementedException();
        }

        public bool Dehop(string roomName, string userName)
        {
            throw new System.NotImplementedException();
        }

        public string GetUserName(string cxId)
        {
            return ChatUserNames[cxId];
        }

        public bool TryGetChanInfo(string room, out ChatRoomInfo chanInfo)
        {
            return Channels.TryGetValue(room, out chanInfo);
        }

        public IEnumerable<ChannelShortInfo> ListChannels(string pattern)
        {
            if (pattern != null)
                return Channels.Where(c => c.Key.Contains(pattern))
                .OrderByDescending(c => c.Value.Users.Count).Select(c => new ChannelShortInfo { RoomName = c.Key, Topic = c.Value.Topic }).Take(10);

            return Channels
             .OrderByDescending(c => c.Value.Users.Count).Select(c => new ChannelShortInfo { RoomName = c.Key, Topic = c.Value.Topic }).Take(10);
        }

        public IEnumerable<string> GetConnexionIds(string userName)
        {
            return ChatCxIds.ContainsKey(userName) ? ChatCxIds[userName] : null;
        }

        /// <summary>
        /// set on error as string couple action
        /// </summary>
        /// <param name="errorHandler"></param>
        public void SetErrorHandler(Action<string, string> errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public bool Kick(string cxId, string userName, string roomName, string reason)
        {
            ChatRoomInfo chanInfo;
            if (!Channels.ContainsKey(roomName))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.LabNoSuchChan).ToString());
                return false;
            }

            if (!Channels.TryGetValue(roomName, out chanInfo)) 
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.LabNoSuchChan).ToString());
                return false;
            }
            
            var kickerName = GetUserName(cxId);
            if (!chanInfo.Ops.Contains(cxId))
            if (!chanInfo.Hops.Contains(cxId))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.LabYouNotOp).ToString());
                return false;
            }

            if (!IsPresent(roomName, userName))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.LabNoSuchUser).ToString());
                return false;
            }
            var ucxs = GetConnexionIds(userName);
            if (chanInfo.Hops.Contains(cxId))
            if (chanInfo.Ops.Any(c => ucxs.Contains(c)))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.HopWontKickOp).ToString());
                return false;
            }
            if (isCop(userName))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.NoKickOnCop).ToString());
                return false;
            }

            // all good, time to kick :-)
            foreach (var ucx in ucxs) { 
            if (chanInfo.Users.Contains(ucx)) 
                chanInfo.Users.Remove(ucx);

            else if (chanInfo.Ops.Contains(ucx)) 
                chanInfo.Ops.Remove(ucx);

            else if (chanInfo.Hops.Contains(ucx)) 
                chanInfo.Hops.Remove(ucx);
            }

            return true;
            
        }
    }
}
