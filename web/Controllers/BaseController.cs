using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yavsc.Model.Identity;
using System.Web.Routing;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using System.Web.Security;

namespace Yavsc.Controllers
{
    public class BaseController : Controller
    {
		public BaseController()
		{}

		private ApplicationUserManager _userManager;

		protected ApplicationUserManager UserManager
		{
			get
			{
				return _userManager;
			}
		}

		private  AppUserState userState = null;

		protected AppUserState UserState { get { return userState; }  }

		public BaseController(ApplicationUserManager userManager,
			ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
		{
			_userManager = userManager;
			AccessTokenFormat = accessTokenFormat;
		}

		public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; set; }

		public IAuthenticationManager AuthenticationManager
		{
			get { return HttpContext.GetOwinContext().Authentication; }
		}
		public void IdentitySignin(AppUserState userState, bool isPersistent = false)
		{
			// create required claims
			var claims = new List<Claim>();
			claims.Add(new Claim(ClaimTypes.NameIdentifier, userState.UserId));
			claims.Add(new Claim(ClaimTypes.Name, userState.Name));
			claims.Add (new Claim (ClaimsIdentity.DefaultNameClaimType,
			                       userState.Name,"typeName"));
			claims.Add(new Claim("userState", userState.ToString()));
			var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
			AuthenticationManager.SignIn(new AuthenticationProperties()
			{
				AllowRefresh = true,
				IsPersistent = isPersistent,
				ExpiresUtc = DateTime.UtcNow.AddDays(7)
			}, identity);
		}

		public void IdentitySignin(string username, string providerKey = null, bool isPersistent = false)
		{
			var appUserState = new AppUserState () {
				UserId = providerKey,
				Name = username
			};
			IdentitySignin(appUserState, isPersistent);
		}

		/* protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);
			// Grab the user's login information from Identity
			AppUserState appUserState = new AppUserState();
			if (User is ClaimsPrincipal)
			{
				var user = User as ClaimsPrincipal;
				var claims = user.Claims.ToList();

				var userStateString = GetClaim(claims, "userState");
				var name = GetClaim(claims, ClaimTypes.Name);
				var id = GetClaim(claims, ClaimTypes.NameIdentifier);

				if (!string.IsNullOrEmpty(userStateString))
					appUserState.FromString(userStateString);
				
			} else if (User is RolePrincipal) {
				var user = User as RolePrincipal;
				appUserState.IsAdmin = user.IsInRole ("Admin");
				appUserState.Name = user.Identity.Name;
			}
			
			userState = appUserState;
			ViewData["UserState"] = UserState;
		} */

		public static string GetClaim(List<Claim> claims, string key)
		{
			var claim = claims.FirstOrDefault(c => c.Type == key);
			if (claim == null)
				return null;

			return claim.Value;
		}

    }
}
