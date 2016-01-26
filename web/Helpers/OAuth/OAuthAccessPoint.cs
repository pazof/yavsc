//
//  Manager.cs
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
using Yavsc.Model.Google;
using System.Net;
using System.IO;
using System.Text;
using System.Configuration;
using System.Collections.Generic;
using Microsoft.Owin.Security;
using Yavsc.Client;
using System.Web;
using System.Web.Profile;
using System.Runtime.Serialization.Json;
using Yavsc.Model.Identity;
using Yavsc.Model.RolesAndMembers;

namespace Yavsc.Helpers.OAuth
{
	/// <summary>
	/// Google base API client.
	/// This class implements the identification values for a Google Api, 
	/// and provides some scope values.
	/// </summary>
	public abstract class OAuthAccessPoint : IIdentified<string>
	{
		private  string clientId=null;
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public  string Id {
			get {
				return clientId;
			}
			set {
				clientId = value;
			}
		}
		/// <summary>
		/// The client secret.
		/// </summary>
		private  string clientSecret=null;

		protected  string Secret {
			get {
				return clientSecret;
			}
		}

		private  string clientApiKey=null;

		/// <summary>
		/// Gets the API key.
		/// </summary>
		/// <value>The API key.</value>
		public  string ApiKey {
			get {
				return clientApiKey;
			}
		}
		/// <summary>
		/// Gets or sets the redirect URI sent to Google.
		/// </summary>
		/// <value>The redirect URI.</value>
		public string RedirectUri { get; set; }

		/// <summary>
		/// Gets or sets the type of the auth.
		/// </summary>
		/// <value>The type of the auth.</value>
		public string AuthType { get; set; }
		/// <summary>
		/// The URI used to get tokens.
		/// </summary>
		public string TokenUri { get; set; } 

		/// <summary>
		/// The URI used to get authorized to.
		/// </summary>
		public string AuthUri { get; set; } 

		protected virtual void InvokeGet (HttpResponseBase bresp, string prms)
		{
			string cont = null;
			WebRequest wr = WebRequest.Create (AuthUri + "?" + prms);
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
		/// Login
		/// by redirecting the specified http web response `bresp`,
		/// and using the specified session state.
		/// </summary>
		/// <param name="bresp">Bresp.</param>
		/// <param name="state">State.</param>
		public void Login (HttpResponseBase bresp, string state, string clientId, IEnumerable<string> scope)
		{
			string scopeParam = string.Join ("%20", scope);
			string prms = String.Format ("response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}&include_granted_scopes=true&approval_prompt=force",
				clientId, RedirectUri, scopeParam, state);
			InvokeGet (bresp, prms);
		}

		/// <summary>
		/// Gets fresh OAuth credential.
		/// </summary>
		/// <returns>The fresh google credential.</returns>
		/// <param name="pr">Pr.</param>
		public virtual AuthToken GetFreshToken (string username, DateTime token_exp, string refresh_token)
		{
			if (token_exp > DateTime.Now.AddSeconds (10))
				return null;
			AuthToken gat = null;
			if (token_exp < DateTime.Now) {
				gat = GetTokenPosting<AuthToken>(
					string.Format ("grant_type=refresh_token&client_id={0}&client_secret={1}&refresh_token={2}",
						Id, Secret, refresh_token));
				UserNameManager.SaveToken (username, gat);
			}
			return gat;
		}

		public string GetCredential(string username)
		{
			string creds = null;
			AuthToken fresh = null;
			SavedToken token = UserNameManager.GetOAuthToken (username, "Google");
			if ((fresh = GetFreshToken (username, token.expires, token.refresh_token))!= null) {
				creds = token.token_type+" "+ fresh.access_token;
			} else {
				creds = token.token_type+" "+ token.access_token;
			}
			return creds;
		}

		/// <summary>
		/// Gets the Authorization token.
		/// </summary>
		/// <returns>The token.</returns>
		/// <param name="rq">Rq.</param>
		/// <param name="state">State.</param>
		/// <param name="message">Message.</param>
		public AuthToken GetToken <TokenType> (HttpRequestBase rq, string state, out string message)
		{
			string code = GetCodeFromRequest (rq, state, out message);
			string postdata = TokenPostDataFromCode (RedirectUri, code);
			return GetTokenPosting <TokenType> (postdata);
		}

		// public static string PublicClientId { get; set; }

		internal AuthToken GetTokenPosting<TokenType> (string postdata)
		{
			HttpWebRequest webreq = WebRequest.CreateHttp (TokenUri);
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
					gat = (AuthToken)new DataContractJsonSerializer (typeof(TokenType)).ReadObject (responseStream);
					responseStream.Close ();
				}
				response.Close ();
			}
			webreq.Abort ();
			return gat;
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
		public string TokenPostDataFromCode (string redirectUri, string code )
		{
			string postdata = 
				string.Format (
					"redirect_uri={0}&client_id={1}&client_secret={2}&code={3}&grant_type=authorization_code",
					HttpUtility.UrlEncode (redirectUri),
					HttpUtility.UrlEncode (Id),
					HttpUtility.UrlEncode (Secret),
					HttpUtility.UrlEncode (code));
			return postdata;
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
	}
	
}
