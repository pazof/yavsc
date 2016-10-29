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
                     ConnectionState cs = App.ChatHubConnection.State;

                    await App.CurrentApp.ChatHubProxy.Invoke<string>("Send", ChatUser, messageEntry.Text);
                    messageEntry.Text = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                IsBusy = false;
            };

            sendPVButton.Clicked += async (sender, args) =>
            {
                IsBusy = true;

                try
                {
                    await App.CurrentApp.ChatHubProxy.Invoke<string>("SendPV", ChatUser, pvEntry.Text);
                    pvEntry.Text = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                IsBusy = false;
            };
            messageList.ItemsSource = Messages = new ObservableCollection<ChatMessage>();
            notifList.ItemsSource = Notifs = new ObservableCollection<ChatMessage>();
            App.ChatHubConnection.StateChanged += ChatHubConnection_StateChanged;
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

        private void ReconnectButton_Clicked(object sender, EventArgs e)
        {
            App.ChatHubConnection.Stop();
            App.ChatHubConnection.Start();
        }

        private void MainSettings_UserChanged(object sender, EventArgs e)
        {
            ChatUser = MainSettings.UserName;
            contactPicker.ItemsSource = DataManager.Current.Contacts;
            PVList.ItemsSource = DataManager.Current.PrivateMessages;
        }

        private void ChatHubConnection_StateChanged(StateChange obj)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () => {
                    sendButton.IsEnabled = obj.NewState == ConnectionState.Connected;
                });
        }
    }
}
