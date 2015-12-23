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
using System.Text.RegularExpressions;
using Yavsc.Model.Messaging;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Helpers
{

	/// <summary>
	/// Yavsc helpers.
	/// </summary>
	public static class YavscHelpers
	{
		/* {"Need": "11 21,10 14", "MEACode": "6829C", "AUTH_USER": "Paul Schneider", "REMOTE_ADDR": "127.0.0.1",
		 * "REMOTE_PORT": "43376", "REMOTE_USER": "", "PreferedDate": "22/12/2015 00:00:00", 
		 * "PerformerName": "Paul Schneider"} */ 
		public static string[] FilteredKeys = {
			"URL",	"HTTPS",	"ALL_RAW",	"ALL_HTTP", "ALL_RAW",	"HTTP_DNT",	"PATH_INFO",
			".ASPXFORM$",	"CERT_FLAGS",	"LOCAL_ADDR",
			"LOGON_USER",	"CERT_COOKIE",	"CERT_ISSUER",	"HTTP_ACCEPT", "HTTP_COOKIE",	"INSTANCE_ID",
			//	"REMOTE_ADDR",
				"REMOTE_HOST",
				"REMOTE_PORT",
				"REMOTE_USER",
			"SCRIPT_NAME",	"SERVER_NAME",	"SERVER_PORT",	"APPL_MD_PATH",	"CERT_KEYSIZE",
			"CERT_SUBJECT",	"CONTENT_TYPE",		"HTTP_REFERER",
			"AUTH_PASSWORD",	"HTTPS_KEYSIZE",	".ASPXANONYMOUS",	"CONTENT_LENGTH",
			"REQUEST_METHOD",	"HTTP_CONNECTION",	"HTTP_USER_AGENT",	"PATH_TRANSLATED",
			"SERVER_PROTOCOL",	"HTTP_ACCEPT_LANGUAGE",	"AUTH_TYPE",
			// "AUTH_USER", 
			"HTTP_HOST", "QUERY_STRING", "SERVER_SOFTWARE", "ASP.NET_SessionId", 
			"CERT_SERIALNUMBER", "GATEWAY_INTERFACE", "HTTP_CONTENT_TYPE",
			"APPL_PHYSICAL_PATH", "CERT_SECRETKEYSIZE", "CERT_SERVER_ISSUER", 
			"INSTANCE_META_PATH", "SERVER_PORT_SECURE",
			"CERT_SERVER_SUBJECT", "HTTPS_SECRETKEYSIZE", "HTTPS_SERVER_ISSUER", 
			"HTTP_CONTENT_LENGTH", "HTTPS_SERVER_SUBJECT", "HTTP_ACCEPT_ENCODING", "HTTP_ACCEPT_LANGUAGE"
		};
		public static long CreateCommandFromRequest()
		{
			var keys = HttpContext.Current.Request.Params.AllKeys.Where (
				x => !YavscHelpers.FilteredKeys.Contains (x)).ToArray();
			var prms = new Dictionary<string,string> ();

			foreach (var key in keys) { 
				prms.Add (key, HttpContext.Current.Request.Params [key]);
			}
			Command cmd = Command.CreateCommand(
				prms, 
				HttpContext.Current.Request.Files);
			return cmd.Id;
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
		/// <param name="helper">Helper.</param>
		/// <param name="user">User.</param>
		public static void SendActivationMessage(this System.Web.Http.Routing.UrlHelper helper, MembershipUser user)
		{
			SendActivationMessage (helper.Route("Default", new { controller="Account", 
				action = "Validate",
				key=user.ProviderUserKey.ToString(), id = user.UserName } )
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
			SendActivationMessage (
				string.Format("{2}://{3}/Account/Validate/{1}?key={0}",
					user.ProviderUserKey.ToString(), user.UserName , 
					helper.RequestContext.HttpContext.Request.Url.Scheme,
					helper.RequestContext.HttpContext.Request.Url.Authority
				)
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
			if (username == null) return null;
			ProfileBase pr = ProfileBase.Create (username);
			object avpath = null;
			if (pr != null) avpath = pr.GetPropertyValue("Avatar");
			if (avpath == null || avpath is DBNull)
				return DefaultAvatar==null?"/bfiles/"+username+".png":DefaultAvatar;
			string avatarLocation = avpath as string;
			if (avatarLocation.StartsWith ("~/")) {
				avatarLocation = helper.RouteUrl("Default", avatarLocation);
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

		public static void Notify(ViewDataDictionary ViewData, string message, string click_action=null, string clickActionName="Ok") {
			Notify(ViewData, new Notification { body = YavscAjaxHelper.QuoteJavascriptString(message), 
				click_action = click_action, click_action_name = YavscAjaxHelper.QuoteJavascriptString(clickActionName)} ) ;
		}

		public static void Notify(ViewDataDictionary ViewData, Notification note) {
			if (ViewData ["Notifications"] == null)
				ViewData ["Notifications"] = new List<Notification> ();
			(ViewData ["Notifications"] as List<Notification>).Add (
				note ) ;
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
		/// <param name="PageIndex">Page index.</param>
		/// <param name="PageSize">Page size.</param>
		/// <param name="ResultCount">Result count.</param>
		/// <param name="args">Arguments.</param>
		/// <param name="pagesLabel">Pages label.</param>
		/// <param name="singlePage">Single page.</param>
		/// <param name="none">None.</param>
		/// <param name="cssClass">Css class.</param>
		public static IHtmlString RenderPageLinks (
			this HtmlHelper helper,
			int PageIndex, int PageSize, int ResultCount, 
			string args="?PageIndex={0}",
			string pagesLabel="Pages: ", string singlePage="",
			string none="néant", string cssClass = "pagelink"
		)
		{
			StringWriter strwr = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter(strwr);
			if (PageSize <= 0)
				PageSize = 0;
			if (ResultCount > 0 &&  ResultCount > PageSize ) {
				int pageCount = ((ResultCount-1) / PageSize) + 1;
				if ( pageCount > 1 ) {

					if (cssClass!=null)
						writer.AddAttribute ("class", cssClass);
					writer.RenderBeginTag ("div");
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
					writer.RenderEndTag ();
				} 
				else {
					writer.Write (singlePage);
				}
			} 
			if (ResultCount == 0) {
				writer.WriteEncodedText(none);
			}
			return new MvcHtmlString(strwr.ToString());
		}
		/// <summary>
		/// The available themes.
		/// </summary>
		public static string[] AvailableThemes = 
			ConfigurationManager.AppSettings["Themes"]
				.Split(',');

		/// <summary>
		/// Themes the CSS links.
		/// </summary>
		/// <returns>The CSS links.</returns>
		/// <param name="html">Html.</param>
		/// <param name="theme">Theme.</param>
		/// <param name="baseName">Base name.</param>
		public static IHtmlString ThemeCSSLinks (
			this System.Web.Mvc.HtmlHelper html, string theme, string baseName) {

			if (!AvailableThemes.Contains (theme))
			if (AvailableThemes.Length > 0)
				theme = AvailableThemes [0];
			else
				return null;
			
			if (string.IsNullOrWhiteSpace(baseName))
				throw new ArgumentException ("Specify a base name");

			StringWriter strwr = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter(strwr);
			// refer to the global style
			writer.AddAttribute ("rel", "stylesheet");
			writer.AddAttribute ("href", 
				string.Format(
					"/App_Themes/{1}.css",
					theme,baseName));
			writer.RenderBeginTag ("link");

			// refer to the themed style
			writer.AddAttribute ("rel", "stylesheet");
			writer.AddAttribute ("title", theme);
			writer.AddAttribute ("href", 
				string.Format(
					"/App_Themes/{0}/{1}.css",
					theme,baseName));
			writer.RenderBeginTag ("link");
			
			// refer to alternate styles
			foreach (string atheme in AvailableThemes) {
				if (atheme != theme) {
					writer.AddAttribute ("rel", "alternate stylesheet");
					writer.AddAttribute ("title", atheme);
					writer.AddAttribute ("href", 
						string.Format (
							"/App_Themes/{0}/{1}.css",
							atheme, baseName));
					writer.RenderBeginTag ("link");
				}
			}
			return new MvcHtmlString(strwr.ToString());
		}
		private static object defaultHtmlAttributes =
			new { @class="actionlink" };
		
		/// <summary>
		/// Translateds the action link.
		/// </summary>
		/// <returns>The action link.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="actionName">Action name.</param>
		/// <param name="htmlAttributes">Html attributes.</param>
		public static IHtmlString TranslatedActionLink (this HtmlHelper helper, 
			string actionLabel, object htmlAttributes = null) {
			return TranslatedActionLink (helper, actionLabel, actionLabel, htmlAttributes);
		}

		/// <summary>
		/// Translateds the action link.
		/// </summary>
		/// <returns>The action link.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="actionName">Action name.</param>
		/// <param name="method">Method.</param>
		/// <param name="htmlAttributes">Html attributes.</param>
		public static IHtmlString TranslatedActionLink (this HtmlHelper helper, 
			string actionName, string method, object routes, object htmlAttributes = null)  {

			string controllerName = helper.ViewContext.Controller.GetType ().Name;
			if (controllerName.EndsWith ("Controller"))
				controllerName = controllerName.Substring (0,controllerName.Length - 10);
			return TranslatedActionLink (helper, actionName, method,
				controllerName, routes, htmlAttributes);
		}
		/// <summary>
		/// Translateds the action link.
		/// </summary>
		/// <returns>The action link.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="actionName">Action name.</param>
		/// <param name="method">Method.</param>
		/// <param name="controller">Controller.</param>
		/// <param name="htmlAttributes">Html attributes.</param>
		public static IHtmlString TranslatedActionLink (this HtmlHelper helper, 
			string actionLabel, string method, string controller, object htmlAttributes = null) {
			string controllerName = helper.ViewContext.Controller.GetType ().Name;
			if (controllerName.EndsWith ("Controller"))
				controllerName = controllerName.Substring (0,controllerName.Length - 10);
			if (controller == null)
				controller = controllerName;
			return TranslatedActionLink (helper, actionLabel, method, controller, 
				new { controller = controller, action = actionLabel }, htmlAttributes);
		}
		/// <summary>
		/// Translateds the action link.
		/// </summary>
		/// <returns>The action link.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="actionName">Action name.</param>
		/// <param name="method">Method.</param>
		/// <param name="controller">Controller.</param>
		/// <param name="routes">Routes.</param>
		/// <param name="htmlAttributes">Html attributes.</param>
		public static IHtmlString TranslatedActionLink (this HtmlHelper helper, 
			string actionLabel, string actionName, string controller, object routes, object htmlAttributes = null)  {


			if (htmlAttributes == null)
				htmlAttributes = defaultHtmlAttributes;
			IHtmlString text = T.Translate (helper, actionLabel);
			StringWriter strwr = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter(strwr);

			foreach (var ppt in htmlAttributes.GetType().GetProperties(BindingFlags.GetProperty|
				BindingFlags.Public)) {
				writer.AddAttribute(ppt.Name, ppt.GetValue(htmlAttributes).ToString())
				;
			}
			writer.AddAttribute ("href",
				UrlHelper.GenerateUrl (
					"Default", actionName, controller,
					( routes == null ) ? null : new RouteValueDictionary ( routes ) , 
					helper.RouteCollection,
					helper.ViewContext.RequestContext,
					false));
			writer.RenderBeginTag ("a");
			writer.Write (text);
			writer.RenderEndTag ();
			return new MvcHtmlString(strwr.ToString());

		
		}






	}
}

