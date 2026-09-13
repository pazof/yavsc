using System.Collections.Concurrent;
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
        private readonly ILogger _logger;

        private Action<string, string>? _errorHandler;

        /// <summary>
        /// by cx id
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>

        static readonly ConcurrentDictionary<string, string> ChatUserNames = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// by user  name
        /// </summary>
        /// <returns></returns>
        static readonly ConcurrentDictionary<string, List<string>> ChatCxIds = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// by user  name,
        /// the list of its chat rooms
        /// </summary>
        /// <returns></returns>
        static readonly ConcurrentDictionary<string, List<string>> ChatRoomPresence = new ConcurrentDictionary<string, List<string>>();
        static readonly ConcurrentDictionary<string, bool> _isCop = new ConcurrentDictionary<string, bool>();

        public static ConcurrentDictionary<string, ChatRoomInfo> Channels = new ConcurrentDictionary<string, ChatRoomInfo>();
        readonly ApplicationDbContext _dbContext;
        readonly IStringLocalizer _localizer;
        public HubConnectionManager(IServiceScopeFactory ssf )
        {
            var scope = ssf.CreateScope();
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

        public bool IsCop(string userName)
        {
            return _isCop[userName];
        }

        public void OnDisconnected(string connectionId)
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
            ChatRoomInfo channelInfo;
            if (Channels.TryGetValue(roomName, out channelInfo))
            {
                if (!channelInfo.Users.Contains(cxId))
                {
                    // TODO NotifyErrorToCaller(roomName, "you didn't join.");
                    return false;
                }
                // FIXME only remove cx, not username,
                // as long as he might be connected
                // from another device, to the same room
                channelInfo.Users.Remove(cxId);
                if (channelInfo.Users.Count == 0)
                {
                    ChatRoomInfo deadChannelInfo;
                    if (Channels.TryRemove(roomName, out deadChannelInfo))
                    {
                        var room = _dbContext.ChatRoom.FirstOrDefault(r => r.Name == roomName);
                        room.LatestJoinPart = DateTime.UtcNow;
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
            ChatRoomInfo channelInfo;
            // if channel already is open
            if (Channels.ContainsKey(roomName))
            {
                if (Channels.TryGetValue(roomName, out channelInfo))
                {
                    if (IsPresent(roomName, userName))
                    {
                        // TODO implement some unique connection sharing protocol
                        // between all terminals from a single user.
                        return channelInfo;
                    }
                    else
                    {
                        if (IsCop(userName))
                        {
                            channelInfo.Ops.Add(cxId);
                        }
                        else{
                            channelInfo.Users.Add(cxId);
                        }
                        _logger.LogInformation($"existing room joint: {userName}=>{roomName}");
                        if (!ChatRoomPresence[userName].Contains(roomName))
                            ChatRoomPresence[userName].Add(roomName);
                        return channelInfo;
                    }
                }
                else
                {
                    string msg = "room seemed to be available ... but we could get no info on it.";
                    _errorHandler(roomName, msg);
                    return null;
                }
            }
            // room was closed.
            var room = _dbContext.ChatRoom.FirstOrDefault(r => r.Name == roomName);
            channelInfo = new ChatRoomInfo();


            if (room != null)
            {
                channelInfo.Topic = room.Topic;
                channelInfo.Name = room.Name;
                channelInfo.Users.Add(cxId);
            }
            else
            { // a first join, we create it.
                channelInfo.Name = roomName;
                channelInfo.Topic =  _localizer.GetString(ChatHubConstants.JustCreatedBy)+userName;
                channelInfo.Ops.Add(cxId);
            }

            if (Channels.TryAdd(roomName, channelInfo))
            {
                ChatRoomPresence[userName].Add(roomName);
                _logger.LogInformation("new room joint");
                return (channelInfo);
            }
            else
            {
                string msg = "Chan create failed unexpectedly...";
                _errorHandler(roomName, msg);
                return null;
            }
        }

        public bool Op(string roomName, string userName)
        {
            throw new System.NotImplementedException();
        }

        public bool DeOp(string roomName, string userName)
        {
            throw new System.NotImplementedException();
        }

        public bool Hop(string roomName, string userName)
        {
            throw new System.NotImplementedException();
        }

        public bool DeHop(string roomName, string userName)
        {
            throw new System.NotImplementedException();
        }

        public string GetUserName(string cxId)
        {
            return ChatUserNames[cxId];
        }

        public bool TryGetChanInfo(string room, out ChatRoomInfo channelInfo)
        {
            return Channels.TryGetValue(room, out channelInfo);
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
            ChatRoomInfo channelInfo;
            if (!Channels.ContainsKey(roomName))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.LabNoSuchChan).ToString());
                return false;
            }

            if (!Channels.TryGetValue(roomName, out channelInfo))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.LabNoSuchChan).ToString());
                return false;
            }

            var kickerName = GetUserName(cxId);
            if (!channelInfo.Ops.Contains(cxId))
            if (!channelInfo.Hops.Contains(cxId))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.LabYouNotOp).ToString());
                return false;
            }

            if (!IsPresent(roomName, userName))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.LabNoSuchUser).ToString());
                return false;
            }
            var userConnectionIds = GetConnexionIds(userName);
            if (channelInfo.Hops.Contains(cxId))
            if (channelInfo.Ops.Any(c => userConnectionIds.Contains(c)))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.HopWontKickOp).ToString());
                return false;
            }
            if (IsCop(userName))
            {
                _errorHandler(roomName, _localizer.GetString(ChatHubConstants.NoKickOnCop).ToString());
                return false;
            }

            // all good, time to kick :-)
            foreach (var ucx in userConnectionIds) {
            if (channelInfo.Users.Contains(ucx))
                channelInfo.Users.Remove(ucx);

            else if (channelInfo.Ops.Contains(ucx))
                channelInfo.Ops.Remove(ucx);

            else if (channelInfo.Hops.Contains(ucx))
                channelInfo.Hops.Remove(ucx);
            }

            return true;

        }
    }
}
