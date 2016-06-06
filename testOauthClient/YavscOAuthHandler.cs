using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Yavsc.Auth
{
    internal class YavscOAuthHandler : OAuthHandler<YavscOAuthOptions>
    {
        private ILogger _logger;
        HttpClient _backchannel;
        private SharedAuthenticationOptions _sharedOptions; 
        public YavscOAuthHandler(HttpClient httpClient, SharedAuthenticationOptions sharedOptions, ILogger logger)
            : base(httpClient)
        {
            _backchannel = httpClient;
            _logger = logger;
            _sharedOptions = sharedOptions;
        }
        // TODO: Abstract this properties override pattern into the base class?
        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var scope = FormatScope();
            var queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            queryStrings.Add("response_type", "code");
            queryStrings.Add("client_id", Options.ClientId);
            queryStrings.Add("redirect_uri", redirectUri);

            AddQueryString(queryStrings, properties, "scope", scope);

            AddQueryString(queryStrings, properties, "access_type", Options.AccessType );
            AddQueryString(queryStrings, properties, "approval_prompt");
            AddQueryString(queryStrings, properties, "login_hint");

            var state = Options.StateDataFormat.Protect(properties);
            queryStrings.Add("state", state);

            var authorizationEndpoint = QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, queryStrings);
            return authorizationEndpoint;
        }

        private static void AddQueryString(IDictionary<string, string> queryStrings, AuthenticationProperties properties,
            string name, string defaultValue = null)
        {
            string value;
            if (!properties.Items.TryGetValue(name, out value))
            {
                value = defaultValue;
            }
            else
            {
                // Remove the parameter from AuthenticationProperties so it won't be serialized to state parameter
                properties.Items.Remove(name);
            }
            queryStrings[name] = value;
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
					_sharedOptions.SignInScheme,
					ClaimsIdentity.DefaultNameClaimType,
					ClaimsIdentity.DefaultRoleClaimType);
				context.Authenticated(new ClaimsPrincipal(id)
					, new Dictionary<string,string>(), new Dictionary<string,object>{
						{ "John" , (object) "Doe" }
					});
				return new AuthenticationTicket(context.Principal, properties, _sharedOptions.SignInScheme);
			}
			catch (Exception ex)
			{
				_logger.LogError("Authentication failed", ex);
				return new AuthenticationTicket(null, properties, this.Options.AuthenticationScheme);
			}

		}
    }
}
