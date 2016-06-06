using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection.KeyManagement;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yavsc.Extensions;
using Yavsc.Models;
using Yavsc.ViewModels.Account;

namespace Yavsc.Controllers
{
    [AllowAnonymous]
    public class OAuthController : Controller
    {
        ApplicationDbContext _context;
        UserManager<ApplicationUser> _userManager;
        
        SiteSettings _siteSettings;

        ILogger _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private TokenAuthOptions _tokenOptions;

        public OAuthController(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, IKeyManager keyManager,
        IOptions<TokenAuthOptions> tokenOptions,
        UserManager<ApplicationUser> userManager,
        IOptions<SiteSettings> siteSettings,
        ILoggerFactory loggerFactory
        )
        {
            _siteSettings = siteSettings.Value;
            _context = context;
            _signInManager = signInManager;
            _tokenOptions = tokenOptions.Value;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<OAuthController>();
        }


        [HttpGet("~/signin")]
        public ActionResult SignIn(string returnUrl = null)
        {
            _logger.LogWarning($"Singin wanted: returnUrl: {returnUrl} ");
            // Note: the "returnUrl" parameter corresponds to the endpoint the user agent
            // will be redirected to after a successful authentication and not
            // the redirect_uri of the requesting client application.
            return View("SignIn", new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalProviders = HttpContext.GetExternalProviders()
            });
            /* Note: When using an external login provider, redirect the query  :
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(OpenIdConnectDefaults.AuthenticationScheme, returnUrl);
            return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, properties);
            */
        }

        [HttpGet("~/authenticate")]
        public ActionResult Authenticate(string returnUrl = null)
        {
            return SignIn(returnUrl);
        }
        
        [HttpGet("~/forbidden")]
        public ActionResult Forbidden(string returnUrl = null)
        {
            return View(returnUrl);
        }
        
        [HttpPost("~/signin")]
        public IActionResult SignIn(string Provider, string ReturnUrl)
        {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrEmpty(Provider))
            {
                _logger.LogWarning("Provider not specified");
                return HttpBadRequest();
            }

            if (!_signInManager.GetExternalAuthenticationSchemes().Any(x => x.AuthenticationScheme == Provider))
            {
                _logger.LogWarning($"Provider not found : {Provider}");
                return HttpBadRequest();
            }

            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs

            // Note: the "returnUrl" parameter corresponds to the endpoint the user agent
            // will be redirected to after a successful authentication and not
            // the redirect_uri of the requesting client application.
            if (string.IsNullOrEmpty(ReturnUrl))
            {
                    _logger.LogWarning("ReturnUrl not specified");
                    return HttpBadRequest();
            }
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = ReturnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(Provider, redirectUrl);
            //  var properties = new AuthenticationProperties{RedirectUri=ReturnUrl};
            return new ChallengeResult(Provider,properties);
        }



        [HttpGet("~/signout"), HttpPost("~/signout")]
        public async Task SignOut()
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).

            await HttpContext.Authentication.SignOutAsync("ServerCookie");
        }




        [HttpGet("~/api/getclaims"), Produces("application/json")]

        public IActionResult GetClaims()
        {
            var identity = User.Identity as ClaimsIdentity;

            var claims = from c in identity.Claims
                         select new
                         {
                             subject = c.Subject.Name,
                             type = c.Type,
                             value = c.Value
                         };

            return Ok(claims);
        }


        [HttpGet("~/connect/authorize"), HttpPost("~/connect/authorize")]
        public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
        {
            // Note: when a fatal error occurs during the request processing, an OpenID Connect response
            // is prematurely forged and added to the ASP.NET context by OpenIdConnectServerHandler.
            // You can safely remove this part and let ASOS automatically handle the unrecoverable errors
            // by switching ApplicationCanDisplayErrors to false in Startup.cs.
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response == null)
            {
                _logger.LogError("GetOpenIdConnectResponse is null");
                return View("OidcError", response);
            }

            // Extract the authorization request from the ASP.NET environment.
            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                _logger.LogError("An internal error has occurred, GetOpenIdConnectRequest is null");
                return View("OidcError", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            // Note: authentication could be theorically enforced at the filter level via AuthorizeAttribute
            // but this authorization endpoint accepts both GET and POST requests while the cookie middleware
            // only uses 302 responses to redirect the user agent to the login page, making it incompatible with POST.
            // To work around this limitation, the OpenID Connect request is automatically saved in the cache and will be
            // restored by the OpenID Connect server middleware after the external authentication process has been completed.

            if (!User.Identities.Any(identity => identity.IsAuthenticated))
            {
                return new ChallengeResult(new AuthenticationProperties {
                    RedirectUri = Url.Action(nameof(Authorize), request.BuildRedirectUrl())});
            }
            // Note: ASOS automatically ensures that an application corresponds to the client_id specified
            // in the authorization request by calling IOpenIdConnectServerProvider.ValidateAuthorizationRequest.
            // In theory, this null check shouldn't be needed, but a race condition could occur if you
            // manually removed the application details from the database after the initial check made by ASOS.
            /* FIXME response.ClientId && request.ClientId are null or empty here */
            _logger.LogInformation($"ensures that an application corresponds to the client_id specified ({request.ClientId})");
            var application = await GetApplicationAsync(request.ClientId, cancellationToken);
            if (application == null)
            {
                _logger.LogError("Details concerning the calling client application cannot be found in the database");
                return View("OidcError", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            // Note: in a real world application, you'd probably prefer creating a specific view model.
            return View("Authorize", new AuthorisationView { Message = request, 
            Application = application});
        }

        [HttpPost("~/connect/authorize/accept"),Authorize]
        public async Task<IActionResult> Accept(CancellationToken cancellationToken)
        {
            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("OidcError", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier,"name",User.GetUserId()));
            if (User.IsInRole(Constants.AdminGroupName))
                identity.AddClaim(new Claim(ClaimTypes.Actor,"role",Constants.AdminGroupName));
           
            // Copy the claims retrieved from the external identity provider
            // (e.g Google, Facebook, a WS-Fed provider or another OIDC server).
            foreach (var claim in User.Claims)
            {
                // Allow ClaimTypes.Name to be added in the id_token.
                // ClaimTypes.NameIdentifier is automatically added, even if its
                // destination is not defined or doesn't include "id_token".
                // The other claims won't be visible for the client application.
                
                if (claim.Type == ClaimTypes.Role
                || claim.Type == ClaimTypes.Email
                || claim.Type == ClaimTypes.NameIdentifier ) {
                    claim.WithDestination( "code" );
                    claim.WithDestination( "id_token" );
                    claim.WithDestination( "token" );
                }
               
                identity.AddClaim(claim);
            }

            var application = await GetApplicationAsync(request.ClientId, cancellationToken);
            if (application == null)
            {
                return View("OidcError", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }
            // Create a new ClaimsIdentity containing the claims associated with the application.
            // Note: setting identity.Actor is not mandatory but can be useful to access
            // the whole delegation chain from the resource server (see ResourceController.cs).
            identity.Actor = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
            identity.Actor.AddClaim(ClaimTypes.NameIdentifier, application.ApplicationID);
            identity.Actor.AddClaim(ClaimTypes.Name, application.DisplayName);

            var properties = new AuthenticationProperties();

            // Note: you can change the list of scopes granted
            // to the client application using SetScopes:
            properties.SetScopes(new[] {
                /* openid: */ OpenIdConnectConstants.Scopes.OpenId,
                /* email: */ OpenIdConnectConstants.Scopes.Email,
                /* profile: */ OpenIdConnectConstants.Scopes.Profile,
                /* api-resource-controller: */ "api-resource-controller"
            });

            // You can also limit the resources endpoints
            // the access token should be issued for:
            properties.SetResources(new[] {
                _siteSettings.Audience
            });
            // This call will instruct AspNet.Security.OpenIdConnect.Server to serialize
            // the specified identity to build appropriate tokens (id_token and token).
            // Note: you should always make sure the identities you return contain either
            // a 'sub' or a 'ClaimTypes.NameIdentifier' claim. In this case, the returned
            // identities always contain the name identifier returned by the external provider.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs.
            await HttpContext.Authentication.SignInAsync(
                "oidc-server",
                new ClaimsPrincipal(identity), properties); 
                 _logger.LogInformation($"request.GrantType: {request.GrantType}");
                 _logger.LogInformation($"request.Code: {request.Code}");
                if (request.GrantType == "authorisation_code") {
                    var reduri=request.BuildRedirectUrl();
                _logger.LogInformation($"{reduri}");
             return Redirect(reduri);
                }
                if (request.GrantType == "token") {
                    
                }
              return new EmptyResult();
           //   return new EmptyResult();
            // Create a new authentication ticket holding the user identity.
            /*var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);*/

            // Set the list of scopes granted to the client application.
            // Note: this sample always grants the "openid", "email" and "profile" scopes
            // when they are requested by the client application: a real world application
            // would probably display a form allowing to select the scopes to grant.
           /* ticket.SetScopes(new[] {
                OpenIdConnectConstants.Scopes.OpenId,
                 OpenIdConnectConstants.Scopes.Email,
                 OpenIdConnectConstants.Scopes.Profile,
                 OpenIdConnectConstants.Scopes.OfflineAccess
            }.Intersect(request.GetScopes())); */

            // Set the resources servers the access token should be issued for.
          //  ticket.SetResources(new string[]{"resource_server"});

            //var response = HttpContext.GetOpenIdConnectResponse();
            

            
        }

        [HttpPost("~/connect/authorize/deny"), ValidateAntiForgeryToken]
        public IActionResult Deny(CancellationToken cancellationToken)
        {
            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("OidcError", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }


            // Notify AspNet.Security.OpenIdConnect.Server that the authorization grant has been denied.
            // Note: OpenIdConnectServerHandler will automatically take care of redirecting
            // the user agent to the client application using the appropriate response_mode.
            HttpContext.SetOpenIdConnectResponse(new OpenIdConnectMessage
            {
                Error = "access_denied",
                ErrorDescription = "The authorization grant has been denied by the resource owner",
                RedirectUri = request.RedirectUri,
                State = request.State
            });

            return new EmptyResult();
        }

        [HttpGet("~/connect/logout")]
        public async Task<ActionResult> Logout()
        {
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                _logger.LogError("GetOpenIdConnectResponse is null");
                return View("OidcError", response);
            }

            // When invoked, the logout endpoint might receive an unauthenticated request if the server cookie has expired.
            // When the client application sends an id_token_hint parameter, the corresponding identity can be retrieved
            // using AuthenticateAsync or using User when the authorization server is declared as AuthenticationMode.Active.
            var identity = await HttpContext.Authentication.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                _logger.LogError("An internal error has occurred");
                return View("OidcError", new OpenIdConnectMessage
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            return View("Logout", Tuple.Create(request, identity));
        }

        [HttpPost("~/connect/logout")]
        [ValidateAntiForgeryToken]
        public async Task Logout(CancellationToken cancellationToken)
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await HttpContext.Authentication.SignOutAsync("ServerCookie");

            // This call will instruct AspNet.Security.OpenIdConnect.Server to serialize
            // the specified identity to build appropriate tokens (id_token and token).
            // Note: you should always make sure the identities you return contain either
            // a 'sub' or a 'ClaimTypes.NameIdentifier' claim. In this case, the returned
            // identities always contain the name identifier returned by the external provider.
            await HttpContext.Authentication.SignOutAsync(OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        protected virtual Task<Application> GetApplicationAsync(string identifier, CancellationToken cancellationToken)
        {
            // Retrieve the application details corresponding to the requested client_id.
            return (from application in _context.Applications
                    where application.ApplicationID == identifier
                    select application).SingleOrDefaultAsync(cancellationToken);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}