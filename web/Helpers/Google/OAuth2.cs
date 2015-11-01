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
using Yavsc.Helpers.Google;

namespace Yavsc.Helpers.Google
{

	/// <summary>
	/// Google O auth2 client.
	/// </summary>
	public class OAuth2 : ApiClient
	{
		/// <summary>
		/// The URI used to get tokens.
		/// </summary>
		protected static string tokenUri = "https://accounts.google.com/o/oauth2/token";

		/// <summary>
		/// The URI used to get authorized to.
		/// </summary>
		protected static string authUri = "https://accounts.google.com/o/oauth2/auth";

		/// <summary>
		/// Gets or sets the redirect URI sent to Google.
		/// </summary>
		/// <value>The redirect URI.</value>
		public string RedirectUri { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Helpers.Google.OAuth2"/> class.
		/// </summary>
		/// <param name="redirectUri">Redirect URI.</param>
		public OAuth2 (string redirectUri, string clientId, string clientSecret)
		{
			RedirectUri = redirectUri;
			CLIENT_ID = clientId;
			CLIENT_SECRET = clientSecret;
		}

		/// <summary>
		/// Login with Google
		/// by redirecting the specified http web response bresp,
		/// and using the specified session state.
		/// </summary>
		/// <param name="bresp">Bresp.</param>
		/// <param name="state">State.</param>
		public void Login (HttpResponseBase bresp, string state)
		{
			string scope = string.Join ("%20", scopeOpenid);

			string prms = String.Format ("response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}&include_granted_scopes=false",
				              CLIENT_ID, RedirectUri, scope, state);
			GetAuthResponse (bresp, prms);
		}

		/// <summary>
		/// Gets the cal authorization.
		/// </summary>
		/// <param name="bresp">Bresp.</param>
		/// <param name="state">State.</param>
		public void GetCalendarScope (HttpResponseBase bresp, string state)
		{
			string prms = String.Format ("response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}&include_granted_scopes=true&access_type=offline&approval_prompt=force",
				CLIENT_ID, RedirectUri, scopeCalendar, state);
			GetAuthResponse (bresp, prms);
		}

		private void GetAuthResponse (HttpResponseBase bresp, string prms)
		{
			string cont = null;
			WebRequest wr = WebRequest.Create (authUri + "?" + prms);
			wr.Method = "GET";
			using (WebResponse response = wr.GetResponse ()) {
				string resQuery = response.ResponseUri.Query;
				cont = HttpUtility.ParseQueryString (resQuery) ["continue"];
				response.Close ();
			}
			wr.Abort ();
			bresp.Redirect (cont);
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
		public static string TokenPostDataFromCode (string redirectUri, string code)
		{
			string postdata = 
				string.Format (
					"redirect_uri={0}&client_id={1}&client_secret={2}&code={3}&grant_type=authorization_code",
					HttpUtility.UrlEncode (redirectUri),
					HttpUtility.UrlEncode (CLIENT_ID),
					HttpUtility.UrlEncode (CLIENT_SECRET),
					HttpUtility.UrlEncode (code));
			return postdata;
		}

		/// <summary>
		/// Gets the Google Authorization token.
		/// </summary>
		/// <returns>The token.</returns>
		/// <param name="rq">Rq.</param>
		/// <param name="state">State.</param>
		/// <param name="message">Message.</param>
		public AuthToken GetToken (HttpRequestBase rq, string state, out string message)
		{
			string code = OAuth2.GetCodeFromRequest (rq, state, out message);
			string postdata = OAuth2.TokenPostDataFromCode (RedirectUri, code);
			return GetTokenPosting (postdata);
		}

		internal static AuthToken GetTokenPosting (string postdata)
		{
			HttpWebRequest webreq = WebRequest.CreateHttp (tokenUri);
			webreq.Method = "POST";
			webreq.Accept = "application/json";
			webreq.ContentType = "application/x-www-form-urlencoded";
			Byte[] bytes = System.Text.Encoding.UTF8.GetBytes (postdata);
			webreq.ContentLength = bytes.Length;

			using (Stream dataStream = webreq.GetRequestStream ()) {
				dataStream.Write (bytes, 0, bytes.Length);
				dataStream.Close ();
			}

			AuthToken gat = null;
			using (WebResponse response = webreq.GetResponse ()) {
				using (Stream responseStream = response.GetResponseStream ()) {
					gat = (AuthToken)new DataContractJsonSerializer (typeof(AuthToken)).ReadObject (responseStream);
					responseStream.Close ();
				}
				response.Close ();
			}
			webreq.Abort ();
			return gat;
		}

		/// <summary>
		/// Gets the code from the Google request.
		/// </summary>
		/// <returns>The code from request.</returns>
		/// <param name="rq">Rq.</param>
		/// <param name="state">State.</param>
		/// <param name="message">Message.</param>
		public static string GetCodeFromRequest (HttpRequestBase rq, string state, out string message)
		{
			message = "";
			string code = rq.Params ["code"];
			string error = rq.Params ["error"];
			if (error != null) {
				message = 
					string.Format (LocalizedText.Google_error,
					LocalizedText.ResourceManager.GetString (error));
				return null;
			}
			string rqstate = rq.Params ["state"];
			if (state != null && string.Compare (rqstate, state) != 0) {
				message = 
					LocalizedText.ResourceManager.GetString ("invalid request state");
				return null;
			}
			return code;
		}

		/// <summary>
		/// Invalid O auth2 refresh token.
		/// </summary>
		public class InvalidOAuth2RefreshToken: Exception
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="Yavsc.Helpers.Google.OAuth2.InvalidOAuth2RefreshToken"/> class.
			/// </summary>
			/// <param name="message">Message.</param>
			public InvalidOAuth2RefreshToken(string message):base(message)
			{
			}
			/// <summary>
			/// Initializes a new instance of the <see cref="Yavsc.Helpers.Google.OAuth2.InvalidOAuth2RefreshToken"/> class.
			/// </summary>
			/// <param name="message">Message.</param>
			/// <param name="innerException">Inner exception.</param>
			public InvalidOAuth2RefreshToken(string message,Exception innerException):base(message,innerException)
			{
			}
		}

		/// <summary>
		/// Gets fresh google credential.
		/// </summary>
		/// <returns>The fresh google credential.</returns>
		/// <param name="pr">Pr.</param>
		public static string GetFreshGoogleCredential (ProfileBase pr)
		{
			string token = (string)pr.GetPropertyValue ("gtoken");
			string token_type = (string) pr.GetPropertyValue ("gtokentype");
			DateTime token_exp = (DateTime) pr.GetPropertyValue ("gtokenexpir");
			if (token_exp < DateTime.Now) {
				object ort = pr.GetPropertyValue ("grefreshtoken");
				if (ort is DBNull || string.IsNullOrWhiteSpace((string)ort)) {
					throw new InvalidOAuth2RefreshToken ("Google");
				}
				string refresh_token = ort as string;
				AuthToken gat = OAuth2.GetTokenPosting (
						string.Format ("grant_type=refresh_token&client_id={0}&client_secret={1}&refresh_token={2}",
						CLIENT_ID, CLIENT_SECRET, refresh_token));
				token = gat.access_token;
				pr.SetPropertyValue ("gtoken", token);
				pr.Save ();
				// ASSERT gat.token_type == pr.GetPropertyValue("gtokentype")
			}
			return token_type + " " + token;
		}

	}
}

