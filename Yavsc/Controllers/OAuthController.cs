using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.DataProtection.KeyManagement;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.Primitives;
using OAuth.AspNet.AuthServer;
using Yavsc.Models;
using Yavsc.Models.Auth;

namespace Yavsc.Controllers
{
    public class TokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string grant_type { get; set; }

        public int entity_id { get; set; }
    }

    [AllowAnonymous]
    public class OAuthController : Controller
    {
        ApplicationDbContext _context;
        UserManager<ApplicationUser> _userManager;

        SiteSettings _siteSettings;

        ILogger _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public OAuthController(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, IKeyManager keyManager,
        UserManager<ApplicationUser> userManager,
        IOptions<SiteSettings> siteSettings,
        ILoggerFactory loggerFactory
        )
        {
            _siteSettings = siteSettings.Value;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<OAuthController>();
        }

        /*
        private async Task<string> GetToken(string purpose, string userid, DateTime? expires)
        {
            // Here, you should create or look up an identity for the user which is being authenticated.
            // For now, just creating a simple generic identity.
            var identuser = await _userManager.FindByIdAsync(userid);

            return await _tokenProvider.GenerateAsync(purpose, _userManager, identuser);
        }

        /// <summary>
        /// Check if currently authenticated. Will throw an exception of some sort which shoudl be caught by a general
        /// exception handler and returned to the user as a 401, if not authenticated. Will return a fresh token if
        /// the user is authenticated, which will reset the expiry.
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost, Authorize]
        [Route("~/oauth/token")]
        public async Task<dynamic> Get()
        {
            bool authenticated = false;
            string user = null;
            int entityId = -1;
            string token = null;
            DateTime? tokenExpires = default(DateTime?);
            var currentUser = User;
            if (currentUser != null)
            {
                authenticated = currentUser.Identity.IsAuthenticated;
                if (authenticated)
                {
                    user = User.GetUserId();
                    _logger.LogInformation($"authenticated user:{user}");

                    foreach (Claim c in currentUser.Claims) if (c.Type == "EntityID") entityId = Convert.ToInt32(c.Value);

                    tokenExpires = DateTime.UtcNow.AddMinutes(2);
                    token = await GetToken("id_token", user, tokenExpires);
                    return new TokenResponse { access_token = token, expires_in = 3400, entity_id = entityId };
                }
            }
            return new { authenticated = false };
        } */


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

        [HttpGet(Constants.AuthorizePath),HttpPost(Constants.AuthorizePath)]
        public async Task<ActionResult> Authorize()
        {
            if (Response.StatusCode != 200)
            {
                return View("AuthorizeError");
            }

            AuthenticationManager authentication = Request.HttpContext.Authentication;
            var appAuthSheme = Startup.IdentityAppOptions.Cookies.ApplicationCookieAuthenticationScheme;

            ClaimsPrincipal principal = await authentication.AuthenticateAsync(appAuthSheme);

            if (principal == null)
            {
                await authentication.ChallengeAsync(appAuthSheme);

                if (Response.StatusCode == 200)
                    return new HttpUnauthorizedResult();

                return new HttpStatusCodeResult(Response.StatusCode);
            }

            string[] scopes = { };
            string redirect_uri=null;

            IDictionary<string,StringValues> queryStringComponents = null;

            if (Request.QueryString.HasValue)
            {
                 queryStringComponents = QueryHelpers.ParseQuery(Request.QueryString.Value);

                if (queryStringComponents.ContainsKey("scope"))
                    scopes = queryStringComponents["scope"];
                if (queryStringComponents.ContainsKey("redirect_uri"))
                    redirect_uri = queryStringComponents["redirect_uri"];
            }

            var model = new AuthorisationView { 
                Scopes = Constants.SiteScopes.Where(s=> scopes.Contains(s.Id)).ToArray(),
                Message = "Welcome."
                } ;

            if (Request.Method == "POST")
            {
                if (!string.IsNullOrEmpty(Request.Form["submit.Grant"]))
                {
                    principal = new ClaimsPrincipal(principal.Identities);

                    ClaimsIdentity primaryIdentity = (ClaimsIdentity)principal.Identity;

                    foreach (var scope in scopes)
                    {
                        primaryIdentity.AddClaim(new Claim("urn:oauth:scope", scope));
                    }
                    _logger.LogWarning("Logging user {principal} against {OAuthDefaults.AuthenticationType}");
                    await authentication.SignInAsync(OAuthDefaults.AuthenticationType, principal);
                }
                if (!string.IsNullOrEmpty(Request.Form["submit.Deny"]))
                {
                    await authentication.SignOutAsync(appAuthSheme);
                    if (redirect_uri!=null)
                        return Redirect(redirect_uri+"?error=scope-denied");
                    return Redirect("/");
                }
                if (!string.IsNullOrEmpty(Request.Form["submit.Login"]))
                {
                    await authentication.SignOutAsync(appAuthSheme);
                    await authentication.ChallengeAsync(appAuthSheme);
                    return new HttpUnauthorizedResult();
                }
            }

            return View(model);
        }

    }
}