using System;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.IO;
using System.Web.Configuration;
using System.Net.Mail;
using Yavsc.Model.RolesAndMembers;
using System.Collections.Generic;
using System.Collections.Specialized;
using Yavsc.Model.Circles;
using System.Web.UI;
using System.Linq.Expressions;
using System.Web.Profile;
using System.Web.Script.Serialization;
using System.Web.Mvc;

namespace Yavsc.Helpers
{
	/// <summary>
	/// Yavsc helpers.
	/// </summary>
	public static class YavscHelpers
	{

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
		/// <param name="helper">Helper.</param>
		/// <param name="user">User.</param>
		public static void SendActivationMessage(this System.Web.Http.Routing.UrlHelper helper, MembershipUser user)
		{
			SendActivationMessage (helper.Route("Default", new { controller="Account", 
				action = "Validate",
				key=user.ProviderUserKey.ToString() } )
				, WebConfigurationManager.AppSettings ["RegistrationMessage"],
				user);
		}
		/// <summary>
		/// Sends the activation message.
		/// </summary>
		/// <param name="helper">Helper.</param>
		/// <param name="user">User.</param>
		public static void SendActivationMessage(this System.Web.Mvc.UrlHelper helper, MembershipUser user)
		{
			SendActivationMessage (helper.Content("~/Account/Validate/"+user.UserName+"/?key="+user.ProviderUserKey.ToString())
				, WebConfigurationManager.AppSettings ["RegistrationMessage"],
				user);
		}

		/// <summary>
		/// Sends the activation message.
		/// </summary>
		/// <param name="validationUrl">Validation URL.</param>
		/// <param name="registrationMessage">Registration message.</param>
		/// <param name="user">User.</param>
		public static void SendActivationMessage(string validationUrl, string registrationMessage, MembershipUser user) {
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
				body = body.Replace ("<%UserActivatonUrl%>", validationUrl);

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
		/// Validates the password reset.
		/// </summary>
		/// <param name="model">Model.</param>
		/// <param name="errors">Errors.</param>
		/// <param name="user">User.</param>
		public static void ValidatePasswordReset(LostPasswordModel model, out StringDictionary errors, out MembershipUser user)
		{
			MembershipUserCollection users = null;
			errors = new StringDictionary ();
			user = null;
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
			foreach (MembershipUser u in users) user = u;
		}
		/// <summary>
		/// Avatars the URL.
		/// </summary>
		/// <returns>The URL.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="username">Username.</param>
		public static string AvatarUrl (this System.Web.Mvc.UrlHelper helper, string username) {
			if (username == null)
				return null;
			ProfileBase pr = ProfileBase.Create (username);
			object avpath = null;
			if (pr != null) avpath = pr.GetPropertyValue("Avatar");
			if (avpath == null) return DefaultAvatar==null?"/bfiles/"+username+".png":DefaultAvatar;
			string avatarLocation = avpath as string;
			if (avatarLocation.StartsWith ("~/")) {
				avatarLocation = helper.RequestContext.HttpContext.Server.MapPath(avatarLocation);
			}
			return avatarLocation;
				
		}

		private static string avatarDir = "~/avatars";		
		private static string defaultAvatar = null;
		private static string defaultAvatarMimetype = null;
		public static string DefaultAvatar { 
			get {
				if (defaultAvatar == null)
					GetAvatarConfig ();
				return defaultAvatar;
			}
		}
		public static string AvatarDir { 
			get {
				return avatarDir;
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Controllers.BlogsController"/> class.
		/// </summary>
		private static void GetAvatarConfig ()
		{
			string[] defaultAvatarSpec = ConfigurationManager.AppSettings.Get ("DefaultAvatar").Split (';');
			if (defaultAvatarSpec.Length != 2)
				throw new ConfigurationErrorsException ("the DefaultAvatar spec should be found as <fileName>;<mime-type> ");
			defaultAvatar = defaultAvatarSpec [0];
			defaultAvatarMimetype = defaultAvatarSpec [1];
		}

		/// <summary>
		/// Javas the script.
		/// </summary>
		/// <returns>The script.</returns>
		/// <param name="html">Html.</param>
		/// <param name="obj">Object.</param>
		public static string JavaScript(this System.Web.Mvc.HtmlHelper html, object obj)
		{
			return JavaScript (obj);
		}
		/// <summary>
		/// Javas the script.
		/// </summary>
		/// <returns>The script.</returns>
		/// <param name="obj">Object.</param>
		public static string JavaScript(object obj)
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Serialize(obj);
		}
		/// <summary>
		/// Notifies 
		/// </summary>
		/// <param name="ViewData">View data.</param>
		/// <param name="message">Message.</param>
		public static void Notify (this HtmlHelper helper, string message) {
			Notify (helper.ViewData, message);
		}
		/// <summary>
		/// Notify the specified viewData and message.
		/// </summary>
		/// <param name="viewData">View data.</param>
		/// <param name="message">Message.</param>
		public static void Notify(ViewDataDictionary viewData, string message) {
			if (viewData ["Notifications"] == null)
				viewData ["Notifications"] = new List<string> ();
			(viewData ["Notifications"] as List<string>).Add (message.Replace("\'","\\\'"));
		}
		/// <summary>
		/// Files the list.
		/// </summary>
		/// <returns>The list.</returns>
		/// <param name="html">Html.</param>
		/// <param name="path">Path.</param>
		/// <param name="patterns">Patterns.</param>
		public static IHtmlString FileList(this System.Web.Mvc.HtmlHelper html, string path, string [] patterns = null) {
			StringWriter str = new StringWriter();
			HtmlTextWriter writter = new HtmlTextWriter (str);
			DirectoryInfo di = new DirectoryInfo (HttpContext.Current.Server.MapPath(path));
			if (!di.Exists)
				return new System.Web.Mvc.MvcHtmlString ("");
			var files = new List<FileInfo> ();
			if (patterns == null)
				patterns = new string[] { "*" };
			var url = new System.Web.Mvc.UrlHelper(html.ViewContext.RequestContext, 
				html.RouteCollection);

			foreach (string pattern in patterns) 
				files.AddRange(
					di.EnumerateFiles (
						pattern, 
						SearchOption.TopDirectoryOnly));
			writter.RenderBeginTag ("table");
			writter.RenderBeginTag ("tr");
			writter.RenderBeginTag ("td");
			writter.Write (html.Translate ("Name"));
			writter.RenderEndTag ();
			writter.RenderBeginTag ("td");
			writter.Write (html.Translate ("Created"));
			writter.RenderEndTag ();
			writter.RenderBeginTag ("td");
			writter.Write (html.Translate ("Modified"));
			writter.RenderEndTag ();
			writter.RenderEndTag ();
			foreach (FileInfo fi in files) {
				writter.RenderBeginTag ("tr");
				writter.RenderBeginTag ("td");
				writter.AddAttribute ("href", url.Content(path+"/"+fi.Name));
				writter.RenderBeginTag ("a");
				writter.Write (fi.Name);
				writter.RenderEndTag ();
				writter.RenderEndTag ();
				writter.RenderBeginTag ("td");
				writter.Write (fi.LastWriteTime.ToString ("U"));
				writter.RenderEndTag ();
				writter.RenderBeginTag ("td");
				writter.Write (fi.CreationTime.ToString("U"));
				writter.RenderEndTag ();
				writter.RenderEndTag ();
			}
			writter.RenderEndTag ();
			return new System.Web.Mvc.MvcHtmlString (str.ToString ());
		}

		/// <summary>
		/// Renders the page links.
		/// </summary>
		/// <returns>The page links.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="ResultCount">Result count.</param>
		/// <param name="PageSize">Page size.</param>
		/// <param name="PageIndex">Page index.</param>
		public static IHtmlString RenderPageLinks (
			this HtmlHelper helper,
			int PageIndex, int PageSize, int ResultCount, 
			string args="?PageIndex={0}",
			string pagesLabel="Pages: ", string singlePage="",
			string none="néant"
		)
		{
			StringWriter strwr = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter(strwr);

			if (ResultCount > 0 &&  ResultCount > PageSize ) {
				int pageCount = ((ResultCount-1) / PageSize) + 1;
				if ( pageCount > 1 ) {
					writer.WriteEncodedText (pagesLabel);
					for (int pi = (PageIndex < 5) ? 0 : PageIndex - 5; pi < pageCount && pi < PageIndex + 5; pi++) {
						if (PageIndex == pi)
							writer.RenderBeginTag ("b");
						else {
							writer.AddAttribute (HtmlTextWriterAttribute.Href,
								string.Format (args, pi));
							writer.RenderBeginTag ("a");
						}
						writer.Write (pi + 1);
						writer.RenderEndTag ();
						writer.Write ("&nbsp;");
					}
				} 
				else {
					writer.Write (singlePage);
				}
			} 
			if (ResultCount == 0) {
				writer.Write (none);
			}
			return new MvcHtmlString(strwr.ToString());
		}


	}
}

