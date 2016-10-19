using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.Helpers
{
    public static class UserHelpers
    {
        public static ImageSource Avatar(string avatarPath)
        {
            var result = avatarPath == null ?
                ImageSource.FromResource( "BookAStar.Images.Users.icon_user.png") :
                avatarPath.StartsWith("res://") ?
                 ImageSource.FromResource(avatarPath.Substring(6)) :
                 ImageSource.FromUri(new Uri(avatarPath));
            return result;
        }

        public static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", MainSettings.CurrentUser.YavscTokens.AccessToken);
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
            using (var client = CreateClient())
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
