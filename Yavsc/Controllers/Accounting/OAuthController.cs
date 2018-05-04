﻿using System.Collections.Generic;
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
                    scopes = ((string)queryStringComponents["scope"]).Split(' ');
                if (queryStringComponents.ContainsKey("redirect_uri"))
                    redirect_uri = queryStringComponents["redirect_uri"];
            }
            var username = User.GetUserName();

            var model = new AuthorisationView { 
                Scopes = (Constants.SiteScopes.Where(s=> scopes.Contains(s.Id))).ToArray(),
                Message = $"Bienvenue {username}."
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

            if (Request.Headers.Keys.Contains("Accept")) {
                var accepted = Request.Headers["Accept"];
                if (accepted == "application/json")
                {
                    return Ok(model);
                }
            }
            return View(model);
        }

        [HttpGet("~/oauth/success")]
        public IActionResult NativeAuthSuccess ()
        {
            return RedirectToAction("Index","Home");
        }

    }
}