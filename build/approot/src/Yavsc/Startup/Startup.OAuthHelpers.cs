using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using OAuth.AspNet.AuthServer;

namespace Yavsc
{
    public partial class Startup
    {
        private readonly ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        private Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            var app = context.ApplicationStore.FindApplication(context.ClientId);
            if (app!=null)
            {
                context.Validated(app.RedirectUri);
            }
            return Task.FromResult(0);
        }

        private Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId,clientSecret;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                if (ValidateClientCredentials(
                new OAuthValidateClientCredentialsContext(clientId,clientSecret,context.ApplicationStore)
                ))
                {
                    context.Validated();
                }
            }
            return Task.FromResult(0);
        }

        private bool ValidateClientCredentials(OAuthValidateClientCredentialsContext context)
        {
            var authapp = context.ApplicationStore.FindApplication(context.ClientId);
            if (authapp == null) return false;
            if (authapp.Secret == context.ClientSecret) return true;
            return false;
        }

        private Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(new GenericIdentity(context.UserName, OAuthDefaults.AuthenticationType), context.Scope.Select(x => new Claim("urn:oauth:scope", x))));

            context.Validated(principal);

            return Task.FromResult(0);
        }

        private Task GrantClientCredetails(OAuthGrantClientCredentialsContext context)
        {
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(new GenericIdentity(context.ClientId, OAuthDefaults.AuthenticationType), context.Scope.Select(x => new Claim("urn:oauth:scope", x))));

            context.Validated(principal);

            return Task.FromResult(0);
        }

        private void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
        {
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _authenticationCodes[context.Token] = context.SerializeTicket();
        }

        private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;
            if (_authenticationCodes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
            }
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
        }
    }
}
