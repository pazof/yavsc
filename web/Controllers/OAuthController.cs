using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Yavsc.ApiControllers;
using Yavsc.Models.Identity;
using Microsoft.AspNet.Identity;
using Yavsc.Model.RolesAndMembers;
using System.Net;
using Yavsc.Model.Identity;
using Yavsc.Helpers;
using Microsoft.AspNet.Identity.Owin;

namespace Yavsc.Controllers
{
	/// <summary>
	/// O auth controller.
	/// </summary>
	public class OAuthController : BaseController
	{
		/// <summary>
		/// External this instance.
		/// </summary>
		public ActionResult External (string returnUrl)
		{
			var authentication = HttpContext.GetOwinContext ().Authentication;
			if (Request.HttpMethod == "POST") {
				foreach (var key in Request.Form.AllKeys) {
					if (key.StartsWith ("submit.External.") && !string.IsNullOrEmpty (Request.Form.Get (key))) {
						var authType = key.Substring ("submit.External.".Length);
						Session ["authType"] = authType;
						Session ["returnUrl"] = returnUrl;
						authentication.Challenge (authType);
						return new HttpUnauthorizedResult ();
					}
				}
			}
			return View ();
		}
		/// <summary>
		/// Externals the callback.
		/// </summary>
		/// <returns>The callback.</returns>
		public ActionResult ExternalCallback ()
		{
			string authType = Session ["authType"] as string;
			string returnUrl = Session ["returnUrl"] as string;
			var authentication = HttpContext.GetOwinContext ().Authentication;
			var authResult = authentication.AuthenticateAsync (authType).Result;

			if (authResult != null) {
				var identity = authResult.Identity;
				if (identity != null) {

					var idClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
					if (idClaim != null)
					{
						var loginInfo = new ExternalLoginInfo()
						{
							DefaultUserName = identity.Name ,
							Login = new UserLoginInfo(idClaim.Issuer, idClaim.Value)
						};
					}

					// var email = identity.FindFirstValue(ClaimTypes.Email);
					var emailClaim = identity.Claims.FirstOrDefault(c => c.Type.Equals("email"));
					var email = emailClaim.Value;
					authentication.SignOut (authType);
					// here, or the user already is logged and that's a profile merging intent, 
					// or he exists and is off-line, and that's just an external login,
					// or we create it and ask him to confirm 
					// his registration and complete his profile.

					var existingAppAuth = authentication.AuthenticateAsync("Application").Result;
					if (existingAppAuth != null) {
						throw new NotSupportedException ("Merge another profile is not yet possible");
					}

					UserManager.FindByEmailAsync (email);

					authentication.SignIn (
						new AuthenticationProperties { IsPersistent = true },
							new ClaimsIdentity (identity.Claims, "Application", 
								identity.NameClaimType, identity.RoleClaimType));
					return Redirect (returnUrl);
				}
			}
			return new HttpUnauthorizedResult ();
		}

		/// <summary>
		/// Authorize this instance.
		/// </summary>
		public ActionResult Authorize ()
		{
			if (Response.StatusCode != 200) {
				return View ("AuthorizeError");
			}

			var authentication = HttpContext.GetOwinContext ().Authentication;
			var ticket = authentication.AuthenticateAsync ("Application").Result;
			var identity = ticket != null ? ticket.Identity : null;
			if (identity == null) {
				authentication.Challenge ("Application");
				return new HttpUnauthorizedResult ();
			}

			var scopes = (Request.QueryString.Get ("scope") ?? "").Split (' ');

			if (Request.HttpMethod == "POST") {
				if (!string.IsNullOrEmpty (Request.Form.Get ("submit.Grant"))) {
					identity = new ClaimsIdentity (identity.Claims, "Bearer", identity.NameClaimType, identity.RoleClaimType);
					foreach (var scope in scopes) {
						identity.AddClaim (new Claim ("urn:oauth:scope", scope));
					}
					authentication.SignIn (identity);
				}
				if (!string.IsNullOrEmpty (Request.Form.Get ("submit.Login"))) {
					authentication.SignOut ("Application");
					authentication.Challenge ("Application");
					return new HttpUnauthorizedResult ();
				}
			}

			return View ();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ExternalLoginConfirmation (ExternalLoginConfirmationViewModel model, string returnUrl)
		{
			if (User.Identity.IsAuthenticated) {
				return RedirectToAction ("Manage");
			}

			if (ModelState.IsValid) {
				// Get the information about the user from the external login provider
				var info = await AuthenticationManager.GetExternalLoginInfoAsync ();
				if (info == null) {
					return View ("ExternalLoginFailure");
				}
				var user = new ApplicationUser () {
					UserName = model.Email, Email = model.Email

				};

				IdentityResult result = await UserManager.CreateAsync (user);
				if (result.Succeeded) {
					result = await UserManager.AddLoginAsync (user.Id, info.Login);
					if (result.Succeeded) {
						await UserManager.AddToRoleAsync (user.Id, "Blogger");
						var userState = new AppUserState ();
						userState.FromUser (user);	
						IdentitySignin (userState, null, false);
						return Redirect (returnUrl);
					}
					// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
					// Send an email with this link
					// string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
					// var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
					// SendEmail(user.Email, callbackUrl, "Confirm your account", "Please confirm your account by clicking this link");
					return Redirect (returnUrl);
				}
				if (!result.Succeeded)
					foreach (var err in result.Errors)
						ModelState.AddModelError ("Email", err);
			}

			ViewBag.ReturnUrl = returnUrl;
			return View (model);
		}
	}
}
