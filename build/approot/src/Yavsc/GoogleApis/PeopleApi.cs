//
//  PeopleApi.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
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

using System.IO;
using System.Net;
using Newtonsoft.Json;
using Yavsc.Models.Google;

namespace Yavsc.GoogleApis
{
    /// <summary>
    /// Google People API.
    /// </summary>
    public class PeopleApi
    {
        /// <summary>
        /// The get people URI.
        /// </summary>
        protected static string getPeopleUri = "https://www.googleapis.com/plus/v1/people";

        /// <summary>
        /// Initializes a new instance of the <see cref="Yavsc.Helpers.Google.Api.PeopleApi"/> class.
        /// </summary>
        /// <param name="authType">Auth type.</param>
        /// <param name="redirectUri">Redirect URI.</param>
        public PeopleApi()
        { }

        /// <summary>
        /// Gets the People object associated to the given Google Access Token
        /// </summary>
        /// <returns>The me.</returns>
        /// <param name="gat">The Google Access Token object <see cref="AuthToken"/> class.</param>
        public People GetMe(AuthToken gat)
        {
            People me;

            HttpWebRequest webreppro = WebRequest.CreateHttp(getPeopleUri + "/me");
            webreppro.ContentType = "application/http";
            webreppro.Headers.Add(HttpRequestHeader.Authorization, gat.token_type + " " + gat.access_token);
            webreppro.Method = "GET";
            using (WebResponse proresp = webreppro.GetResponse())
            {
                using (Stream prresponseStream = proresp.GetResponseStream())
                {
                    using (StreamReader rdr = new StreamReader(prresponseStream))
                    {
                        me = JsonConvert.DeserializeObject<People>(rdr.ReadToEnd());
                        prresponseStream.Close();
                    }
                    proresp.Close();
                }
                webreppro.Abort();
                return me;
            }
        }

    }

}
