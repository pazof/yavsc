using System;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.IO;
using System.Web.Configuration;
using System.Net.Mail;

namespace Yavsc.Helpers
{
	/// <summary>
	/// Yavsc helpers.
	/// </summary>
	public static class YavscHelpers
	{
		private static string registrationMessage =
			WebConfigurationManager.AppSettings ["RegistrationMessage"];


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
		/// Sends the activation email.
		/// </summary>
		/// <param name="user">User.</param>
		public static void SendActivationEmail(MembershipUser user) {
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

	}
}

