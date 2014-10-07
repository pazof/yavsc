using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Yavsc;

namespace Yavsc.Controllers
{
	public class HomeController : Controller
	{
		// Site name
		private static string name = null;
			
		/// <summary>
		/// Gets or sets the site name.
		/// </summary>
		/// <value>The name.</value>
		[Obsolete("Use YavscHelpers.SiteName insteed.")]
		public static string Name {
			get {
				if (name == null) 
					name = WebConfigurationManager.AppSettings ["Name"];
				return name;
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


		private static string owneremail = null;
		/// <summary>
		/// Gets or sets the owner email.
		/// </summary>
		/// <value>The owner email.</value>
		public static string OwnerEmail {
			get {
				if (owneremail == null)
					owneremail = WebConfigurationManager.AppSettings.Get ("OwnerEMail");
				return owneremail;
			}
			set {
				owneremail = value;
			}
		}

		public ActionResult Index ()
		{
			InitCulture ();
			ViewData ["Message"] = string.Format(T.GetString("Welcome")+"({0})",GetType ().Assembly.FullName);
				return View ();
		}

		public void InitCulture() { 
			CultureInfo culture = null;
			string defaultCulture = "fr";
			if (Request.UserLanguages==null)
				culture = CultureInfo.CreateSpecificCulture(defaultCulture);
			else
			if (Request.UserLanguages.Length > 0) {
				try {
				culture = new CultureInfo (Request.UserLanguages [0]);
				}
				catch (Exception e) {
					ViewData ["Message"] = e.ToString ();
					culture = CultureInfo.CreateSpecificCulture(defaultCulture);      
				}
			}
			else culture = CultureInfo.CreateSpecificCulture(defaultCulture);      
			System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			string lcd = Server.MapPath ("./locale");
			Mono.Unix.Catalog.Init("i8n1", lcd );
		}

		public ActionResult AOEMail (string reason, string body)
		{
			// requires valid owner and admin email?
			using (System.Net.Mail.MailMessage msg = new MailMessage(owneremail,admail,"Poke : "+reason,body)) 
			{
				using (System.Net.Mail.SmtpClient sc = new SmtpClient()) 
				{
					sc.Send (msg);
					return View ();
				}
			}
		}
	}
}
