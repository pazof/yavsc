//
//  YavscAuthenticationHandler.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
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
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Yavsc.Authentication
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.AspNet.Authentication;
    using Microsoft.AspNet.Authentication.OAuth;
    using Microsoft.AspNet.Http;
    using Microsoft.AspNet.Http.Authentication;
    using Microsoft.AspNet.Http.Features.Authentication;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    public static class YavscAuthenticationExtensions
	{
	}

	class YavscAuthenticationHandler : OAuthHandler<YavscAuthenticationOptions>
	{
		private ILogger _logger;
		private HttpClient _backchannel;

		public YavscAuthenticationHandler(HttpClient backchannel, ILogger logger) : base (backchannel)
		{
			_backchannel = backchannel;
			_logger = logger;
		}

		protected new async Task<AuthenticationTicket> AuthenticateAsync(AuthenticateContext context)
		{
			AuthenticationProperties properties = null;

			try
			{
				// ASP.Net Identity requires the NameIdentitifer field to be set or it won't
				// accept the external login (AuthenticationManagerExtensions.GetExternalLoginInfo)

				string code = null;
				string state = null;

				IReadableStringCollection query = Request.Query;
				IList<string> values = query["code"];
				if (values != null && values.Count == 1)
				{
					code = values[0];
				}
				values = query["state"];
				if (values != null && values.Count == 1)
				{
					state = values[0];
				}

				properties = Options.StateDataFormat.Unprotect(state);
				if (properties == null)
				{
					return null;
				}

				// OAuth2 10.12 CSRF
				if (!ValidateCorrelationId(properties))
				{
					return new AuthenticationTicket(null, properties, this.Options.AuthenticationScheme);
				}

				string requestPrefix = Request.Scheme + "://" + Request.Host;
				string redirectUri = requestPrefix + Request.PathBase + Options.CallbackPath;

				// Build up the body for the token request
				var body = new List<KeyValuePair<string, string>>();
				body.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
				body.Add(new KeyValuePair<string, string>("code", code));
				body.Add(new KeyValuePair<string, string>("redirect_uri", redirectUri));
				body.Add(new KeyValuePair<string, string>("client_id", Options.ClientId));
				body.Add(new KeyValuePair<string, string>("client_secret", Options.ClientSecret));

				// Request the token
				HttpResponseMessage tokenResponse =
					await _backchannel.PostAsync(Options.TokenEndpoint, new FormUrlEncodedContent(body));
				tokenResponse.EnsureSuccessStatusCode();
				string text = await tokenResponse.Content.ReadAsStringAsync();

				// Deserializes the token response
				JObject response = JObject.Parse(text);
				string accessToken = response.Value<string>("access_token");

				if (string.IsNullOrWhiteSpace(accessToken))
				{
					_logger.LogWarning("Access token was not found");
					return new AuthenticationTicket(null, properties, this.Options.AuthenticationScheme);
				}

				// Get the  user
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
				request.Headers.Authorization = new AuthenticationHeaderValue(this.Options.AuthenticationScheme, accessToken);
				HttpResponseMessage graphResponse = await _backchannel.SendAsync(request);
				graphResponse.EnsureSuccessStatusCode();
				text = await graphResponse.Content.ReadAsStringAsync();
				JObject user = JObject.Parse(text);
				// Read user data

				var id = new ClaimsIdentity(
					Options.SignInAsAuthenticationType,
					ClaimsIdentity.DefaultNameClaimType,
					ClaimsIdentity.DefaultRoleClaimType);
				context.Authenticated(new ClaimsPrincipal(id)
					, new Dictionary<string,string>(), new Dictionary<string,object>{
						{ "John" , (object) "Doe" }
					});
				return new AuthenticationTicket(context.Principal, properties, Options.SignInAsAuthenticationType);
			}
			catch (Exception ex)
			{
				_logger.LogError("Authentication failed", ex);
				return new AuthenticationTicket(null, properties, this.Options.AuthenticationScheme);
			}

		}


	}
}

