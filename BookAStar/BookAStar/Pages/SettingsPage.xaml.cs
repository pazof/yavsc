using BookAStar.Model.Auth.Account;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace BookAStar
{
   
    public partial class SettingsPage : ContentPage
	{
        

        public ICommand RemoteSettingsRefreshCommand { get; private set; }

        public SettingsPage ()
		{
			InitializeComponent ();

            AccountListView.ItemsSource = MainSettings.AccountList;

            this.Musical = MainSettings.Musical;
            this.Environ = MainSettings.Environ;
            this.BindingContext = this;

            AddAccountBtn.Clicked += AddAccountBtn_Clicked;

            RemoveAccountBouton.Clicked += RemoveAccountBouton_Clicked;

            AccountListView.ItemSelected += Accounts_ItemSelected;
           
            pushstatus.Toggled += Pushstatus_Toggled;
            
        }

        protected void Pushstatus_Toggled(object sender, ToggledEventArgs e)
        {
            MainSettings.PushNotifications = pushstatus.IsToggled;
        }

        public ObservableCollection<User> Accounts { get; private set; }
        public Dictionary<string, double> Musical { get; private set; }
        public Dictionary<string, double> Environ { get; private set; }

        private void Accounts_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var user = e.SelectedItem as User;
            // should later invoke IdentificationChanged
            MainSettings.CurrentUser = user;
        }

        private async void RemoveAccountBouton_Clicked(object sender, EventArgs e)
        {
            User user = AccountListView.SelectedItem as User;
            var doIt = await Confirm("Suppression de l'identification",
                $"Vous êtes sur le point de supprimer votre identification pour ce compte : {user.UserName}\nContinuer ?");
            if (doIt)
                App.PlatformSpecificInstance.RevokeAccount(user.UserName);
        }

        async Task<bool> Confirm(string title, string procedure)
        {
            string yes = "Oui", no = "Non";
            var answer = await DisplayAlert(title,
               procedure, yes, no);
            return answer;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AccountListView.SelectedItem = MainSettings.CurrentUser;
            pushstatus.IsToggled = MainSettings.PushNotifications;
        }

        private void AddAccountBtn_Clicked(object sender, EventArgs e)
        {
            App.PlatformSpecificInstance.AddAccount();
        }

       
    }
}

