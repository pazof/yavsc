using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.DataProtection.KeyManagement;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
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
            // Note: the "returnUrl" parameter corresponds to the endpoint the user agent
            // will be redirected to after a successful authentication and not
            // the redirect_uri of the requesting client application against the third
            // party identity provider.
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
            return View("Forbidden",returnUrl);
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

        protected virtual Task<Application> GetApplicationAsync(string identifier, CancellationToken cancellationToken)
        {
            // Retrieve the application details corresponding to the requested client_id.
            return (from application in _context.Applications
                    where application.ApplicationID == identifier
                    select application).SingleOrDefaultAsync(cancellationToken);
        }

   
    }
}