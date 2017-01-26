
using System;
using Xamarin.Forms;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ZicMoove.Helpers;
using XLabs.Forms.Behaviors;
using XLabs.Forms.Controls;
using ZicMoove.Model.Auth.Account;

namespace ZicMoove.Pages.UserProfile
{
   
    public partial class AccountChooserPage : ContentPage
	{
        
        public AccountChooserPage ()
		{
			InitializeComponent ();

            AddAccountBtn.Clicked += AddAccountBtn_Clicked;

            //RemoveAccountBouton.Clicked += RemoveAccountBouton_Clicked;

            AccountListView.ItemSelected += Accounts_ItemSelected;
            // MainSettings.UserChanged += MainSettings_UserChanged;
        }
        // Should be useless
        private void MainSettings_UserChanged(object sender, EventArgs e)
        {
            AccountListView.SelectedItem = MainSettings.CurrentUser;
            throw new NotImplementedException();
        }

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
        }

        private void AddAccountBtn_Clicked(object sender, EventArgs e)
        {
            App.PlatformSpecificInstance.AddAccount();
        }

       
    }
}

