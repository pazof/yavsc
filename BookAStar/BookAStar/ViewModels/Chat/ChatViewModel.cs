using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels
{
    using Data;
    using Model;
    using Model.Social.Messaging;

    class ChatViewModel: ViewModel
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ObservableCollection<ChatMessage> Notifs { get; set; }
        public ObservableCollection<ChatMessage> PVs { get; set; }
        public ObservableCollection<ClientProviderInfo> Contacts { get; set; }
        private string chatUser;
        public string ChatUser
        {
            get
            {
                return chatUser;
            }
            set
            {
                SetProperty<string>(ref chatUser, value);
            }
        }
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
            PVs = DataManager.Current.PrivateMessages;
            Contacts = DataManager.Current.Contacts;
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
                // TODO make admin possible 
                // by assigning a server side username to anonymous.
                // From now, don't log anonymous
                if (!string.IsNullOrEmpty(userName))
                {
                    Notifs.Add(new ChatMessage
                    {
                        Message = eventId,
                        SenderId = userName,
                        Date = DateTime.Now
                    });
                    if (eventId == "connected")
                        OnUserConnected(cxId, userName);
                    else if (eventId == "disconnected")
                        OnUserDisconnected(userName);
                }
            });
            ChatUser = MainSettings.UserName;
        }

        private void OnUserConnected(string cxId, string userName)
        {
            var user = Contacts.SingleOrDefault(
                c => c.UserName == userName);
            if (user != null)
                user.ChatHubConnectionId = cxId;
        }

        private void OnUserDisconnected (string userName)
        {
            var user = Contacts.SingleOrDefault(
                c => c.UserName == userName);
            if (user != null)
                user.ChatHubConnectionId = null;
        }

        private void MainSettings_UserChanged(object sender, EventArgs e)
        {
            ChatUser = MainSettings.UserName;
        }

        private void ChatHubConnection_StateChanged(StateChange obj)
        {
            SetProperty<ConnectionState>(ref state, obj.NewState, "State");
        }
    }
}
