using System;
using System.Collections.Generic;
using Yavsc.ViewModels.Chat;

namespace Yavsc.Services
{
    public interface IConnexionManager {
        void SetUserName(string cxId, string userName);

        string GetUserName (string cxId);
        void OnConnected(string userName, bool isCop);
        bool IsConnected(string candidate);
        bool IsPresent(string roomName, string userName);
        
        ChatRoomInfo Join(string roomName, string userName);

        bool Part(string cxId, string roomName,  string reason);

        bool Kick(string cxId, string userName, string roomName,  string reason);

        bool Op(string roomName, string userName);
        bool Deop(string roomName, string userName);
        bool Hop(string roomName, string userName);
        bool Dehop(string roomName, string userName);
        bool TryGetChanInfo(string room, out ChatRoomInfo chanInfo);

        IEnumerable<string> GetConnexionIds(string userName);
        void Abort(string connectionId);

        void SetErrorHandler(Action<string,string> errorHandler);
        IEnumerable<ChannelShortInfo> ListChannels(string pattern);
        
    }
}
