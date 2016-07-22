//
//  PostJson.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Yavsc.Helpers
{
    /// <summary>
    /// Simple json post method.
    /// </summary>
    public class NotSoSimpleJsonPostMethod : IDisposable
    {
        private HttpWebRequest request = null;

        /// <summary>
        /// Initializes a new instance of the Yavsc.Helpers.SimpleJsonPostMethod class.
        /// </summary>
        /// <param name="pathToMethod">Path to method.</param>
        public NotSoSimpleJsonPostMethod(string pathToMethod, string authorizationHeader = null)
        {
            request = (HttpWebRequest)WebRequest.Create(pathToMethod);
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            // request.SendChunked = true;
            // request.TransferEncoding = "UTF-8";
            request.Headers = new WebHeaderCollection();
            if (authorizationHeader != null)
                request.Headers["Authorization"] = "Bearer " + authorizationHeader;

        }

        public void Dispose()
        {
            request.Abort();
        }

        /// <summary>
        /// Invoke the specified query.
        /// </summary>
        /// <param name="query">Query.</param>
        public async Task<TAnswer> Invoke<TAnswer>(object query)
        {

            using (Stream streamQuery = await request.GetRequestStreamAsync())
            {
                using (StreamWriter writer = new StreamWriter(streamQuery))
                {
                    writer.Write(JsonConvert.SerializeObject(query));
                }
            }
            TAnswer ans = default(TAnswer);
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader rdr = new StreamReader(responseStream))
                    {
                        ans = (TAnswer)JsonConvert.DeserializeObject<TAnswer>(rdr.ReadToEnd());
                    }
                }
            }
            return ans;
        }

        public async Task<object> InvokeAnonymous(object query)
        {
            object result = null;
            using (Stream streamQuery = await request.GetRequestStreamAsync())
            {
                using (StreamWriter writer = new StreamWriter(streamQuery))
                {
                    var json = JsonConvert.SerializeObject(query);

                    writer.Write(json);

                }
            }
            try
            {
                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream.Length > 0)
                        {
                            using (var sr = new StreamReader(stream))
                            {
                                var json = await sr.ReadToEndAsync();
                                result = JsonConvert.DeserializeObject(json);

                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message.ToString());
            }
            return result;
        }
    }
}

