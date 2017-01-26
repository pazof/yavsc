
using ZicMoove.ViewModels.UserProfile;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;

using Xamarin.Forms;

namespace ZicMoove.Pages.UserProfile
{
    public partial class UserProfilePage : ContentPage
    {
        public UserProfilePage()
        {
            InitializeComponent();
            AvatarButton.Clicked += AvatarButton_Clicked;
        }
        public UserProfilePage(UserProfileViewModel model)
        {
            InitializeComponent();
            AvatarButton.Clicked += AvatarButton_Clicked;
            BindingContext = model;
        }
        private async void AvatarButton_Clicked (object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "Avatars",
                Name = "me.jpg"
            });

            if (file == null)
                return;
                  // ImageSource.FromFile(file.Path);
            /* ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            }); */
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
