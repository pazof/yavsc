using BookAStar.Model.Auth.Account;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using BookAStar.Helpers;
using XLabs.Forms.Behaviors;
using System.Diagnostics;
using XLabs.Forms.Controls;

namespace BookAStar.Pages
{
   
    public partial class AccountChooserPage : ContentPage
	{
        public ICommand RemoteSettingsRefreshCommand { get; private set; }

        public AccountChooserPage ()
		{
			InitializeComponent ();

            AccountListView.ItemsSource = MainSettings.AccountList;

            this.Musical = MainSettings.Musical;
            this.Environ = MainSettings.Environ;
            this.BindingContext = this;

            AddAccountBtn.Clicked += AddAccountBtn_Clicked;

             // avatarImage.
            //RemoveAccountBouton.Clicked += RemoveAccountBouton_Clicked;

            AccountListView.ItemSelected += Accounts_ItemSelected;
            DumpParam = new RelayGesture((g, x) =>
            {
                if (g.GestureType == GestureType.Swipe && g.Direction == Directionality.Left)
                {
                    RemoveAccount();
                }
            }); 
        }

        public RelayGesture DumpParam { get; set; }
        public ObservableCollection<User> Accounts { get; private set; }
        public Dictionary<string, double> Musical { get; private set; }
        public Dictionary<string, double> Environ { get; private set; }

        private void Accounts_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var user = e.SelectedItem as User;
            // should later invoke IdentificationChanged
            // FIXME don't do it when it's not from the user action
            MainSettings.CurrentUser = user;
        }

        private async void RemoveAccount()
        {
            User user = AccountListView.SelectedItem as User;
            var doIt = await this.Confirm("Suppression de l'identification",
                $"Vous êtes sur le point de supprimer votre identification pour ce compte : {user.UserName}\nContinuer ?");
            if (doIt)
                App.PlatformSpecificInstance.RevokeAccount(user.UserName);
        }

        

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AccountListView.SelectedItem = MainSettings.CurrentUser;
        }

        private void AddAccountBtn_Clicked(object sender, EventArgs e)
        {
            App.PlatformSpecificInstance.AddAccount();
        }

       
    }
}

