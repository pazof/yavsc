using System;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.IO;
using System.Web.Configuration;
using System.Net.Mail;
using System.Web.Http.ModelBinding;
using Yavsc.Model.RolesAndMembers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;
using Yavsc.Model.Circles;
using System.Web.UI;
using System.Linq.Expressions;

namespace Yavsc.Helpers
{
	/// <summary>
	/// Yavsc helpers.
	/// </summary>
	public static class YavscHelpers
	{
		/// <summary>
		/// Formats the circle.
		/// </summary>
		/// <returns>The circle.</returns>
		/// <param name="c">C.</param>
		public static HtmlString FormatCircle (Circle c)
		{
			if (c.Members!=null)
			if (c.Members.Length > 0) {
				TagBuilder i = new TagBuilder ("i");
				i.SetInnerText (String.Join (", ", c.Members));
				return new HtmlString (c.Title+"<br/>\n"+i.ToString());
			}
			return new HtmlString (c.Title);
		}
		/// <summary>
		/// Formats the circle.
		/// </summary>
		/// <returns>The circle as Html string.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="c">C.</param>
		public static HtmlString FormatCircle(this HtmlHelper helper, Circle c)
		{
			return FormatCircle (c);
		}

		private static string siteName = null; 
		/// <summary>
		/// Gets the name of the site.
		/// </summary>
		/// <value>The name of the site.</value>
		public static string SiteName {
			get {
				if (siteName == null) 
					siteName = WebConfigurationManager.AppSettings ["Name"];
				return siteName;
			}
		}
		// Administrator email
		private static string admail =
			WebConfigurationManager.AppSettings ["AdminEmail"];
		/// <summary>
		/// Gets the Administrator email.
		/// </summary>
		/// <value>The admail.</value>
		public static string Admail {
			get {
				return admail;
			}
		}

		/// <summary>
		/// Sends the activation message.
		/// </summary>
		/// <param name="user">User.</param>
		public static void SendActivationMessage(MembershipUser user)
		{
			SendEmail (WebConfigurationManager.AppSettings ["RegistrationMessage"],
				user);
		}

		/// <summary>
		/// Sends the email.
		/// </summary>
		/// <param name="registrationMessage">Registration message.</param>
		/// <param name="user">User.</param>
		public static void SendEmail(string registrationMessage, MembershipUser user) {
			FileInfo fi = new FileInfo (
				HttpContext.Current.Server.MapPath (registrationMessage));
			if (!fi.Exists) {
				throw new Exception(
					string.Format (
						"Erreur inattendue (pas de corps de message " +
						"à envoyer pour le message de confirmation ({0}))",
						registrationMessage));
			}

			using (StreamReader sr = fi.OpenText ()) {
				string body = sr.ReadToEnd ();
				body = body.Replace ("<%SiteName%>", YavscHelpers.SiteName);
				body = body.Replace ("<%UserName%>", user.UserName);
				body = body.Replace ("<%UserActivatonUrl%>",
					string.Format ("<{0}://{1}/Account/Validate/{2}?key={3}>",
						HttpContext.Current.Request.Url.Scheme,
						HttpContext.Current.Request.Url.Authority,
						user.UserName,
						user.ProviderUserKey.ToString ()));
				using (MailMessage msg = new MailMessage (
					Admail, user.Email,
					string.Format ("Validation de votre compte {0}", YavscHelpers.SiteName),
					body)) {
					using (SmtpClient sc = new SmtpClient ()) {
						sc.Send (msg);
					}
				}

			}
		}

		/// <summary>
		/// Resets the password.
		/// </summary>
		/// <param name="model">Model.</param>
		/// <param name="errors">Errors.</param>
		public static void ResetPassword(LostPasswordModel model, out StringDictionary errors)
		{
			MembershipUserCollection users = null;
			errors = new StringDictionary ();

			if (!string.IsNullOrEmpty (model.UserName)) {
				users =
					Membership.FindUsersByName (model.UserName);
				if (users.Count < 1) {
					errors.Add ("UserName", "User name not found");
					return ;
				}
				if (users.Count != 1) {
					errors.Add ("UserName", "Found more than one user!(sic)");
					return ;
				}
			}
			if (!string.IsNullOrEmpty (model.Email)) {
				users =
					Membership.FindUsersByEmail (model.Email);
				if (users.Count < 1) {
					errors.Add ( "Email", "Email not found");
					return ;
				}
				if (users.Count != 1) {
					errors.Add ("Email", "Found more than one user!(sic)");
					return ;
				}
			}
			if (users==null)
				return;
			// Assert users.Count == 1
			if (users.Count != 1)
				throw new InvalidProgramException ("Emails and user's names are uniques, and we find more than one result here, aborting.");
			foreach (MembershipUser u in users) 
				YavscHelpers.SendActivationMessage (u);
		}
	}
}

