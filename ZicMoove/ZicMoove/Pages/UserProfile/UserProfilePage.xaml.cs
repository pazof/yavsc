using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using Xamarin.Forms;
using System.Net.Http;
using Android.Graphics;

namespace ZicMoove.Pages.UserProfile
{
    using ViewModels.UserProfile;
    using Helpers;
    using System.IO;
    using System.Threading.Tasks;
    using Android.Content.Res;

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
            IsBusy = true;
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                IsBusy = false;
                return;
            }
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "Avatars",
                Name = "me.jpg",
                DefaultCamera = CameraDevice.Front
            });

            if (file == null)
            {
                IsBusy = false;
                return;
            }
            if (TargetPlatform.Android == Device.OS)
            {
                // Rotate if needed
                BitmapFactory.Options options = new BitmapFactory.Options { };
                Android.Media.ExifInterface exif = new Android.Media.ExifInterface(file.Path);
                var orientation = exif.GetAttributeInt(
                    Android.Media.ExifInterface.TagOrientation, 0);
                using (var stream = file.GetStream())
                {
                    var bmp = await BitmapFactory.DecodeStreamAsync(stream);

                    // Next we calculate the ratio that we need to resize the image by
                    // in order to fit the requested dimensions.
                    int outHeight = options.OutHeight;
                    int outWidth = options.OutWidth;
                    Matrix mtx = new Matrix();

                    Bitmap nbmp = null;
                    switch (orientation)
                    {
                        case (int)Orientation.Undefined: // Nexus 7 landscape...
                            mtx.PreRotate(180);
                            nbmp = Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, mtx, false);
                            mtx.Dispose();
                            mtx = null;
                            break;
                        case (int)Orientation.Landscape:
                            break;
                        case 1: // landscape left
                        case 3: // Landskape right
                            mtx.PreRotate(180);
                            nbmp = Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, mtx, false);
                            break;
                        case 6: // portrait
                            mtx.PreRotate(90);
                            nbmp = Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, mtx, false);
                            break;
                        case 8: // my portrait
                            mtx.PreRotate(-90);
                            nbmp = Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, mtx, false);
                            break;
                    }
                    if (nbmp != null)
                    {
                        using (var ostream = new MemoryStream())
                        {
                            nbmp.Compress(Bitmap.CompressFormat.Png, 10, ostream);
                            ostream.Seek(0, SeekOrigin.Begin);
                            var ok = await SendAvatarStreamAsync(ostream);
                        }
                        nbmp.Dispose();
                    }
                    else
                    {
                        using (var ostream = new MemoryStream())
                        {
                            bmp.Compress(Bitmap.CompressFormat.Png, 10, ostream);
                            ostream.Seek(0, SeekOrigin.Begin);
                            var ok = await SendAvatarStreamAsync(ostream);
                        }
                    }
                    mtx.Dispose();
                    bmp.Dispose();
                }
                exif.Dispose();
                options.Dispose();
            }
            else
            {
                using (var stream = file.GetStream())
                {
                    var ok = await SendAvatarStreamAsync(stream);
                }
            }
            IsBusy = false;
        }

        private async Task<bool> SendAvatarStreamAsync(Stream stream)
        {
            using (var client = UserHelpers.CreateJsonClient())
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
                        return true;
                    }
                }
            }
            return false;
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
