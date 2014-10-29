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
using System.Reflection;
using Yavsc.App_GlobalResources;
using System.Resources;

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

		public ActionResult ReferencedAssemblies()
		{
			AssemblyName[] model = GetType ().Assembly.GetReferencedAssemblies ();

			return View (model);
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
			string cn = CultureInfo.CurrentCulture.Name;
			ViewData ["Message"] = LocalizedText.Welcome;
			return View ();
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
