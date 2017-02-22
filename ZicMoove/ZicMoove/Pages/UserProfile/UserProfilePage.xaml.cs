
using ZicMoove.ViewModels.UserProfile;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;

using Xamarin.Forms;
using ZicMoove.Settings;
using ZicMoove.Helpers;
using System.Net.Http;

namespace ZicMoove.Pages.UserProfile
{
    public partial class UserProfilePage 
    {
        public UserProfilePage()
        {
            InitializeComponent();
            AvatarButton.Clicked += AvatarButton_Clicked;
            btnPay.Clicked += BtnPay_Clicked;

        }

        public UserProfilePage(UserProfileViewModel model)
        {
            InitializeComponent();
            AvatarButton.Clicked += AvatarButton_Clicked;
            BindingContext = model;
        }

        
        private async void BtnPay_Clicked(object sender, EventArgs e)
        {
            App.PlatformSpecificInstance.Pay(0.1, Interfaces.PayMethod.Immediate, "test payment");

        }


        private async void AvatarButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                return;
            }
            IsBusy = true;
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "Avatars",
                Name = "me.jpg"
            });

            if (file == null)
                return;
            using (var client = UserHelpers.CreateJsonClient())
            {
                // Get the whole data
                try
                {
                    using (var stream = file.GetStream())
                    {
                        var requestContent = new MultipartFormDataContent();
                        var content = new StreamContent(stream);
                        var filename = "me.jpg";
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                        content.Headers.Add("Content-Disposition", $"form-data; name=\"file\"; filename=\"{filename}\"");
                        requestContent.Add(content, "file", filename);

                        using (var response = await client.PostAsync(Constants.YavscApiUrl + "/setavatar", requestContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                // TODO image update
                                var recnt = await response.Content.ReadAsStringAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // TODO error report
                }
             }
            IsBusy = false;
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
