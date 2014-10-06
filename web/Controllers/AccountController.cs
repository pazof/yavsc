using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Profile;
using System.Web.Security;
using Yavsc;
using yavscModel.RolesAndMembers;
using Yavsc.Helpers;

namespace Yavsc.Controllers
{
	public class AccountController : Controller
	{
		private static string registrationMessage =
			WebConfigurationManager.AppSettings ["RegistrationMessage"];

		string avatarDir = "~/avatars";

		public string AvatarDir {
			get { return avatarDir; }
			set { avatarDir = value; }
		}

		public ActionResult Index ()
		{
			return View ();
		}

		public ActionResult Login (string returnUrl)
		{
			ViewData ["returnUrl"] = returnUrl;
			return View ();
		}

		[Authorize]
		public ActionResult Profile(Profile model)
		{
			ViewData ["UserName"] = Membership.GetUser ().UserName;
			model.FromProfileBase(HttpContext.Profile);
			return View (model);
		}
		// TODO [ValidateAntiForgeryToken]
		public ActionResult DoLogin (LoginModel model, string returnUrl)
		{
			if (ModelState.IsValid) {
				if (Membership.ValidateUser (model.UserName, model.Password)) {
					FormsAuthentication.SetAuthCookie (model.UserName, model.RememberMe);
					if (returnUrl != null)
						return Redirect (returnUrl);
					else return View ("Index");
				} else {
					ModelState.AddModelError ("UserName", "The user name or password provided is incorrect.");
				}
			}

			ViewData ["returnUrl"] = returnUrl;

			// If we got this far, something failed, redisplay form
			return View ("Login",model);
		}

		public ActionResult Register (RegisterViewModel model, string returnUrl)
		{
			ViewData["returnUrl"] = returnUrl;
			if (Request.RequestType == "GET") {
				foreach (string k in ModelState.Keys)
					ModelState [k].Errors.Clear ();
				return View (model);
			}
			if (ModelState.IsValid) {
				if (model.ConfirmPassword != model.Password)
				{
					ModelState.AddModelError("ConfirmPassword","Veuillez confirmer votre mot de passe");
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
					ModelState.AddModelError("Email", "Cette adresse e-mail correspond " +
						"à un compte utilisateur existant");
					return View (model);
				case MembershipCreateStatus.DuplicateUserName:
					ModelState.AddModelError("UserName", "Ce nom d'utilisateur est " +
						"déjà enregistré");
					return View (model);
				case MembershipCreateStatus.Success:
					FileInfo fi = new FileInfo (
						              Server.MapPath (registrationMessage));
					if (!fi.Exists) {
						ViewData ["Error"] = 
							string.Format (
							"Erreur inattendue (pas de corps de message " +
							"à envoyer pour le message de confirmation ({0}))",
							registrationMessage);
						return View (model);
					}

					using (StreamReader sr = fi.OpenText()) {
						string body = sr.ReadToEnd();
						body = body.Replace("<%SiteName%>",YavscHelpers.SiteName);
						body = body.Replace("<%UserName%>",user.UserName);
						body = body.Replace("<%UserActivatonUrl%>",
						                    string.Format("<{0}://{1}/Account/Validate/{2}?key={3}",
						              Request.Url.Scheme,
						              Request.Url.Authority,
						              user.UserName,
						             user.ProviderUserKey.ToString()));
						using (MailMessage msg = new MailMessage(
							HomeController.Admail,user.Email,
							string.Format("Validation de votre compte {0}",YavscHelpers.SiteName),
							body)) 
						{
							using (SmtpClient sc = new SmtpClient()) 
							{
								sc.Send (msg);
							}
						}

						ViewData ["username"] = user.UserName;
						ViewData ["email"] = user.Email;
						return View ("RegistrationPending");
					}
				default:
					ViewData["Error"] = "Une erreur inattendue s'est produite" +
						"a l'enregistrement de votre compte utilisateur" +
							string.Format("({0}).",mcs.ToString()) +
						"Veuillez pardonner la gêne" +
						"occasionnée";
					return View (model);
				}

			}
			return View (model);
		}

		public ActionResult ChangePasswordSuccess ()
		{
			return View ();
		}

		[HttpGet]
		[Authorize]
		public ActionResult ChangePassword()
		{
			return View();
		}

		[Authorize]
		[HttpPost]
		public ActionResult ChangePassword (ChangePasswordModel model)
		{
			if (ModelState.IsValid) {

				// ChangePassword will throw an exception rather
				// than return false in certain failure scenarios.
				bool changePasswordSucceeded=false;
				try {
					var users = Membership.FindUsersByName (model.Username);

					if (users.Count > 0) {
						MembershipUser user = Membership.GetUser (model.Username,true);	
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



		[Authorize]
		[HttpPost]
		//public ActionResult UpdateProfile(HttpPostedFileBase Avatar, string Address, string CityAndState, string ZipCode, string Country, string WebSite) 
		public ActionResult UpdateProfile(Profile model, HttpPostedFileBase AvatarFile) 
		{
			string username = Membership.GetUser ().UserName;

			if (AvatarFile != null) { 

				if (AvatarFile.ContentType == "image/png") {
					// byte[] img = new byte[AvatarFile.ContentLength];
					// AvatarFile.InputStream.Read (img, 0, AvatarFile.ContentLength);
					// model.Avatar = img;

					string avdir=Server.MapPath (AvatarDir);
					string avpath=Path.Combine(avdir,username+".png");
					AvatarFile.SaveAs (avpath);
				} else
					ModelState.AddModelError ("Avatar",
						string.Format ("Image type {0} is not supported (suported formats : {1})",
							AvatarFile.ContentType, "image/png")
					);
			}
			if (ModelState.IsValid) {
				HttpContext.Profile.SetPropertyValue (
					"Address", model.Address);
				HttpContext.Profile.SetPropertyValue (
					"BlogTitle", model.BlogTitle);
				HttpContext.Profile.SetPropertyValue (
					"BlogVisible", model.BlogVisible);
				HttpContext.Profile.SetPropertyValue (
					"CityAndState", model.CityAndState);
				HttpContext.Profile.SetPropertyValue (
					"Country", model.Country);
				HttpContext.Profile.SetPropertyValue (
					"WebSite", model.WebSite);

			}
			// HttpContext.Profile.SetPropertyValue("Avatar",Avatar);
			return RedirectToAction ("Profile");
		}

		[Authorize]
		public ActionResult Logout (string returnUrl)
		{
			FormsAuthentication.SignOut();
			return Redirect(returnUrl);
		}



		[HttpGet]
		public ActionResult Validate (string id, string key)
		{
			MembershipUser u = Membership.GetUser (id, false);
			if (u == null) {
				ViewData ["Error"] = 
					string.Format ("Cet utilisateur n'existe pas ({0})", id);
			}
			else 
			if (u.ProviderUserKey.ToString () == key) {
				u.IsApproved = true;
				Membership.UpdateUser(u);
				ViewData["Message"] = 
					string.Format ("La création de votre compte ({0}) est validée.", id);
			}
			else ViewData["Error"] = "La clé utilisée pour valider ce compte est incorrecte";
			return View ();
		}
	}
}
