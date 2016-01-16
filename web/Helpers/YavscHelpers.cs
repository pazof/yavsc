using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using MailKit.Net.Smtp;
using MimeKit;
using Yavsc.Model;
using Yavsc.Model.Calendar;
using Yavsc.Model.Circles;
using Yavsc.Model.FileSystem;
using Yavsc.Model.FrontOffice;
using Yavsc.Helpers.Google.Api;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Model.WorkFlow;
using Yavsc.Client;
using Yavsc.Client.Events;
using Yavsc.Client.FrontOffice;
using Yavsc.Client.Messaging;
using Yavsc.Client.Maps;
using System.Net.Configuration;
using System.Web.Profile;
using Yavsc.Model.Identity;

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
			"HTTP_CONTENT_LENGTH", "HTTPS_SERVER_SUBJECT", "HTTP_ACCEPT_ENCODING", "HTTP_ACCEPT_LANGUAGE",
			"HTTP_CACHE_CONTROL", "__RequestVerificationToken"
		};

		/// <summary>
		/// Froms the user.
		/// </summary>
		/// <param name="state">State.</param>
		/// <param name="user">User.</param>
		public static void FromUser(this AppUserState state, MembershipUser user)
		{
			if (user == null) {
				state.UserId = Guid.NewGuid ().ToString ();
				return;
			}
			state.UserId = user.ProviderUserKey.ToString();
			state.Name = user.UserName;
			state.Email = user.Email;
			state.IsAdmin = Roles.IsUserInRole ("Admin");
			state.Theme = (string) ProfileBase.Create(state.Name).GetPropertyValue("UITheme");
		}

		/// <summary>
		/// Populate the specified userProfile and username.
		/// </summary>
		/// <param name="userProfile">User profile.</param>
		/// <param name="username">Username.</param>
		public static void Populate(this Profile userProfile, string username)
		{
			ProfileBase profile = ProfileBase.Create(username);
			if (profile == null) throw new Exception ("No profile");
			userProfile.UserName = username;
			userProfile.UITheme = (string) profile.GetPropertyValue ("UITheme");
			if (profile.IsAnonymous) return;
			userProfile.IsAdmin = Roles.IsUserInRole ("Admin");
			userProfile.Email = Membership.GetUser (username).Email;
			userProfile.BlogVisible = (bool) profile.GetPropertyValue ("BlogVisible");
			userProfile.BlogTitle = (string) profile.GetPropertyValue ("BlogTitle");
			userProfile.avatar = (string) profile.GetPropertyValue ("Avatar");
			userProfile.Address = (string) profile.GetPropertyValue ("Address"); 
			userProfile.CityAndState = (string) profile.GetPropertyValue ("CityAndState");
			userProfile.Country = (string) profile.GetPropertyValue ("Country");
			userProfile.ZipCode = (string) profile.GetPropertyValue ("ZipCode");
			userProfile.WebSite = (string) profile.GetPropertyValue ("WebSite");
			userProfile.Name = (string) profile.GetPropertyValue ("Name");
			userProfile.Phone = (string) profile.GetPropertyValue ("Phone");
			userProfile.Mobile = (string) profile.GetPropertyValue ("Mobile");
			userProfile.BankCode = (string)profile.GetPropertyValue ("BankCode");
			userProfile.IBAN = (string)profile.GetPropertyValue ("IBAN");
			userProfile.BIC =  (string)profile.GetPropertyValue ("BIC");
			userProfile.WicketCode = (string) profile.GetPropertyValue ("WicketCode");
			userProfile.AccountNumber = (string) profile.GetPropertyValue ("AccountNumber");
			userProfile.BankedKey = (int)  profile.GetPropertyValue ("BankedKey");
			userProfile.GoogleCalendar = (string) profile.GetPropertyValue ("gcalid");
			userProfile.GoogleRegId = (string) profile.GetPropertyValue ("gregid");
			userProfile.MEACode = (string) profile.GetPropertyValue ("MEACode");
		}

		public static CommandRegistration CreateCommandFromRequest(string userProfileUrl)
		{
			var keys = HttpContext.Current.Request.Params.AllKeys.Where (
				x => !YavscHelpers.FilteredKeys.Contains (x)).ToArray();
			var prms = new Dictionary<string,string> ();
			CommandRegistration cmdreg;
			foreach (var key in keys) { 
				prms.Add (key, HttpContext.Current.Request.Params [key]);
			}
			CommandRegistration comreg;
			var files = HttpContext.Current.Request.Files;
			var fileList = new List<HttpPostedFileBase> ();
			// for (int i = 0; i < files.Count; i++) fileList.Add ( files [i] as HttpPostedFileBase );
			Command  com =
				CreateCommand(
					prms, fileList.ToArray(), 
				out cmdreg);
			if (typeof(NominativeCommandRegistration).IsAssignableFrom (cmdreg.GetType ())) {
				// TODO send a message with topic /global/commandregistration
				var nomcomreg = cmdreg as NominativeCommandRegistration;
				INominative cmdn = com as INominative;
				NominativeEventPub ev = new NominativeEventPub ();
				ev.PerformerName = cmdn.PerformerName;
				string desc = com.GetDescription ();
				desc += userProfileUrl;
				ev.Description = desc;
				ev.Title = LocalizedText.EstimateWanted;

				if (com.GetType ().GetInterface ("ILocation") != null) {
					ILocation loc = com as ILocation;
					ev.Location = new Location { 
						Address = loc.Address,
						Latitude = loc.Latitude,
						Longitude = loc.Longitude
					};
				}
				try { 
					var gnresponse = GoogleHelpers.NotifyEvent (ev);
					if (gnresponse.failure > 0 || gnresponse.success <= 0)
						nomcomreg.NotifiedOnMobile = false;
					else
						nomcomreg.NotifiedOnMobile = true;

				} catch (WebException ex) {
					string errorMsgGCM;
					using (var respstream = ex.Response.GetResponseStream ()) {
						using (StreamReader rdr = new StreamReader (respstream)) {
							errorMsgGCM = rdr.ReadToEnd ();
							rdr.Close ();
						}
						respstream.Close ();
					}
					if (errorMsgGCM == null)
						throw;

					throw new Exception (errorMsgGCM, ex);
				}
				string errorEMail = null;
				try {
					var pref = Membership.GetUser (cmdn.PerformerName);
					MimeMessage msg = new MimeMessage ();
					msg.From.Add (new MailboxAddress ("Owner", WebConfigurationManager.AppSettings.Get ("OwnerEMail")));
					msg.To.Add (new MailboxAddress (cmdn.PerformerName,
						pref.Email));
					msg.Body = new TextPart ("plain") {
						Text = desc};

					using (SmtpClient sc = new SmtpClient ()) {

						Configuration c = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
						MailSettingsSectionGroup settings = (MailSettingsSectionGroup)c.GetSectionGroup("system.net/mailSettings");
						 ;

						sc.Connect(settings.Smtp.Network.Host,settings.Smtp.Network.Port,
							settings.Smtp.Network.EnableSsl);
						sc.Send (msg);
						nomcomreg.EmailSent = true;
					}

				} catch (Exception ex) {
					errorEMail = ex.Message;
					nomcomreg.EmailSent = false;
				}

			} else {
				var ev = new YaEvent {
					Title = LocalizedText.EstimateWanted,
					Description = com.GetDescription (),
				};
				throw new NotImplementedException ();
			}

			return cmdreg;
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
		private static string owneremail = WebConfigurationManager.AppSettings.Get ("OwnerEMail");
		/// <summary>
		/// Gets or sets the owner email.
		/// </summary>
		/// <value>The owner email.</value>
		public static string OwnerEmail {
			get {
				return owneremail;
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
					user.ProviderUserKey.ToString(), 
					HttpUtility.UrlEncode(user.UserName), 
					helper.RequestContext.HttpContext.Request.Url.Scheme,
					helper.RequestContext.HttpContext.Request.Url.Authority
				)
				, WebConfigurationManager.AppSettings ["RegistrationMessage"],
				user);
		}
		public static string GetAvatar(this PerformerProfile profile)
		{
			return ProfileBase.Create (profile.UserName).GetPropertyValue ("Avatar") as string;
		}
		public static bool HasCalendar(this PerformerProfile profile)
		{
			return ProfileBase.Create (profile.UserName).GetPropertyValue ("gcalid") as string != null;
		}

		public static void SendNewPasswordMessage(MembershipUser user)
		{
			SendActivationMessage (
				string.Format("{2}://{3}/Account/UpdatePassword/{1}?key={0}",
					user.ProviderUserKey.ToString(), 
					HttpUtility.UrlEncode(user.UserName), 
					HttpContext.Current.Request.Url.Scheme,
					HttpContext.Current.Request.Url.Authority
				)
				,  WebConfigurationManager.AppSettings ["LostPasswordMessage"],
				user,
				"Mot de passe {0} perdu"
			);
		}


		/// <summary>
		/// Sends the activation message.
		/// </summary>
		/// <param name="validationUrl">Validation URL.</param>
		/// <param name="registrationMessage">Registration message.</param>
		/// <param name="user">User.</param>
		public static void SendActivationMessage(string validationUrl, 
			string registrationMessage, MembershipUser user, string title = "Validation de votre compte {0}") {
			FileInfo fi = new FileInfo (
				HttpContext.Current.Server.MapPath (registrationMessage));
			if (!fi.Exists) {
				throw new Exception(
					string.Format (
						"Erreur inattendue (pas de corps de message " +
						"à envoyer pour le message de confirmation ({0}))",
						registrationMessage));
			}
			string body = null;
			using (StreamReader sr = fi.OpenText ()) {
				body = sr.ReadToEnd ();
			}
			body = body.Replace ("<%SiteName%>", YavscHelpers.SiteName);
			body = body.Replace ("<%UserName%>", user.UserName);
			body = body.Replace ("<%UserActivatonUrl%>", validationUrl);
			string titleWithSiteName = string.Format (title, YavscHelpers.SiteName);
			var mimeBody = new  TextPart ("plain") {Text=body};
			/* TODO 
			 * multipart in case of attachment:
			 * var multipart = new Multipart ("mixed");
			multipart.Add (mimeBody); */
			MimeMessage msg = new MimeMessage ();
			msg.From.Add (new MailboxAddress ("Admin",Admail) );
			msg.To.Add (new MailboxAddress (user.UserName,user.Email)); 
			msg.Subject = titleWithSiteName;
			msg.Body = mimeBody;
			using (SmtpClient client = new SmtpClient ()) {
				client.Connect ("localhost", 25);
				// Note: since we don't have an OAuth2 token, disable
				// the XOAUTH2 authentication mechanism.
				client.AuthenticationMechanisms.Remove ("XOAUTH2");
				client.Send (msg);
				client.Disconnect (true);
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
			MembershipUser user1 = null;
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
				foreach (var u in users) user1 = u as MembershipUser;
			}
			if (string.IsNullOrEmpty (model.Email))
				errors.Add ("Email", "Please, specify an e-mail");
			else {
				users = Membership.FindUsersByEmail (model.Email);
				if (users.Count < 1) {
					errors.Add ("Email", "Email not found");
					return;
				}
				if (users.Count != 1) {
					errors.Add ("Email", "Found more than one user!(sic)");
					return ;
				}
			}
			MembershipUser user2 = null;
			foreach (var u in users) user2 = u as MembershipUser;
			if (user1.UserName != user2.UserName)
				errors.Add("UserName", "This user is not registered with this e-mail");
			user = user1;
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

		public static void Notify(this ViewDataDictionary ViewData, string message, string click_action=null, string clickActionName="Ok") {
			Notify(ViewData, new Notification { body = message, 
				click_action = click_action, click_action_name = clickActionName} ) ;
		}

		public static void Notify(this ViewDataDictionary ViewData, Notification note) {
			
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
			if (AvailableThemes==null) return null;
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
			string actionName, string method, object routes = null, object htmlAttributes = null)  {
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
		/// <param name="routes">Routes.</param>
		/// <param name="htmlAttributes">Html attributes.</param>
		public static IHtmlString TranslatedActionLink (this HtmlHelper helper, 
			string actionLabel, string actionName, string controller, object routes = null, object htmlAttributes = null)  {
			if (controller == null) {
				string controllerName = helper.ViewContext.Controller.GetType ().Name;
				if (controllerName.EndsWith ("Controller"))
					controllerName = controllerName.Substring (0,controllerName.Length - 10);
				controller = controllerName;
			}
			if (htmlAttributes == null)
				htmlAttributes = defaultHtmlAttributes;
			IHtmlString text = T.Translate (helper, actionLabel);
			StringWriter strwr = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter(strwr);

			foreach (var ppt in htmlAttributes.GetType().GetRuntimeProperties()) {
				if (ppt.CanRead) {
					var name = ppt.Name;
					var val = ppt.GetValue (htmlAttributes).ToString ();
					writer.AddAttribute (name, val);
				}
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

		/// <summary>
		/// Creates a command from the http post request.
		/// This methods applies to one product reference,
		/// given in a required value "ref" in the given 
		/// collection of name value couples.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="files">Files.</param>
		private static CommandRegistration FromPost(this Command cmd, Dictionary<string,string>  collection, 
			HttpPostedFileBase [] files)
		{
			// string catref=collection["catref"]; // Catalog Url from which formdata has been built
			cmd.ProductRef = collection ["productref"];
			if (cmd.ProductRef == null)
				throw new InvalidOperationException (
					"A product reference cannot be blank at command time");
			cmd.ClientName = collection ["clientname"];
			if (cmd.ClientName == null)
				throw new InvalidOperationException (
					"A client name cannot be blank at command time");

			cmd.CreationDate = DateTime.Now;
			cmd.Status = CommandStatus.Inserted;
			// stores the parameters:
			cmd.SetParameters(collection);

			var registration = WorkFlowManager.RegisterCommand (cmd); // gives a value to this.Id
			UserFileSystemManager.Put(Path.Combine("commandes",cmd.Id.ToString ()),files);
			return registration;
		}

		/// <summary>
		/// Creates a command using the specified collection
		/// as command parameters, handles the files upload,
		/// ans register the command in db, positionning the 
		/// command id.
		/// 
		/// Required values in the command parameters : 
		///
		/// * ref: the product reference,
		/// * type: the command concrete class name.
		///
		/// </summary>
		/// <returns>The command.</returns>
		/// <param name="collection">Collection.</param>
		/// <param name="files">Files.</param>
		/// <param name="cmdreg">Cmdreg.</param>
		public static Command CreateCommand (
			Dictionary<string,string> collection,
			HttpPostedFileBase [] files, 
			out CommandRegistration cmdreg)
		{
			string cls = collection ["type"];
			if (cls == null)
				throw new InvalidOperationException (
					"A command type cannot be blank");
			var cmd = FrontOfficeHelpers.CreateCommand (cls);
			cmdreg = cmd.FromPost (collection, files);
			return cmd;
		}


		/// <summary>
		/// List the specified user.
		/// </summary>
		/// <param name="user">User.</param>
		public static IEnumerable<CircleBase> List(string user)
		{
			if (user == null)
				user = HttpContext.Current.User.Identity.Name;
			return CircleManager.DefaultProvider.List (user);
		}
		/// <summary>
		/// Circles the specified user and relation.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="relation">Relation.</param>
		public static string[] Circles(string relation, string user = null )
		{
			if (user == null)
				user = HttpContext.Current.User.Identity.Name;
			return CircleManager.DefaultProvider.Circles (user, relation);
		}
		/// <summary>
		/// Lists the available circles.
		/// </summary>
		/// <returns>The available circles.</returns>
		/// <param name="user">User.</param>
		public static string[] ListAvailableCircles (string user = null )
		{
			if (user == null)
				user = HttpContext.Current.User.Identity.Name;
			return CircleManager.DefaultProvider.List (user).Select (x => x.Title).ToArray();
		}



	}
}

