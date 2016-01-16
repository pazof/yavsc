using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yavsc.Model.Identity;
using System.Web.Routing;
using System.Security.Claims;

namespace Yavsc.Controllers
{
    public class BaseController : Controller
    {
		private  AppUserState userState = null;
		protected AppUserState UserState { get { return userState; }  }

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
				//var name = GetClaim(claims, ClaimTypes.Name);
				//var id = GetClaim(claims, ClaimTypes.NameIdentifier);

				if (!string.IsNullOrEmpty(userStateString))
					appUserState.FromString(userStateString);
			}
			userState = appUserState;
			ViewData["UserState"] = UserState;
		}

        public virtual ActionResult Index()
        {
            return View ();
        }
		public static string GetClaim(List<Claim> claims, string key)
		{
			var claim = claims.FirstOrDefault(c => c.Type == key);
			if (claim == null)
				return null;

			return claim.Value;
		}

		/// <summary>
		/// Allow external initialization of this controller by explicitly
		/// passing in a request context
		/// </summary>
		/// <param name="requestContext"></param>
		public void InitializeForced(RequestContext requestContext)
		{
			Initialize(requestContext);
		}


		/// <summary>
		/// Displays a self contained error page without redirecting.
		/// Depends on ErrorController.ShowError() to exist
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="redirectTo"></param>
		/// <returns></returns>
		protected internal ActionResult DisplayErrorPage(string title, string message, string redirectTo = null)
		{
			ErrorController controller = new ErrorController();
			controller.InitializeForced(ControllerContext.RequestContext);
			return controller.ShowError(title, message, redirectTo);
		}

    }
}
