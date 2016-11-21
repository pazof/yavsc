
using BookAStar.ViewModels.UserProfile;
using System;

using Xamarin.Forms;

namespace BookAStar.Pages.UserProfile
{
    public partial class UserProfilePage : ContentPage
    {
        public UserProfilePage()
        {
            InitializeComponent();
          // AvatarButton.Clicked += AvatarButton_Clicked;
           
        }

        private void AvatarButton_Clicked (object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnManageFiles(object sender, EventArgs e)
        {
            ShowPage<UserFiles>(null, true);
        }

        public void OnViewPerformerStatus(object sender, EventArgs e)
        {
            ShowPage<AccountChooserPage>(null, true);
        }

        private void ShowPage<T>(object[] args, bool animate = false) where T : Page
        {
            App.NavigationService.NavigateTo<T>(animate, args);
            App.MasterPresented = false;
        }
    }
}
