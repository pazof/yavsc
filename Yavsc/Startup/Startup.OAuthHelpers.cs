using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OAuth.AspNet.AuthServer;
using Yavsc.Models;

namespace Yavsc
{
    public partial class Startup
    {
        private Application GetApplication(string clientId)
        {
            var dbContext = new ApplicationDbContext();
            var app = dbContext.Applications.FirstOrDefault(x=>x.ApplicationID == clientId);
            return app;
        }
        private readonly ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        private Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context==null) throw new InvalidOperationException("context == null");
            var app = GetApplication(context.ClientId);
            if (app == null) return Task.FromResult(0);
            Startup.logger.LogInformation($"ValidateClientRedirectUri: Validated ({app.RedirectUri})");
            context.Validated(app.RedirectUri);
            return Task.FromResult(0);
        }

        private Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId,clientSecret;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                var app = GetApplication(clientId);
                if (app != null && app.Secret == clientSecret)
                {
                    Startup.logger.LogInformation($"ValidateClientAuthentication: Validated ({clientId})");
                    context.Validated();
                } else Startup.logger.LogInformation($"ValidateClientAuthentication: KO ({clientId})");
            }
            else Startup.logger.LogInformation($"ValidateClientAuthentication: nor Basic neither Form credential found");
            return Task.FromResult(0);
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
            Startup.logger.LogInformation("CreateAuthenticationCode");
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
            var uid = context.Ticket.Principal.GetUserId();
            Startup.logger.LogInformation($"CreateRefreshToken for {uid}");
            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            var uid = context.Ticket.Principal.GetUserId();
            Startup.logger.LogInformation($"ReceiveRefreshToken for {uid}");
            context.DeserializeTicket(context.Token);
        }
    }
}
