using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xamarin.Forms;

namespace BookAStar.Pages
{
    public partial class ChatPage : TabbedPage
    {
        public ObservableCollection<string> Messages { get; set; }
        public string ChatUser { get; set; }

        private HubConnection chatHubConnection;
        private IHubProxy chatHubProxy;

        public ChatPage()
        {
            InitializeComponent();

            Title = "Chat";

            sendButton.Clicked += async (sender, args) => {
                IsBusy = true;

                try
                {
                    await chatHubProxy.Invoke<string>("Send", ChatUser, messageEntry.Text);
                }
                catch (Exception ex)
                {

                    Debug.WriteLine(ex);
                }

                IsBusy = false;
            };
             messageList.ItemsSource = Messages = new ObservableCollection<string>();
        }

        protected override async void OnAppearing()
        {
            IsBusy = true;
            chatHubConnection = new HubConnection(Constants.SignalRHubsUrl);
            
            chatHubProxy = chatHubConnection.CreateHubProxy("ChatHub");
            
            chatHubProxy.On<string, string>("AddMessage", (n, m) => {
                Messages.Add(string.Format("{0} says: {1}", n, m));
            });
            await chatHubConnection.Start();
            IsBusy = false;
            sendButton.IsEnabled = true;
        }
    }
}
