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
using Yavsc.Model.Circles;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Configuration;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Account controller.
	/// </summary>
	public class AccountController : Controller
	{
		/// <summary>
		/// Avatar the specified user.
		/// </summary>
		/// <param name="id">User.</param>
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

		// TODO [ValidateAntiForgeryToken]
		/// <summary>
		/// Does login.
		/// </summary>
		/// <returns>The login.</returns>
		/// <param name="model">Model.</param>
		/// <param name="returnUrl">Return URL.</param>
		[HttpPost]
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
			ViewData ["returnUrl"] = returnUrl;
			return View ("Register",model);
		}

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
			string logged = Membership.GetUser ().UserName;
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
			Profile model = new Profile (ProfileBase.Create (id));
			model.RememberMe = FormsAuthentication.GetAuthCookie (id, true) == null;
			return View (model);
		}



		/// <summary>
		/// Profile the specified id, model and AvatarFile.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="model">Model.</param>
		/// <param name="AvatarFile">Avatar file.</param>
		[Authorize]
		[HttpPost]
		public ActionResult Profile (string id, Profile model, HttpPostedFileBase AvatarFile)
		{
			// ASSERT("Membership.GetUser ().UserName is made of simple characters, no slash nor backslash"

			string logdu = Membership.GetUser ().UserName;
			if (string.IsNullOrWhiteSpace (id))
				id = logdu;
			ViewData ["UserName"] = id;
			bool editsMyName = (string.Compare(id,logdu)==0);
			if (!editsMyName)
			if (!Roles.IsUserInRole ("Admin"))
			if (!Roles.IsUserInRole ("FrontOffice"))
				throw new UnauthorizedAccessException ("Your are not authorized to modify this profile");

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
			/* Sync the property in the Profile model to display :
		 *  string cAvat = HttpContext.Profile.GetPropertyValue ("Avatar") as string;
			if (cAvat != null) if (model.avatar == null) model.avatar = cAvat;
			*/
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
				prf.Save ();

				if (editsMyName) {
					UserManager.ChangeName (id, model.Name);
					FormsAuthentication.SetAuthCookie (model.Name, model.RememberMe);
				}
				YavscHelpers.Notify(ViewData, "Profile enregistré"+((editsMyName)?", nom public inclu.":""));
			}
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
			FormsAuthentication.SignOut ();
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
				
				if (user != null && ModelState.IsValid)
					Url.SendActivationMessage (user);
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
				YavscHelpers.Notify( ViewData,
					string.Format ("Cet utilisateur n'existe pas ({0})", id));
			} else if (u.ProviderUserKey.ToString () == key) {
				if (u.IsApproved) {
					YavscHelpers.Notify( ViewData,
						string.Format ("Votre compte ({0}) est déjà validé.", id));
				} else { 
					u.IsApproved = true;
					Membership.UpdateUser (u);
					YavscHelpers.Notify( ViewData,
						string.Format ("La création de votre compte ({0}) est validée.", id));
				}
			} else
				YavscHelpers.Notify( ViewData, "La clé utilisée pour valider ce compte est incorrecte" );
			return View ();
		}

	}
}
