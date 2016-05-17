using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Yavsc.ApiControllers;
using Microsoft.AspNet.Identity;
using Yavsc.Model.RolesAndMembers;
using System.Net;
using Yavsc.Model.Identity;
using Yavsc.Helpers;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Security;
using Yavsc.Model.Google;
using Yavsc.Helpers.OAuth.Api;
using Yavsc.Helpers.Google.Api;

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
						authentication.Challenge (
								new AuthenticationProperties() {
									RedirectUri="/OAuth/ExternalCallback"
								},
								authType);
						return new HttpUnauthorizedResult ();
					}
				}
			}
			// should not occur
			return Redirect(Session ["returnUrl"] as string);
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


		/// <summary>
		/// Externals the callback.
		/// </summary>
		/// <returns>The callback.</returns>
		public ActionResult ExternalCallbackRedirect()
		{
			return Redirect("/OAuth/ExternalCallback");
		}

		public ActionResult ExternalCallback (string id)
		{
			var info =  AuthenticationManager.GetExternalLoginInfoAsync ();
			if (info == null) {
				return View ("ExternalLoginFailure");
			}
			string returnUrl = Session ["returnUrl"] as string;
			string authType = Session ["authType"] as string;
			var authentication = HttpContext.GetOwinContext ().Authentication;
			var authResult = authentication.AuthenticateAsync (authType).Result;

			if (authResult != null) {
				var identity = authResult.Identity;
				if (identity != null) {
					ExternalLoginInfo loginInfo = null;
					var claim = identity.FindFirst(ClaimTypes.NameIdentifier);
					if (claim != null)
					{
						loginInfo = new ExternalLoginInfo()
						{
							DefaultUserName = identity.Name ,
							Login = new UserLoginInfo(claim.Issuer, claim.Value)
						};
					}
					claim = identity.FindFirst(ClaimTypes.Email);
					string email = null;
					if (claim != null)
					{
						email = claim.Value;
						var users = Membership.FindUsersByEmail(email);
						foreach (MembershipUser user in users) {
							authentication.SignOut (authType);
							IdentitySignin (user.UserName, user.ProviderUserKey.ToString(), false);
							return Redirect (returnUrl);
						}
					}

					SignIn model = new SignIn ();
					model.Email = email;
					model.UserName = loginInfo.DefaultUserName;
					authentication.SignOut (authType);
					Session["ExternalLoginInfo"] = loginInfo;
					return ExternalLoginConfirmation(model,returnUrl).Result;
				}
			}
			return new HttpUnauthorizedResult ();
		}
		#region OLD

		/// <summary>
		/// Serves the external callback.
		/// </summary>
		/// <returns>The callback.</returns>
		[HttpGet]
		public ActionResult OldExternalCallback ()
		{
			string msg;
			string authType = Session ["authType"] as string;
			PeopleApi oa = GoogleHelpers.CreateOAuth2<PeopleApi>(authType);
			AuthToken token = oa.GetToken<AuthToken> (Request, (string)Session ["state"], out msg);
			if (token == null) {
				ViewData.Notify(msg);
				return View ();
			}
			string returnUrl = (string)Session ["returnUrl"];
			SignIn regmod = new SignIn ();

			People me = oa.GetMe (token);
			// TODO use me.id to retreive an existing user
			string accEmail = me.emails.Where (x => x.type == "account").First ().value;
			MembershipUserCollection mbrs = Membership.FindUsersByEmail (accEmail);
			if (mbrs.Count == 1) {
				// TODO check the google id
				// just set this user as logged on
				foreach (MembershipUser u in mbrs) {
					string username = u.UserName;
					// FormsAuthentication.SetAuthCookie (username, true);

					/* var upr = ProfileBase.Create (username);
					SaveToken (upr,gat); */
				}
				Session ["returnUrl"] = null;
				return Redirect (returnUrl);
			}
			// else create the account
			regmod.Email = accEmail;
			regmod.UserName = me.displayName;
			Session ["me"] = me;
			Session ["AuthToken"] = token;
			return OldSiginConfirmation (regmod);
		}

		/// <summary>
		/// Creates an account using the Google authentification.
		/// </summary>
		/// <param name="regmod">Regmod.</param>
		[HttpPost]
		public ActionResult OldSiginConfirmation (SignIn regmod)
		{
			if (ModelState.IsValid) {
				if (Membership.GetUser (regmod.UserName) != null) {
					ModelState.AddModelError ("UserName", "This user name already is in use");
					return View ();
				}
				string returnUrl = (string) Session ["returnUrl"];
				AuthToken token = (AuthToken) Session ["GoogleAuthToken"];
				People me = (People) Session ["me"];
				if (token == null || me == null)
					throw new InvalidProgramException ();

				Random rand = new Random ();
				string passwd = rand.Next (100000).ToString () + rand.Next (100000).ToString ();

				MembershipCreateStatus mcs;
				Membership.CreateUser (
					regmod.UserName,
					passwd,
					regmod.Email,
					null,
					null,
					true,
					out mcs);
				switch (mcs) {
				case MembershipCreateStatus.DuplicateEmail:
					ModelState.AddModelError ("Email", "Cette adresse e-mail correspond " +
						"à un compte utilisateur existant");
					return View (regmod);
				case MembershipCreateStatus.DuplicateUserName:
					ModelState.AddModelError ("UserName", "Ce nom d'utilisateur est " +
						"déjà enregistré");
					return View (regmod);
				case MembershipCreateStatus.Success:
					Membership.ValidateUser (regmod.UserName, passwd);
					FormsAuthentication.SetAuthCookie (regmod.UserName, true);

					HttpContext.Profile.Initialize (regmod.UserName, true);
					HttpContext.Profile.SetPropertyValue ("Name", me.displayName);
					// TODO use image
					if (me.image != null) {
						HttpContext.Profile.SetPropertyValue ("Avatar", me.image.url);
					}
					if (me.placesLived != null) {
						People.Place pplace = me.placesLived.Where (x => x.primary).First ();
						if (pplace != null)
							HttpContext.Profile.SetPropertyValue ("CityAndState", pplace.value);
					}
					if (me.url != null)
						HttpContext.Profile.SetPropertyValue ("WebSite", me.url);
					// Will be done in SaveToken: HttpContext.Profile.Save ();

					// SaveToken (HttpContext.Profile, token);
					Session ["returnUrl"] = null;
					return Redirect (returnUrl);
				}
				ViewData ["returnUrl"] = returnUrl;
			}
			return View (regmod);
		}

		#endregion

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ExternalLoginConfirmation (SignIn regmod, string returnUrl)
		{
			if (User.Identity.IsAuthenticated) {
				return RedirectToAction ("Profile",new {controller="Account"});
			}

			if (ModelState.IsValid) {

				// Get the information about the user from the external login provider
				ExternalLoginInfo info = Session["ExternalLoginInfo" ] as ExternalLoginInfo;
				if (info == null) {
					return View ("ExternalLoginFailure");
				}

				var user = new ApplicationUser () {
					UserName = regmod.DisplayName, 
					Email = regmod.Email
				};

				Random rand = new Random ();
				string passwd = rand.Next (100000).ToString () + rand.Next (100000).ToString ();

				MembershipCreateStatus mcs;
				Membership.CreateUser (
					regmod.UserName,
					passwd,
					regmod.Email,
					null,
					null,
					true,
					out mcs);
				switch (mcs) {
					case MembershipCreateStatus.DuplicateEmail:
						ModelState.AddModelError ("Email", "Cette adresse e-mail correspond " +
						                      "à un compte utilisateur existant");
					return View (regmod);
					case MembershipCreateStatus.DuplicateUserName:
						ModelState.AddModelError ("UserName", "Ce nom d'utilisateur est " +
						                      "déjà enregistré");
					return View (regmod);
					case MembershipCreateStatus.Success:
						Membership.ValidateUser (regmod.UserName, passwd);
						FormsAuthentication.SetAuthCookie (regmod.UserName, true);

						HttpContext.Profile.Initialize (regmod.UserName, true);
						HttpContext.Profile.SetPropertyValue ("Name", info.DefaultUserName);
						// TODO use image
						if (regmod.Avatar != null) {
							HttpContext.Profile.SetPropertyValue ("Avatar", regmod.Avatar);
						}
						if (regmod.Location != null) {
							HttpContext.Profile.SetPropertyValue ("CityAndState", regmod.Location);
						}
						HttpContext.Profile.Save ();
						Session ["returnUrl"] = null;
					return Redirect (returnUrl);
				}
			}

			string authType = Session ["authType"] as string;
			ViewBag.LoginProvider = authType;
			ViewBag.ReturnUrl = returnUrl;
			return View ("ExternalLoginConfirmation",regmod);
		}


	}
}
