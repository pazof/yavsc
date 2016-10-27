using BookAStar.Data;
using BookAStar.Model.Social.Messaging;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xamarin.Forms;
using XLabs.Caching;
using XLabs.Forms.Controls;
using XLabs.Ioc;

namespace BookAStar.Pages
{
    public partial class ChatPage : TabbedPage
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ObservableCollection<ChatMessage> Notifs { get; set; }
        public string ChatUser { get; set; }

        public ChatPage()
        {
            InitializeComponent();

            Title = "Chat";

            sendButton.Clicked += async (sender, args) =>
            {
                IsBusy = true;

                try
                {
                    await App.CurrentApp.ChatHubProxy.Invoke<string>("Send", ChatUser, messageEntry.Text);
                    messageEntry.Text = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                IsBusy = false;
            };
            // contactPicker.DisplayProperty = "UserName";
            
            messageList.ItemsSource = Messages = new ObservableCollection<ChatMessage>();
            PVList.ItemsSource = DataManager.Current.PrivateMessages;
            App.CurrentApp.ChatHubConnection.StateChanged += ChatHubConnection_StateChanged;
            // DataManager.Current.Contacts 

            MainSettings.UserChanged += MainSettings_UserChanged;
            MainSettings_UserChanged(this, null);

            App.CurrentApp.ChatHubProxy.On<string, string>("addMessage", (n, m) =>
            {
                Messages.Add(new ChatMessage
                {
                    Message = m,
                    SenderId = n,
                    Date = DateTime.Now
                });
            });

            App.CurrentApp.ChatHubProxy.On<string, string>("notify", (n, m) =>
            {
                Notifs.Add(new ChatMessage
                {
                    Message = m,
                    SenderId = n,
                    Date = DateTime.Now
                });
            });
        }

        private void MainSettings_UserChanged(object sender, EventArgs e)
        {
            ChatUser = MainSettings.UserName;
            contactPicker.ItemsSource = DataManager.Current.Contacts;
        }

        private void ChatHubConnection_StateChanged(StateChange obj)
        {
            sendButton.IsEnabled = obj.NewState == ConnectionState.Connected;
        }
    }
}
