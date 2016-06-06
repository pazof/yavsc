using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Yavsc.Auth
{
    internal class GoogleHandler : OAuthHandler<YavscGoogleOptions>
    {
        private ILogger _logger;
        public GoogleHandler(HttpClient httpClient,ILogger logger)
            : base(httpClient)
        {
            _logger = logger;
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity,
        AuthenticationProperties properties, OAuthTokenResponse tokens
)
        {
            // TODO use Options.AuthenticationType instead of Bearer ?
            // Get the Google user
            var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            var response = await Backchannel.SendAsync(request, Context.RequestAborted);
            response.EnsureSuccessStatusCode();

            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());

            var identifier = GoogleHelper.GetId(payload);


            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), properties, Options.AuthenticationScheme);
            var context = new GoogleOAuthCreatingTicketContext(Context, Options, Backchannel, tokens, ticket, identifier);

            if (!string.IsNullOrEmpty(identifier))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identifier, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var givenName = GoogleHelper.GetGivenName(payload);
            if (!string.IsNullOrEmpty(givenName))
            {
                identity.AddClaim(new Claim(ClaimTypes.GivenName, givenName, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var familyName = GoogleHelper.GetFamilyName(payload);
            if (!string.IsNullOrEmpty(familyName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Surname, familyName, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var name = GoogleHelper.GetName(payload);
            if (!string.IsNullOrEmpty(name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, name, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var email = GoogleHelper.GetEmail(payload);
            if (!string.IsNullOrEmpty(email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, email, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var profile = GoogleHelper.GetProfile(payload);
            if (!string.IsNullOrEmpty(profile))
            {
                identity.AddClaim(new Claim("urn:google:profile", profile, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            await Options.Events.CreatingTicket(context);

            return ticket;
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

            AddQueryString(queryStrings, properties, "access_type", Options.AccessType);
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
    }
}
