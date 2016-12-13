using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels.Messaging
{
    using Data;
    using Model.Social.Messaging;

    class ChatViewModel: ViewModel
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ObservableCollection<ChatMessage> Notifs { get; set; }
        public ObservableCollection<ChatMessage> PVs { get; set; }
        public ObservableCollection<UserViewModel> Contacts { get; set; }

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
            Contacts = 
                new ObservableCollection<UserViewModel>(
                DataManager.Instance.Contacts.Select(c=>new UserViewModel { Data = c }));
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
                    OnUserDisconnected(userName);
            });
        }

        private void OnUserConnected(string cxId, string userName)
        {
            var user = Contacts.SingleOrDefault(
                c => c.Data.UserName == userName);
            if (user != null)
                user.ConnexionId = cxId;
        }

        private void OnUserDisconnected (string userName)
        {
            var user = Contacts.SingleOrDefault(
                c => c.Data.UserName == userName);
            if (user != null)
                user.ConnexionId = null;
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
