using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using OAuth.AspNet.AuthServer;
using Yavsc.Models;
using Yavsc.Models.Auth;

namespace Yavsc
{
    public partial class Startup
    {
        private Client GetApplication(string clientId)
        {
            Client app = null;
            using (var dbContext = new ApplicationDbContext())
            {
                app = dbContext.Applications.FirstOrDefault(x => x.Id == clientId);
            }
            return app;
        }
        private readonly ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        private Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context == null) throw new InvalidOperationException("context == null");
            var app = GetApplication(context.ClientId);
            if (app == null) return Task.FromResult(0);
            Startup.logger.LogInformation($"ValidateClientRedirectUri: Validated ({app.RedirectUri})");
            context.Validated(app.RedirectUri);
            return Task.FromResult(0);
        }

        private Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId, clientSecret;

            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                logger.LogInformation($"ValidateClientAuthentication: Got id&secret: ({clientId} {clientSecret})");
                var client = GetApplication(clientId);
                if (client.Type == ApplicationTypes.NativeConfidential)
                {
                    if (string.IsNullOrWhiteSpace(clientSecret))
                    {
                        context.SetError("invalid_clientId", "Client secret should be sent.");
                        return Task.FromResult<object>(null);
                    }
                    else
                    {
                        //  if (client.Secret != Helper.GetHash(clientSecret))
                        // TODO store a hash in db, not the pass
                        if (client.Secret != clientSecret)
                        {
                            context.SetError("invalid_clientId", "Client secret is invalid.");
                            return Task.FromResult<object>(null);
                        }
                    }
                }

                if (!client.Active)
                {
                    context.SetError("invalid_clientId", "Client is inactive.");
                    return Task.FromResult<object>(null);
                }

                if (client != null && client.Secret == clientSecret)
                {
                    logger.LogInformation($"\\o/ ValidateClientAuthentication: Validated ({clientId})");
                    context.Validated();
                }
                else Startup.logger.LogInformation($":'( ValidateClientAuthentication: KO ({clientId})");
            }
            else Startup.logger.LogWarning($"ValidateClientAuthentication: neither Basic nor Form credential were found");
            return Task.FromResult(0);
        }
        UserManager<ApplicationUser> _usermanager;
           
        private async Task<Task> GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            logger.LogWarning($"GrantResourceOwnerCredentials task ... {context.UserName}");

            ApplicationUser user = null;
            user = await _usermanager.FindByNameAsync(context.UserName);
            if (await _usermanager.CheckPasswordAsync(user, context.Password))
            {

                var claims = new List<Claim>(
                        context.Scope.Select(x => new Claim("urn:oauth:scope", x))
                );
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
                claims.AddRange((await _usermanager.GetRolesAsync(user)).Select(
                    r => new Claim(ClaimTypes.Role, r)
                ));
                ClaimsPrincipal principal = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new GenericIdentity(context.UserName, OAuthDefaults.AuthenticationType),
                        claims)
                        );
                // TODO set a NameIdentifier, roles and scopes claims
                context.HttpContext.User = principal;

                context.Validated(principal);
            }

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
            logger.LogInformation("CreateAuthenticationCode");
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _authenticationCodes[context.Token] = context.SerializeTicket();
        }

        private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;
            if (_authenticationCodes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
                logger.LogInformation("ReceiveAuthenticationCode: Success");
            }
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            var uid = context.Ticket.Principal.GetUserId();
            logger.LogInformation($"CreateRefreshToken for {uid}");
            foreach (var c in context.Ticket.Principal.Claims)
                logger.LogInformation($"| User claim: {c.Type} {c.Value}");

            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            var uid = context.Ticket.Principal.GetUserId();
            logger.LogInformation($"ReceiveRefreshToken for {uid}");
            foreach (var c in context.Ticket.Principal.Claims)
                logger.LogInformation($"| User claim: {c.Type} {c.Value}");
            context.DeserializeTicket(context.Token);
        }
    }
}
