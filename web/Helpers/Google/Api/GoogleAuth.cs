//
//  OAuth2.cs
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
using Yavsc.Client;
using System.Collections.Generic;
using Yavsc.Model.Identity;

namespace Yavsc.Helpers.OAuth.Api
{

	/// <summary>
	/// Google O auth2 client.
	/// </summary>
	public abstract class GoogleAuth : OAuthAccessPoint
	{
		
		/// <summary>
		/// The Map tracks scope .
		/// </summary>
		protected static string scopeTracks = "https://www.googleapis.com/auth/tracks";

		/// <summary>
		/// The calendar scope.
		/// </summary>
		protected static string scopeCalendar = "https://www.googleapis.com/auth/calendar";

		/// <summary>
		/// The scope openid.
		/// </summary>
		protected static string[] scopeOpenid = { "openid","profile","email"};


		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Helpers.Google.OAuth2"/> class.
		/// </summary>
		/// <param name="redirectUri">Redirect URI.</param>
		/// <param name="clientId">Client identifier.</param>
		/// <param name="clientSecret">Client secret.</param>
		public GoogleAuth (string authType, string redirectUri)
		{
			if (string.IsNullOrWhiteSpace (authType))
				throw new InvalidOperationException ("authType");
			if (string.IsNullOrWhiteSpace (redirectUri))
				throw new InvalidOperationException ("redirectUri");
			RedirectUri = redirectUri;
			AuthType = authType;
			TokenUri = "https://accounts.google.com/o/oauth2/token";
			AuthUri = "https://accounts.google.com/o/oauth2/auth";
		}


		/// <summary>
		/// Gets the cal authorization.
		/// </summary>
		/// <param name="bresp">Bresp.</param>
		/// <param name="state">State.</param>
		public void GetCalendarScope (HttpResponseBase bresp, string state)
		{
			string prms = String.Format ("response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}&include_granted_scopes=true&access_type=offline&approval_prompt=force",
				Id, RedirectUri, scopeCalendar, state);
			InvokeGet (bresp, prms);
		}



		/// <summary>
		/// Builds the post data, from code given 
		/// by Google in the request parameters,
		/// and using the given <c>redirectUri</c>.
		/// This request body is used to get a new
		/// OAuth2 token from Google, it is Url encoded.
		/// </summary>
		/// <returns>The post data from code.</returns>
		/// <param name="redirectUri">Redirect URI.</param>
		/// <param name="code">Code.</param>
		public static string TokenPostDataFromCode (string redirectUri, string code ,string clientid, string secret)
		{
			string postdata = 
				string.Format (
					"redirect_uri={0}&client_id={1}&client_secret={2}&code={3}&grant_type=authorization_code",
					HttpUtility.UrlEncode (redirectUri),
					HttpUtility.UrlEncode (clientid),
					HttpUtility.UrlEncode (secret),
					HttpUtility.UrlEncode (code));
			return postdata;
		}




	}


}

