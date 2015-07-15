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

using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Yavsc.Model.Google;
using System.Web.Profile;
using System.Web;
using Yavsc.Model;
using System.Runtime.Serialization.Json;
using Yavsc.Helpers.Google;

namespace Yavsc.Helpers.Google
{
	/// <summary>
	/// Google People API.
	/// </summary>
	public class PeopleApi: ApiClient
	{
		private static string getPeopleUri = "https://www.googleapis.com/plus/v1/people";

		/// <summary>
		/// Gets the People object associated to the given Google Access Token
		/// </summary>
		/// <returns>The me.</returns>
		/// <param name="gat">The Google Access Token object <see cref="AuthToken"/> class.</param>
		public static People GetMe (AuthToken gat)
		{
			People me;
			DataContractJsonSerializer ppser = new DataContractJsonSerializer (typeof(People));
			HttpWebRequest webreppro = WebRequest.CreateHttp (getPeopleUri + "/me");
			webreppro.ContentType = "application/http";
			webreppro.Headers.Add (HttpRequestHeader.Authorization, gat.token_type + " " + gat.access_token);
			webreppro.Method = "GET";
			using (WebResponse proresp = webreppro.GetResponse ()) {
				using (Stream prresponseStream = proresp.GetResponseStream ()) {
					me = (People)ppser.ReadObject (prresponseStream);
					prresponseStream.Close ();
				}
				proresp.Close ();
			}
			webreppro.Abort ();
			return me;
		}
	}
	
}
