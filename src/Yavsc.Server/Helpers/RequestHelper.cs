using System.Net;
using System.Net.Http.Headers;
using Yavsc.Server.Model;

namespace Yavsc.Server.Helpers
{
    /// <summary>
    /// Thanks to Stefan @ Stackoverflow
    /// </summary>
    public class RequestHelper
    {
        public static async Task<string> PostMultipart(string url, FormFile[] formFiles, string access_token = null)
        {

            if (formFiles != null && formFiles.Length > 0)
            {
                var client = new HttpClient();
                var formData = new MultipartFormDataContent();

                    if (access_token != null)
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                    foreach (var formFile in formFiles)
                    {
                        HttpContent fileStreamContent = new StreamContent(formFile.Stream);
                        if (formFile.ContentType!=null)
                            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(formFile.ContentType);
                            else fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                        // fileStreamContent.Headers.ContentDisposition = formFile.ContentDisposition!=null? new ContentDispositionHeaderValue(
                           //     formFile.ContentDisposition) : new ContentDispositionHeaderValue("form-data; name=\"file\"; filename=\"" + formFile.Name + "\"");
                        fileStreamContent.Headers.Add("Content-Disposition", formFile.ContentDisposition);
                        fileStreamContent.Headers.Add("Content-Length", formFile.Stream.Length.ToString());

                        //fileStreamContent.Headers.Add("FilePath", formFile.FilePath);

                        formData.Add(fileStreamContent, "file", formFile.Name);

                    }

                    var response = client.PostAsync(url, formData).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }
                    return await response.Content.ReadAsStringAsync();
                } // end if formFiles != null

            return null;
        }



    }


}
