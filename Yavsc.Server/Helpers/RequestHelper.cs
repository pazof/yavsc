using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Yavsc.Server.Model;

namespace Yavsc.Server.Helpers
{
    /// <summary>
    /// Thanks to Stefan @ Stackoverflow
    /// </summary>
    public class RequestHelper
    {
        string WRPostMultipart(string url, Dictionary<string, object> parameters, string authorizationHeader = null)
        {

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            if (authorizationHeader != null)
                request.Headers["Authorization"] = authorizationHeader;
            if (parameters != null && parameters.Count > 0)
            {

                using (Stream requestStream = request.GetRequestStream())
                {
                    using (WebResponse response = request.GetResponse())
                    {


                        foreach (KeyValuePair<string, object> pair in parameters)
                        {

                            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                            if (pair.Value is FormFile)
                            {
                                FormFile file = pair.Value as FormFile;
                                string header = "Content-Disposition: form-data; name=\"" + pair.Key + "\"; filename=\"" + file.Name + "\"\r\nContent-Type: " + file.ContentType + "\r\n\r\n";
                                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(header);
                                requestStream.Write(bytes, 0, bytes.Length);
                                byte[] buffer = new byte[32768];
                                int bytesRead;
                                if (file.Stream == null)
                                {
                                    // upload from file
                                    using (FileStream fileStream = File.OpenRead(file.FilePath))
                                    {
                                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                            requestStream.Write(buffer, 0, bytesRead);
                                        fileStream.Close();
                                    }
                                }
                                else
                                {
                                    // upload from given stream
                                    while ((bytesRead = file.Stream.Read(buffer, 0, buffer.Length)) != 0)
                                        requestStream.Write(buffer, 0, bytesRead);
                                }
                            }
                            else
                            {
                                string data = "Content-Disposition: form-data; name=\"" + pair.Key + "\"\r\n\r\n" + pair.Value;
                                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                                requestStream.Write(bytes, 0, bytes.Length);
                            }

                        }

                        byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                        requestStream.Write(trailer, 0, trailer.Length);
                        requestStream.Close();

                        using (Stream responseStream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            return reader.ReadToEnd();
                        }
                    } // end WebResponse response

                } // end using requestStream

            }
            else throw new ArgumentOutOfRangeException("no parameter found ");

        }

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
