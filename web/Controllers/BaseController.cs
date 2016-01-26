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
using Yavsc.Model.RolesAndMembers;
using Microsoft.AspNet.Identity.Owin;

namespace Yavsc.Controllers
{
    public class BaseController : Controller
    {
		public BaseController()
		{}

		private ApplicationUserManager _userManager;
		private  AppUserState userState = null;

		protected AppUserState UserState { get { return userState; }  }

		public BaseController(ApplicationUserManager userManager,
			ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
		{
			UserManager = userManager;
			AccessTokenFormat = accessTokenFormat;
		}

		public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; set; }

		public IAuthenticationManager AuthenticationManager
		{
			get { return HttpContext.GetOwinContext().Authentication; }
		}

		public void IdentitySignin(string username, string providerKey = null, bool isPersistent = false)
		{
			var claims = new List<Claim>();
			var appUserState = new AppUserState () {
				UserId = providerKey,
				Name = username
			};
			// create required claims
			claims.Add(new Claim(ClaimTypes.NameIdentifier, providerKey));
			claims.Add(new Claim(ClaimTypes.Name, username));
			claims.Add (new Claim (ClaimsIdentity.DefaultNameClaimType,
				appUserState.Name, "Application"));
			// custom – my serialized AppUserState object
			claims.Add(new Claim("userState", appUserState.ToString()));

			var identity = new ClaimsIdentity(claims, "Application");

			AuthenticationManager.SignIn(new AuthenticationProperties()
				{
					AllowRefresh = true,
					IsPersistent = isPersistent,
					ExpiresUtc = DateTime.UtcNow.AddDays(7)
				}, identity);
		}

		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager> ();
			}
			private set
			{
				_userManager = value;
			}
		}

		protected override void Initialize(RequestContext requestContext)
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
			}
			userState = appUserState;
			ViewData["UserState"] = UserState;
		}

		public static string GetClaim(List<Claim> claims, string key)
		{
			var claim = claims.FirstOrDefault(c => c.Type == key);
			if (claim == null)
				return null;

			return claim.Value;
		}

    }
}
