
using BookAStar.ViewModels.UserProfile;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;

using Xamarin.Forms;

namespace BookAStar.Pages.UserProfile
{
    public partial class UserProfilePage : ContentPage
    {
        public UserProfilePage()
        {
            InitializeComponent();
            AvatarButton.Clicked += AvatarButton_Clicked;
        }

        private async void AvatarButton_Clicked (object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "Sample",
                Name = "test.jpg"
            });

            if (file == null)
                return;

            await DisplayAlert("File Location", file.Path, "OK");
            AvatarButton.Image.File = file.Path;
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
