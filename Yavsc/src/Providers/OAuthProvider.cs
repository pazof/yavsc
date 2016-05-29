using System;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yavsc.Models;

namespace Yavsc.Providers {
    public sealed class AuthorizationProvider : OpenIdConnectServerProvider {

        private ILogger _logger;

        public AuthorizationProvider(ILoggerFactory loggerFactory) {
            _logger = loggerFactory.CreateLogger<AuthorizationProvider>();
        }
        public override Task MatchEndpoint(MatchEndpointContext context) {
            // Note: by default, OpenIdConnectServerHandler only handles authorization requests made to the authorization endpoint.
            // This context handler uses a more relaxed policy that allows extracting authorization requests received at
            // /connect/authorize/accept and /connect/authorize/deny (see AuthorizationController.cs for more information).
            if (context.Options.AuthorizationEndpointPath.HasValue &&
                context.Request.Path.StartsWithSegments(context.Options.AuthorizationEndpointPath)) {
                context.MatchesAuthorizationEndpoint();
            }

            return Task.FromResult<object>(null);
        }

        public override async Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context) {
            // Note: the OpenID Connect server middleware supports the authorization code, implicit and hybrid flows
            // but this authorization provider only accepts response_type=code authorization/authentication requests.
            // You may consider relaxing it to support the implicit or hybrid flows. In this case, consider adding
            // checks rejecting implicit/hybrid authorization requests when the client is a confidential application.
            if (!context.Request.IsAuthorizationCodeFlow()) {
                context.Rejected(
                    error: OpenIdConnectConstants.Errors.UnsupportedResponseType,
                    description: "Only the authorization code flow is supported by this authorization server");

                return;
            }

            var database = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            _logger.LogInformation($"Searching fo app id {context.ClientId}");

            // Retrieve the application details corresponding to the requested client_id.
            var application = await (from entity in database.Applications
                                     where entity.ApplicationID == context.ClientId
                                     select entity).SingleOrDefaultAsync(context.HttpContext.RequestAborted);

            if (application == null) {
                context.Rejected(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Application not found in the database: ensure that your client_id is correct");

                return;
            }

            if (!string.IsNullOrEmpty(context.Request.RedirectUri) &&
                !string.Equals(context.Request.RedirectUri, application.RedirectUri, StringComparison.Ordinal)) {
                context.Rejected(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Invalid redirect_uri");

                return;
            }

            context.Validated();
        }

        public override async Task ValidateTokenRequest(ValidateTokenRequestContext context) {
            // Note: the OpenID Connect server middleware supports authorization code, refresh token, client credentials
            // and resource owner password credentials grant types but this authorization provider uses a safer policy
            // rejecting the last two ones. You may consider relaxing it to support the ROPC or client credentials grant types.
            if (!context.Request.IsAuthorizationCodeGrantType() && !context.Request.IsRefreshTokenGrantType()) {
                context.Rejected(
                    error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    description: "Only authorization code and refresh token grant types " +
                                 "are accepted by this authorization server");

                return;
            }

            // Note: client authentication is not mandatory for non-confidential client applications like mobile apps
            // (except when using the client credentials grant type) but this authorization server uses a safer policy
            // that makes client authentication mandatory and returns an error if client_id or client_secret is missing.
            // You may consider relaxing it to support the resource owner password credentials grant type
            // with JavaScript or desktop applications, where client credentials cannot be safely stored.
            // In this case, call context.Skip() to inform the server middleware the client is not trusted.
            if (string.IsNullOrEmpty(context.Request.ClientId) || string.IsNullOrEmpty(context.Request.ClientSecret)) {
                context.Rejected(
                    error: OpenIdConnectConstants.Errors.InvalidRequest,
                    description: "Missing credentials: ensure that your credentials were correctly " +
                                 "flowed in the request body or in the authorization header");

                return;
            }

            var database = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            // Retrieve the application details corresponding to the requested client_id.
            var application = await (from entity in database.Applications
                                     where entity.ApplicationID == context.ClientId
                                     select entity).SingleOrDefaultAsync(context.HttpContext.RequestAborted);

            if (application == null) {
                context.Rejected(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Application not found in the database: ensure that your client_id is correct");

                return;
            }

            if (!string.Equals(context.Request.ClientSecret, application.Secret, StringComparison.Ordinal)) {
                context.Rejected(
                    error: OpenIdConnectConstants.Errors.InvalidClient,
                    description: "Invalid credentials: ensure that you specified a correct client_secret");

                return;
            }

            context.Validated();
        }


    }
}