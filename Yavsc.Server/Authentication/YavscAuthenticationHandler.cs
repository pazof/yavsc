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
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Yavsc.Model.Authentication
{
	using Owin;
	public static class YavscAuthenticationExtensions
	{
		public static IAppBuilder UseYavscAuthentication(this IAppBuilder app, YavscAuthenticationOptions options)
		{
			return app.Use(typeof(YavscAuthenticationMiddleware), app, options);
		}
	}

	// Created by the factory in the DummyAuthenticationMiddleware class.
	class YavscAuthenticationHandler : AuthenticationHandler<YavscAuthenticationOptions>
	{
		protected override Task ApplyResponseGrantAsync()
		{
			return base.ApplyResponseGrantAsync();
		}

		protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
		{
			AuthenticationProperties properties = null;
			return Task.FromResult<AuthenticationTicket>(new AuthenticationTicket(null, properties));
		}

		#if FALSE
		protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
		{
			AuthenticationProperties properties = null;

			try
			{
				// ASP.Net Identity requires the NameIdentitifer field to be set or it won't  
				// accept the external login (AuthenticationManagerExtensions.GetExternalLoginInfo)

				string code = null;
				string state = null;

				IReadableStringCollection query = Request.Query;
				IList<string> values = query.GetValues("code");
				if (values != null && values.Count == 1)
				{
					code = values[0];
				}
				values = query.GetValues("state");
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
				if (!ValidateCorrelationId(properties, _logger))
				{
					return new AuthenticationTicket(null, properties);
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
					await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(body));
				tokenResponse.EnsureSuccessStatusCode();
				string text = await tokenResponse.Content.ReadAsStringAsync();

				// Deserializes the token response
				JObject response = JObject.Parse(text);
				string accessToken = response.Value<string>("access_token");

				if (string.IsNullOrWhiteSpace(accessToken))
				{
					_logger.WriteWarning("Access token was not found");
					return new AuthenticationTicket(null, properties);
				}

				// Get the Google user
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				HttpResponseMessage graphResponse = await _httpClient.SendAsync(request, Request.CallCancelled);
				graphResponse.EnsureSuccessStatusCode();
				text = await graphResponse.Content.ReadAsStringAsync();
				JObject user = JObject.Parse(text);

				var context = new GoogleOAuth2AuthenticatedContext(Context, user, response);
				context.Identity = new ClaimsIdentity(
					Options.AuthenticationType,
					ClaimsIdentity.DefaultNameClaimType,
					ClaimsIdentity.DefaultRoleClaimType);
				if (!string.IsNullOrEmpty(context.Id))
				{
					context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.Id, 
					                                ClaimValueTypes.String, Options.AuthenticationType));
				}
				if (!string.IsNullOrEmpty(context.GivenName))
				{
					context.Identity.AddClaim(new Claim(ClaimTypes.GivenName, context.GivenName, 
					                                ClaimValueTypes.String, Options.AuthenticationType));
				}
				if (!string.IsNullOrEmpty(context.FamilyName))
				{
					context.Identity.AddClaim(new Claim(ClaimTypes.Surname, context.FamilyName,
					                                ClaimValueTypes.String, Options.AuthenticationType));
				}                
				if (!string.IsNullOrEmpty(context.Name))
				{
					context.Identity.AddClaim(new Claim(ClaimTypes.Name, context.Name, ClaimValueTypes.String,
					                                Options.AuthenticationType));
				}
				if (!string.IsNullOrEmpty(context.Email))
				{
					context.Identity.AddClaim(new Claim(ClaimTypes.Email, context.Email, ClaimValueTypes.String, 
					                                Options.AuthenticationType));
				}

				if (!string.IsNullOrEmpty(context.Profile))
				{
					context.Identity.AddClaim(new Claim("urn:google:profile", context.Profile, ClaimValueTypes.String, 
					                                Options.AuthenticationType));
				}
				context.Properties = properties;

				await Options.Provider.Authenticated(context);

				return new AuthenticationTicket(context.Identity, context.Properties);
			}
			catch (Exception ex)
			{
				_logger.WriteError("Authentication failed", ex);
				return new AuthenticationTicket(null, properties);
			}
			
		}
		#endif 
		protected override Task ApplyResponseChallengeAsync()
		{
			if (Response.StatusCode == 401)
			{
				var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

				// Only react to 401 if there is an authentication challenge for the authentication 
				// type of this handler.
				if (challenge != null)
				{
					var state = challenge.Properties;

					if (string.IsNullOrEmpty(state.RedirectUri))
					{
						state.RedirectUri = Request.Uri.ToString();
					}
					var chprops = challenge.Properties;
					var stateString = Options.StateDataFormat.Protect(state);
					string scopeParam = string.Join (" ", Options.Scope);


					var extRedirectUri = new Uri(Request.Uri.AbsoluteUri);
					var extRedirectUriStr = extRedirectUri.Scheme + "://" +
														  extRedirectUri.Authority ;
					              
					var redirectUri = WebUtilities.AddQueryString(Options.AuthenticationServerUri, "response_type", "code");

					redirectUri = WebUtilities.AddQueryString(redirectUri, "client_id", Options.ClientId);



					redirectUri = WebUtilities.AddQueryString(redirectUri, "redirect_uri", 
					                                                                extRedirectUriStr+
					                                                                  Options.ReturnUrl.ToUriComponent());
					redirectUri = WebUtilities.AddQueryString(redirectUri, "scope", scopeParam);
					redirectUri = WebUtilities.AddQueryString(redirectUri, "state", stateString);
					redirectUri = WebUtilities.AddQueryString(redirectUri, "include_granted_scopes", "true");
					redirectUri = WebUtilities.AddQueryString(redirectUri, "approval_prompt", "force");
					var env = Context.Environment;
					Response.Redirect(redirectUri);
				}
			}

			return Task.FromResult<object>(null);
		}

		public override async Task<bool> InvokeAsync()
		{
			// This is always invoked on each request. For passive middleware, only do anything if this is
			// for our callback path when the user is redirected back from the authentication provider.
			if (Options.ReturnUrl.HasValue && Options.ReturnUrl == Request.Path)
			{
				var ticket = await AuthenticateAsync();

				if (ticket != null)
				{
					Context.Authentication.SignIn(ticket.Properties, ticket.Identity);

					Response.Redirect(ticket.Properties.RedirectUri);

					// Prevent further processing by the owin pipeline.
					return true;
				}
			}
			// Let the rest of the pipeline run.
			return false;
		}
	}
}

