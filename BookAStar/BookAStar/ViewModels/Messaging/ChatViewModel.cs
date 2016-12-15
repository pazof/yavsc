using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels.Messaging
{
    using Data;
    using Model.Social.Chat;
    using Model.Social.Messaging;

    class ChatViewModel: ViewModel
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ObservableCollection<ChatMessage> Notifs { get; set; }
        public ObservableCollection<ChatMessage> PVs { get; set; }
        public ChatUserCollection ChatUsers { get; set; }

        private ConnectionState state;
        public ConnectionState State
        {
            get { return state; }
        }

        public ChatViewModel()
        {
            App.ChatHubConnection.StateChanged += ChatHubConnection_StateChanged;
            MainSettings.UserChanged += MainSettings_UserChanged;
            Messages = new ObservableCollection<ChatMessage>();
            Notifs = new ObservableCollection<ChatMessage>();
            PVs = DataManager.Instance.PrivateMessages;
            ChatUsers = DataManager.Instance.ChatUsers;
            App.ChatHubProxy.On<string, string>("addMessage", (n, m) =>
            {
                Messages.Add(new ChatMessage
                {
                    Message = m,
                    SenderId = n,
                    Date = DateTime.Now
                });
            });

            App.ChatHubProxy.On<string, string, string>("notify", (eventId, cxId, userName) =>
            {
                var msg = new ChatMessage
                {
                    Message = eventId,
                    SenderId = userName,
                    Date = DateTime.Now
                };
                // TODO make admin possible 
                // by assigning a server side username to anonymous.
                if (string.IsNullOrEmpty(userName))
                {
                    msg.SenderId = $"({cxId})";
                }
                Notifs.Add(msg);
                if (eventId == "connected")
                    OnUserConnected(cxId, userName);
                else if (eventId == "disconnected")
                    OnUserDisconnected(cxId, userName);
            });
        }

        private void OnUserConnected(string cxId, string userName)
        {
            var user = ChatUsers.SingleOrDefault(
                c => c.UserName == userName);
            if (user == null)
            {
                user = new ChatUserInfo {
                    UserName = userName
                };
                ChatUsers.Add(user);
            }
            user.OnConnected(cxId);
        }

        private void OnUserDisconnected (string cxId, string userName)
        {
            var user = ChatUsers.SingleOrDefault(
                c => c.UserName == userName);
            if (user == null)
            {
                return;
            }
            user.OnDisconnected(cxId);
        }

        private void MainSettings_UserChanged(object sender, EventArgs e)
        {
        }

        private void ChatHubConnection_StateChanged(StateChange obj)
        {
            SetProperty<ConnectionState>(ref state, obj.NewState, "State");
        }
    }
}
