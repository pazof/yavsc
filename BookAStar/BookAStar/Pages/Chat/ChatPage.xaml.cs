using System;
using System.Diagnostics;
using Microsoft.AspNet.SignalR.Client;
using Xamarin.Forms;

namespace BookAStar.Pages.Chat
{
    using Data;
    using System.Linq;
    using ViewModels.Messaging;

    public partial class ChatPage : TabbedPage
    {
        public string ChatUser { get; set; }

        public ChatPage()
        {
            InitializeComponent();

            Title = "Chat";
            /*
            ToolbarItems.Add(new ToolbarItem(
                name: "...",
                icon: null,
                activated: () => { })); */
            App.ChatHubConnection.StateChanged += ChatHubConnection_StateChanged;
            sendButton.Clicked += async (sender, args) =>
            {
                IsBusy = true;

                try
                {
                     ConnectionState cs = App.ChatHubConnection.State;

                    await App.ChatHubProxy.Invoke<string>("Send", ChatUser, messageEntry.Text);
                    messageEntry.Text = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                IsBusy = false;
            };
            chatUserList.BindingContext = DataManager.Instance.ChatUsers;
            
            /*
            sendPVButton.Clicked += async (sender, args) =>
            {
                string userName = contactPicker.SelectedItem as string;
                if (string.IsNullOrEmpty(userName)) return;
                var user = DataManager.Current.Contacts.Single(
                    c => c.UserName == userName);
                IsBusy = true;
                try
                {
                    await App.ChatHubProxy.Invoke<string>("SendPV", user.ChatHubConnectionId, pvEntry.Text);
                    pvEntry.Text = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                IsBusy = false;
            };*/

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
