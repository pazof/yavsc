
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ZicMoove.Helpers
{
    using Data.NonCrUD;
    using Model.FileSystem;

    public static class UserHelpers
    {
        public static ImageSource Avatar(string username)
        {
            var result = username == null ?
                ImageSource.FromResource( "ZicMoove.Images.Users.icon_user.png") :
                ImageSource.FromUri(new Uri(Constants.YavscHomeUrl+ $"/Avatars/{username}.png")) ;
            return result;
        }

        public static ImageSource SmallAvatar(string avatarPath, string username)
        {
            return avatarPath == null ?
                ImageSource.FromResource("ZicMoove.Images.Users.icon_user.png") :
             ImageSource.FromUri(new Uri($"{Constants.YavscHomeUrl}/Avatars/{username}.s.png"));
        }

        public static ImageSource ExtraSmallAvatar(string avatarPath, string username)
        {
            return avatarPath == null ?
                   ImageSource.FromResource("ZicMoove.Images.Users.icon_user.png") :
             ImageSource.FromUri(new Uri($"{Constants.YavscHomeUrl}/Avatars/{username}.xs.png"));
        }

        public static HttpClient CreateJsonClient()
        {
            return CreateJsonClient(MainSettings.CurrentUser.YavscTokens.AccessToken);
        }

        public static HttpClient CreateJsonClient(string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", accessToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }

        /// <summary>
        /// Uploads the given stream to
        /// /api/fs, in order to be saved under the given file name
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<bool> Upload(Stream inputStream, string fileName)
        {
            using (var client = CreateJsonClient())
            {
                using (var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(inputStream), "Files", fileName);

                    using (
                       var message =
                           await client.PostAsync(Constants.FsUrl, content))
                    {
                        return (message.IsSuccessStatusCode);
                    }
                }
            }
        }

    }
}
