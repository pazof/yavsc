using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

using Yavsc.Model.Identity;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Web;
using System.Web.Mvc;
using Yavsc.Model.RolesAndMembers;
using System.Web.Security;
using Yavsc.Helpers;
using Yavsc.Client;
using Yavsc.Model.WorkFlow;
using System.Net;
using System.IO;
using System.Web.Profile;
using System.Linq;
using Yavsc.Model.Circles;
using Yavsc.Client.Messaging;

namespace Yavsc.Controllers
{



	/// <summary>
	/// Account controller.
	/// </summary>
	public class AccountController : BaseController
	{
		

		public void IdentitySignout()
		{
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie,
				DefaultAuthenticationTypes.ExternalCookie);
		}

		#if FALSE
		/// <summary>
		/// Register the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		[AcceptVerbs(HttpVerbs.Post), ValidateAntiForgeryToken]
		public ActionResult Register(RegisterViewModel model)
		{
			if (!YaUserStore.IsAvailable (model.UserName))
				ModelState.AddModelError ("UserName", 
					string.Format(LocalizedText.DuplicateUserName,
						model.UserName));
			
			if (!string.IsNullOrWhiteSpace(Membership.GetUserNameByEmail (model.Email)))
				ModelState.AddModelError ("EMail",
					string.Format(LocalizedText.DuplicateEmail,
						model.Email));

			if (model.Password != model.ConfirmPassword)
				ModelState.AddModelError ("ConfirmPassword", 
					LocalizedText.PleaseConfirmYourNewPassword);

			if (ModelState.IsValid) {
				MembershipCreateStatus mcs;
				var user = Membership.CreateUser (
					model.UserName,
					model.Password,
					model.Email,
					null,
					null,
					false,
					out mcs);
				switch (mcs) {
				case MembershipCreateStatus.Success:
					Url.SendActivationMessage (user);
					ViewData ["username"] = user.UserName;
					ViewData ["email"] = user.Email;
					AppUserState appUserState = new AppUserState ();
					appUserState.FromUser (user);
					IdentitySignin (appUserState, appUserState.UserId);

					return View ("RegistrationPending",model);
				default:
					ViewData.Notify(
						string.Format(LocalizedText.RegistrationUnexpectedError, 
							mcs.ToString ()));
					break;
				}
			}
			return View (model);
		}  
		#endif

		/// <summary>
		/// Avatar the specified user.
		/// </summary>
		/// <param name="id">User.</param>
		// TODO let drop this code
		[AcceptVerbs (HttpVerbs.Get)]
		public ActionResult Avatar (string id)
		{
			string avatarLocation = Url.AvatarUrl (id);
			WebRequest wr = WebRequest.Create (avatarLocation);
			FileContentResult res;
			using (WebResponse resp = wr.GetResponse ()) {
				using (Stream str = resp.GetResponseStream ()) {
					byte[] content = new byte[str.Length];
					str.Read (content, 0, (int)str.Length);
					res = File (content, resp.ContentType);
					wr.Abort ();
					return res;
				}
			}
		}


		/// <summary>
		/// Index this instance.
		/// </summary>
		public ActionResult Index ()
		{
			return View ();
		}

		/// <summary>
		/// Does login.
		/// </summary>
		/// <returns>The login.</returns>
		/// <param name="model">Model.</param>
		/// <param name="returnUrl">Return URL.</param>
		[HttpPost,ValidateAntiForgeryToken] 
		public ActionResult Login (LoginModel model, string returnUrl)
		{
			if (ModelState.IsValid) {
				
				if (Membership.ValidateUser (model.UserName, model.Password)) {
					var user = Membership.GetUser (model.UserName);
					var key = user.ProviderUserKey.ToString ();
					var userState = new AppUserState () {
						Name= user.UserName,
						Email = user.Email,
						IsAdmin = Roles.IsUserInRole("Admin"),
						Theme = ProfileBase.Create(user.UserName).GetPropertyValue("UITheme") as string,
						UserId = key
					};
					IdentitySignin(userState, model.RememberMe);
					if (returnUrl != null)
						return Redirect (returnUrl);
					else
						return View ("Index");
				} else {
					ModelState.AddModelError ("UserName", "The user name or password provided is incorrect.");
				}
			}
			ViewData ["returnUrl"] = returnUrl;
			return View (model);
		}


		/// <summary>
		/// Login the specified returnUrl.
		/// </summary>
		/// <param name="returnUrl">Return URL.</param>
		[HttpGet]
		public ActionResult Login (string returnUrl)
		{
			ViewData ["returnUrl"] = returnUrl;
			ViewBag.AuthTypes = HttpContext.GetOwinContext ().Authentication.GetAuthenticationTypes ()
				.Where (d =>  !string.IsNullOrEmpty (d.Caption)).ToArray ();
			return View ();
		}

		/// <summary>
		/// Gets the registration form.
		/// </summary>
		/// <returns>The register.</returns>
		/// <param name="model">Model.</param>
		/// <param name="returnUrl">Return URL.</param>
		public ActionResult GetRegister(RegisterViewModel model, string returnUrl)
		{
			model.ReturnUrl = returnUrl;
			return View ("Register",model);
		}

		#if FORMS_AUTHENTICATION
		/// <summary>
		/// Register the specified model and returnUrl.
		/// </summary>
		/// <param name="model">Model.</param>
		/// <param name="returnUrl">Return URL.</param>
		[HttpPost]
		public ActionResult Register (RegisterViewModel model, string returnUrl)
		{
			ViewData ["returnUrl"] = returnUrl;
				
			if (ModelState.IsValid) {
				if (model.ConfirmPassword != model.Password) {
					ModelState.AddModelError ("ConfirmPassword", "Veuillez confirmer votre mot de passe");
					return View (model);
				}

				MembershipCreateStatus mcs;
				var user = Membership.CreateUser (
					           model.UserName,
					           model.Password,
					           model.Email,
					           null,
					           null,
					           false,
					           out mcs);
				switch (mcs) {
				case MembershipCreateStatus.DuplicateEmail:
					ModelState.AddModelError ("Email", "Cette adresse e-mail correspond " +
					"à un compte utilisateur existant");
					return View (model);
				case MembershipCreateStatus.DuplicateUserName:
					ModelState.AddModelError ("UserName", "Ce nom d'utilisateur est " +
					"déjà enregistré");
					return View (model);
				case MembershipCreateStatus.Success:
					Url.SendActivationMessage (user);
					ViewData ["username"] = user.UserName;
					ViewData ["email"] = user.Email;
					return View ("RegistrationPending");
				default:
					ViewData ["Error"] = "Une erreur inattendue s'est produite" +
					"a l'enregistrement de votre compte utilisateur" +
					string.Format ("({0}).", mcs.ToString ()) +
					"Veuillez pardonner la gêne" +
					"occasionnée";
					return View (model);
				}

			}
			return View (model);
		}

		#endif

		/// <summary>
		/// Changes the password success.
		/// </summary>
		/// <returns>The password success.</returns>
		public ActionResult ChangePasswordSuccess ()
		{
			return View ();
		}


		/// <summary>
		/// Changes the password.
		/// </summary>
		/// <returns>The password.</returns>
		[HttpGet]
		[Authorize]
		public ActionResult ChangePassword ()
		{
			return View ();
		}

		/// <summary>
		/// Unregister the specified id and confirmed.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="confirmed">If set to <c>true</c> confirmed.</param>
		[Authorize]
		public ActionResult Unregister (string id, bool confirmed = false)
		{
			ViewData ["UserName"] = id;
			if (!confirmed)
				return View ();
			string logged = this.User.Identity.Name;
			if (logged != id)
			if (!Roles.IsUserInRole ("Admin"))
				throw new Exception ("Unregister another user");

			Membership.DeleteUser (
				Membership.GetUser ().UserName);
			return RedirectToAction ("Index", "Home");
		}

		
		/// <summary>
		/// Changes the password.
		/// </summary>
		/// <returns>The password.</returns>
		/// <param name="model">Model.</param>
		[Authorize]
		[HttpPost]
		public ActionResult ChangePassword (ChangePasswordModel model)
		{
			if (ModelState.IsValid) {

				// ChangePassword will throw an exception rather
				// than return false in certain failure scenarios.
				bool changePasswordSucceeded = false;
				try {
					MembershipUserCollection users =
						Membership.FindUsersByName (model.Username);
					if (users.Count > 0) {
						MembershipUser user = Membership.GetUser (model.Username, true);	

						changePasswordSucceeded = user.ChangePassword (model.OldPassword, model.NewPassword);
					} else {
						changePasswordSucceeded = false;
						ModelState.AddModelError ("Username", "The user name not found.");
					}
				} catch (Exception ex) {
					ViewData ["Error"] = ex.ToString ();
				}

				if (changePasswordSucceeded) {
					return RedirectToAction ("ChangePasswordSuccess");
				} else {
					ModelState.AddModelError ("Password", "The current password is incorrect or the new password is invalid.");
				}
			}

			// If we got this far, something failed, redisplay form
			return View (model);
		}


		/// <summary>
		/// Profile the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[Authorize]
		[HttpGet]
		public ActionResult Profile (string id)
		{
			if (id == null)
				id = Membership.GetUser ().UserName;
			ViewData ["UserName"] = id;

			ProfileEdition model = new ProfileEdition ();
			model.UserName = id;
			model.Populate ();
			var profile = ProfileBase.Create (id);

			model.RememberMe = FormsAuthentication.GetAuthCookie (id, true) == null;
			SetMEACodeViewData (model);
			SetUIThemeViewData (model);
			return View (model);
		}
		// TODO replace this with an Html helper method usage
		private void SetUIThemeViewData(Profile model) {
			ViewData ["UITheme"] = YavscHelpers.AvailableThemes.Select (
				x => new SelectListItem () { Selected = 
					model.UITheme == x, Text = x, Value = x }).ToArray();
			
		}
		// TODO replace this with an Html helper method usage
		private void SetMEACodeViewData(Profile model) {
			var activities = WorkFlowManager.FindActivity ("%", false);
			var items = new List<SelectListItem> ();
			items.Add (new SelectListItem () { Selected = model.MEACode == null, Text = LocalizedText.DoNotPublishMyActivity, Value="none" });
			foreach (var a in activities) {
				items.Add(new SelectListItem() { Selected = model.MEACode == a.Id, 
					Text = string.Format("{1} : {0}",a.Title,a.Id), 
					Value = a.Id });
			}
			ViewData ["MEACode"] = items;
		}

		/// <summary>
		/// Profile the specified id, model and AvatarFile.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="model">Model.</param>
		/// <param name="AvatarFile">Avatar file.</param>
		[Authorize]
		[HttpPost]
		public ActionResult Profile (string id, ProfileEdition model, HttpPostedFileBase AvatarFile)
		{
			string logdu = User.Identity.Name;
			if (string.IsNullOrWhiteSpace (id)) {
				if (string.IsNullOrWhiteSpace (model.UserName)) {
					model.UserName = logdu;
					return View (model);
				} else {
					id = logdu;
				}
			}
			ViewData ["UserName"] = id;
			bool editsTheUserName = model.NewUserName!=null&&(string.Compare(id,model.NewUserName)!=0);
			// Checks authorisation
			if (logdu!=id)
			if (!Roles.IsUserInRole ("Admin"))
			if (!Roles.IsUserInRole ("FrontOffice"))
				throw new UnauthorizedAccessException ("Your are not authorized to modify this profile");
			// checks availability of a new username
			if (editsTheUserName)
			if (!UserNameManager.IsAvailable (model.NewUserName))
				ModelState.AddModelError ("UserName",
					string.Format (
						LocalizedText.DuplicateUserName,
						model.NewUserName
					));
			if (AvatarFile != null) { 
				// if said valid, move as avatar file
				// else invalidate the model
				if (AvatarFile.ContentType == "image/png") {
					string avdir = Server.MapPath (YavscHelpers.AvatarDir);
					var di = new DirectoryInfo (avdir);
					if (!di.Exists)
						di.Create ();
					string avpath = Path.Combine (avdir, id + ".png");
					AvatarFile.SaveAs (avpath);
					model.avatar = Url.Content( YavscHelpers.AvatarDir + "/" + id + ".png");
				} else
					ModelState.AddModelError ("Avatar",
						string.Format ("Image type {0} is not supported (suported formats : {1})",
							AvatarFile.ContentType, "image/png"));
			}
			if (ModelState.IsValid) {
				ProfileBase prf = ProfileBase .Create (id);
				prf.SetPropertyValue ("Name", model.Name);
				prf.SetPropertyValue ("BlogVisible", model.BlogVisible);
				prf.SetPropertyValue ("BlogTitle", model.BlogTitle);
				if (AvatarFile != null) { 
					prf.SetPropertyValue ("Avatar", model.avatar);
				} else {
					var av = prf.GetPropertyValue ("Avatar");
					if (av != null)
						model.avatar = av as string;
				}
				prf.SetPropertyValue ("Address", model.Address);
				prf.SetPropertyValue ("CityAndState", model.CityAndState);
				prf.SetPropertyValue ("Country", model.Country);
				prf.SetPropertyValue ("ZipCode", model.ZipCode);
				prf.SetPropertyValue ("WebSite", model.WebSite);
				prf.SetPropertyValue ("Name", model.Name);
				prf.SetPropertyValue ("Phone", model.Phone);
				prf.SetPropertyValue ("Mobile", model.Mobile);
				prf.SetPropertyValue ("BankCode", model.BankCode);
				prf.SetPropertyValue ("IBAN", model.IBAN);
				prf.SetPropertyValue ("BIC", model.BIC);
				prf.SetPropertyValue ("WicketCode", model.WicketCode);
				prf.SetPropertyValue ("AccountNumber", model.AccountNumber);
				prf.SetPropertyValue ("BankedKey", model.BankedKey);
				prf.SetPropertyValue ("gcalid", model.GoogleCalendar);
				prf.SetPropertyValue ("UITheme", model.UITheme);
				prf.SetPropertyValue ("MEACode", model.MEACode);
				prf.Save ();

				if (editsTheUserName) {
					UserNameManager.ChangeName (id, model.NewUserName);
					FormsAuthentication.SetAuthCookie (model.NewUserName, model.RememberMe);
					model.UserName = model.NewUserName;
				}
				ViewData.Notify( "Profile enregistré"+((editsTheUserName)?", nom public inclu.":""));
			}
			SetMEACodeViewData (model);
			SetUIThemeViewData (model);
			return View (model);
		}
		/// <summary>
		/// Circles this instance.
		/// </summary>
		[Authorize]
		public ActionResult Circles ()
		{
			string user = Membership.GetUser ().UserName;
			ViewData["Circles"] = CircleManager.DefaultProvider.List (user);
			return View ();
		}

		/// <summary>
		/// Logout the specified returnUrl.
		/// </summary>
		/// <param name="returnUrl">Return URL.</param>
		[Authorize]
		public ActionResult Logout (string returnUrl)
		{
			IdentitySignout ();
			return Redirect (returnUrl);
		}

		/// <summary>
		/// Losts the password.
		/// </summary>
		/// <returns>The password.</returns>
		/// <param name="model">Model.</param>
		public ActionResult ResetPassword(LostPasswordModel model)
		{
			if (Request.HttpMethod == "POST") {
				StringDictionary errors;
				MembershipUser user;
				YavscHelpers.ValidatePasswordReset (model, out errors, out user);
				foreach (string key in errors.Keys)
					ModelState.AddModelError (key, errors [key]);
				
				if (user != null && ModelState.IsValid) {
					YavscHelpers.SendNewPasswordMessage (user);
					ViewData.Notify ( new Notification () { 
						body = LocalizedText.NewPasswordMessageSent } );
				} 
			}
			return View (model);
		}

		/// <summary>
		/// Validate the specified id and key.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="key">Key.</param>
		[HttpGet]
		public ActionResult Validate (string id, string key)
		{
			MembershipUser u = Membership.GetUser (id, false);
			if (u == null) {
				ViewData.Notify( 
					string.Format ("Cet utilisateur n'existe pas ({0})", id));
			} else if (u.ProviderUserKey.ToString () == key) {
				if (u.IsApproved) {
					ViewData.Notify( 
						string.Format ("Votre compte ({0}) est déjà validé.", id));
				} else { 
					u.IsApproved = true;
					Membership.UpdateUser (u);
					ViewData.Notify( 
						string.Format ("La création de votre compte ({0}) est validée.", id));
				}
			} else
				ViewData.Notify( "La clé utilisée pour valider ce compte est incorrecte" );
			return View ();
		}

		/// <summary>
		/// Updates the password.
		/// </summary>
		/// <returns>The password.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="key">Key.</param>
		[HttpGet]
		public ActionResult UpdatePassword (string id, string key)
		{
			MembershipUser u = Membership.GetUser (id, false);
			if (u == null) {
				return Content(
					string.Format("Cet utilisateur n'existe pas ({0})", id));
			} 
			if (u.ProviderUserKey.ToString () != key) {
				return Content("Clé invalide");
			}
			ViewData["UserName"] = id;
			ViewData["UserKey"] = key;
			return View ();
		}

		/// <summary>
		/// Updates the password.
		/// </summary>
		/// <returns>The password.</returns>
		/// <param name="model">Model.</param>
		[HttpPost]
		public ActionResult UpdatePassword (UpdatePassword model)
		{
			if (ModelState.IsValid) {
				if (model.Password == model.ConfirmPassword) {
					MembershipUser u = Membership.GetUser (model.UserName, false);
					if (u == null) {
						return Content(
							string.Format("Cet utilisateur n'existe pas ({0})", model.UserName));
					} 
					if (u.ProviderUserKey.ToString () != model.UserKey) {
						return Content("Clé invalide");
					}
					u.ChangePassword(u.GetPassword (),model.Password);
					ViewData.Notify( "La mise à jour du mot de passe a été effectuée.");
					return View("Index");
				}
			}
			return View();
		}


	}
}
