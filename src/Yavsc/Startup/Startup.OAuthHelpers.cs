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
            if (_dbContext==null)
              _logger.LogError("no db!");
            Client app =  _dbContext.Applications.FirstOrDefault(x => x.Id == clientId);
            if (app==null)
              _logger.LogError($"no app for <{clientId}>");
            return app;
        }
        private readonly ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        private Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context == null) throw new InvalidOperationException("context == null");
            var app = GetApplication(context.ClientId);
            if (app == null) return Task.FromResult(0);
            Startup._logger.LogInformation($"ValidateClientRedirectUri: Validated ({app.RedirectUri})");
            context.Validated(app.RedirectUri);
            return Task.FromResult(0);
        }

        private Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId, clientSecret;

            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                _logger.LogInformation($"ValidateClientAuthentication: Got id: ({clientId} secret: {clientSecret})");
                var client = GetApplication(clientId);
                if (client==null) {
                    context.SetError("invalid_clientId", "Client secret is invalid.");
                    return Task.FromResult<object>(null);
                } else 
                if (client.Type == ApplicationTypes.NativeConfidential)
                {
                    _logger.LogInformation($"NativeConfidential key");
                    if (string.IsNullOrWhiteSpace(clientSecret))
                    {
                        _logger.LogInformation($"invalid_clientId: Client secret should be sent.");
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
                            _logger.LogInformation($"invalid_clientId: Client secret is invalid.");
                            return Task.FromResult<object>(null);
                        }
                    }
                }

                if (!client.Active)
                {
                    context.SetError("invalid_clientId", "Client is inactive.");
                    _logger.LogInformation($"invalid_clientId: Client is inactive.");
                    return Task.FromResult<object>(null);
                }

                if (client != null && client.Secret == clientSecret)
                {
                    _logger.LogInformation($"\\o/ ValidateClientAuthentication: Validated ({clientId})");
                    context.Validated();
                }
                else _logger.LogInformation($":'( ValidateClientAuthentication: KO ({clientId})");
            }
            else _logger.LogWarning($"ValidateClientAuthentication: neither Basic nor Form credential were found");
            return Task.FromResult(0);
        }
        UserManager<ApplicationUser> _usermanager;
           
        private async Task<Task> GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            _logger.LogWarning($"GrantResourceOwnerCredentials task ... {context.UserName}");

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
            _logger.LogInformation("CreateAuthenticationCode");
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _authenticationCodes[context.Token] = context.SerializeTicket();
        }

        private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;
            if (_authenticationCodes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
                _logger.LogInformation("ReceiveAuthenticationCode: Success");
            }
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            var uid = context.Ticket.Principal.GetUserId();
            _logger.LogInformation($"CreateRefreshToken for {uid}");
            foreach (var c in context.Ticket.Principal.Claims)
                _logger.LogInformation($"| User claim: {c.Type} {c.Value}");

            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            var uid = context.Ticket.Principal.GetUserId();
            _logger.LogInformation($"ReceiveRefreshToken for {uid}");
            foreach (var c in context.Ticket.Principal.Claims)
                _logger.LogInformation($"| User claim: {c.Type} {c.Value}");
            context.DeserializeTicket(context.Token);
        }
    }
}
