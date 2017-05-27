
using ZicMoove.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using XLabs.Forms.Mvvm;
using Yavsc;
using System;
using Newtonsoft.Json;
using ZicMoove.Model.Social.Messaging;

namespace ZicMoove.Model.Social.Chat
{
    public class ChatUserInfo : ViewModel, IChatUserInfo
    {
        public ChatUserInfo()
        {
            PrivateMessages.CollectionChanged += PrivateMessages_CollectionChanged;
        }

        private void PrivateMessages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Unread");
        }

        public string avatar;
        public string Avatar
        {
            get
            {
                return avatar;
            }
            set
            {
                var newSource = UserHelpers.Avatar(value);
                SetProperty<string>(ref avatar, value);
                SetProperty<ImageSource>(ref avatarSource, newSource, "AvatarSource");
            }
        }

        ImageSource avatarSource;
        [JsonIgnore]
        public ImageSource AvatarSource
        {
            get
            {
                return avatarSource;
            }
        }

        public Connection [] Connections
        {
            get
            {
                return ObservableConnections?.ToArray();
            }
            set
            {
                ObservableConnections = new ObservableCollection<Connection>(value);
            }
        }

        ObservableCollection<Connection> connections;
        [JsonIgnore]
        public ObservableCollection<Connection> ObservableConnections
        {
            get
            {
                return connections;
            }
            set
            {
                SetProperty<ObservableCollection<Connection>>(ref connections, value);
            }
        }

        string[] roles;
        public string[] Roles
        {
            get
            {
                return roles;
            }
            set
            {
                SetProperty<string[]>(ref roles, value);
                NotifyPropertyChanged("RolesAsAString");
            }
        }

        [JsonIgnore]
        public string RolesAsAString
        {
            get
            {
                return Roles == null? "": string.Join(", ", Roles);
            }
        }

        string userId;
        public string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                SetProperty<string>(ref userId, value);
            }
        }

        string userName;
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                SetProperty<string>(ref userName, value);
            }
        }

        public bool IsConnected { get
            {
                return Connections.Length > 0;
            } }

        [JsonIgnore]
        IConnection[] IChatUserInfo.Connections
        {
            get
            {
                return Connections;
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        ObservableCollection<ChatMessage> privateMessages = new ObservableCollection<ChatMessage>();
        [JsonIgnore]
        public ObservableCollection<ChatMessage> PrivateMessages
        {
            get
            {
                return privateMessages;
            }
        }

        [JsonIgnore]
        public bool Unread
        {
            get
            {
                return PrivateMessages==null?false: PrivateMessages.Any(
                    m => !m.Read);
            }
        }

        [JsonIgnore]
        public ImageSource MessagesBadge
        {
            get
            {
                return Unread ? ImageSource.FromResource("ZicMoove.Images.Chat.talk.png") :null;
            }
        }

        public void OnConnected(string cxId)
        {
            // We do assume this cxId dosn't already exist in this list.
            var cx = new Connection { ConnectionId = cxId, Connected = true };
            if (ObservableConnections == null)
                ObservableConnections = new ObservableCollection<Connection>();
            ObservableConnections.Add(cx);
            if (this.ObservableConnections.Count == 1)
                NotifyPropertyChanged("IsConnected");
        }

        public void OnDisconnected(string cxId)
        {
            var existentcx = Connections.FirstOrDefault(cx => cx.ConnectionId == cxId);
            if (existentcx != null)
            {
                this.ObservableConnections.Remove(existentcx);

                if (this.ObservableConnections.Count == 0)
                    NotifyPropertyChanged("IsConnected");
            }
        }
    }
}
