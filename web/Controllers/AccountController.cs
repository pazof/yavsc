using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Profile;
using System.Web.Security;
using Yavsc;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Helpers;
using System.Web.Mvc;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Account controller.
	/// </summary>
	public class AccountController : Controller
	{
	
		string avatarDir = "~/avatars";

		/// <summary>
		/// Gets or sets the avatar dir.
		/// This value is past to <c>Server.MapPath</c>,
		/// it should start with <c>~/</c>, and we assume it
		/// to be relative to the application path.
		/// </summary>
		/// <value>The avatar dir.</value>
		public string AvatarDir {
			get { return avatarDir; }
			set { avatarDir = value; }
		}

		/// <summary>
		/// Index this instance.
		/// </summary>
		public ActionResult Index ()
		{
			return View ();
		}

		// TODO [ValidateAntiForgeryToken]
		/// <summary>
		/// Does login.
		/// </summary>
		/// <returns>The login.</returns>
		/// <param name="model">Model.</param>
		/// <param name="returnUrl">Return URL.</param>
		public ActionResult Login (LoginModel model, string returnUrl)
		{
			if (ModelState.IsValid) {
				if (Membership.ValidateUser (model.UserName, model.Password)) {
					FormsAuthentication.SetAuthCookie (model.UserName, model.RememberMe);
					if (returnUrl != null)
						return Redirect (returnUrl);
					else
						return View ("Index");
				} else {
					ModelState.AddModelError ("UserName", "The user name or password provided is incorrect.");
				}
			}

			ViewData ["returnUrl"] = returnUrl;

			// If we got this far, something failed, redisplay form
			return View (model);
		}

		/// <summary>
		/// Register the specified model and returnUrl.
		/// </summary>
		/// <param name="model">Model.</param>
		/// <param name="returnUrl">Return URL.</param>
		public ActionResult Register (RegisterViewModel model, string returnUrl)
		{
			ViewData ["returnUrl"] = returnUrl;
			if (Request.RequestType == "GET") {
				foreach (string k in ModelState.Keys)
					ModelState [k].Errors.Clear ();
				return View (model);
			}
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
					YavscHelpers.SendActivationEmail (user);
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
		/// Unregister the specified confirmed.
		/// </summary>
		/// <param name="confirmed">If set to <c>true</c> confirmed.</param>
		[Authorize]
		public ActionResult Unregister (bool confirmed = false)
		{
			if (!confirmed)
				return View ();

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
					var users = Membership.FindUsersByName (model.Username);

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
		/// Profile the specified user.
		/// </summary>
		/// <param name="user">User name.</param>
		[Authorize]
		[HttpGet]
		public ActionResult Profile (string user)
		{
			ViewData ["ProfileUserName"] = user;
			string logdu = Membership.GetUser ().UserName;
			ViewData ["UserName"] = logdu;
			if (user == null)
				user = logdu;
			Profile model = new Profile (ProfileBase.Create (user));
			model.RememberMe = FormsAuthentication.GetAuthCookie (user, true) == null;
			return View (model);
		}

		/// <summary>
		/// Profile the specified user, model and AvatarFile.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="model">Model.</param>
		/// <param name="AvatarFile">Avatar file.</param>
		[Authorize]
		[HttpPost]
		public ActionResult Profile (string user, Profile model, HttpPostedFileBase AvatarFile)
		{
			// ASSERT("Membership.GetUser ().UserName is made of simple characters, no slash nor backslash"

			string logdu = Membership.GetUser ().UserName;
			ViewData ["UserName"] = logdu;
			bool editsMyProfile = (user == logdu);
			if (!editsMyProfile)
			if (!Roles.IsUserInRole ("Admin"))
			if (!Roles.IsUserInRole ("FrontOffice"))
				throw new UnauthorizedAccessException ("Your are not authorized to modify this profile");


			if (AvatarFile != null) { 
				// if said valid, move as avatar file
				// else invalidate the model
				if (AvatarFile.ContentType == "image/png") {
					string avdir = Server.MapPath (AvatarDir);
					string avpath = Path.Combine (avdir, user + ".png");
					AvatarFile.SaveAs (avpath);
					model.avatar = Request.Url.Scheme + "://" + Request.Url.Authority + AvatarDir.Substring (1) + "/" + user + ".png";
				} else
					ModelState.AddModelError ("Avatar",
						string.Format ("Image type {0} is not supported (suported formats : {1})",
							AvatarFile.ContentType, "image/png"));
			}
			/* Sync the property in the Profile model to display :
		 *  string cAvat = HttpContext.Profile.GetPropertyValue ("avatar") as string;
			if (cAvat != null) if (model.avatar == null) model.avatar = cAvat;
			*/
			if (ModelState.IsValid) {
				ProfileBase prf = ProfileBase .Create (model.UserName);
				prf.SetPropertyValue ("BlogVisible", model.BlogVisible);
				prf.SetPropertyValue ("BlogTitle", model.BlogTitle);
				prf.SetPropertyValue ("avatar", model.avatar);
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
				prf.Save ();

				// only do the following if this profile belongs to current user
				if (editsMyProfile)
					FormsAuthentication.SetAuthCookie (user, model.RememberMe);
				ViewData ["Message"] = "Profile enregistré"+((editsMyProfile)?", cookie modifié.":"");
			}
			return View (model);
		}

		/// <summary>
		/// Logout the specified returnUrl.
		/// </summary>
		/// <param name="returnUrl">Return URL.</param>
		[Authorize]
		public ActionResult Logout (string returnUrl)
		{
			FormsAuthentication.SignOut ();
			return Redirect (returnUrl);
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
				ViewData ["Error"] = 
					string.Format ("Cet utilisateur n'existe pas ({0})", id);
			} else if (u.ProviderUserKey.ToString () == key) {
				u.IsApproved = true;
				Membership.UpdateUser (u);
				ViewData ["Message"] = 
					string.Format ("La création de votre compte ({0}) est validée.", id);
			} else
				ViewData ["Error"] = "La clé utilisée pour valider ce compte est incorrecte";
			return View ();
		}
	}
}
