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
using System.Resources;
using Yavsc.Model;

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

		public ActionResult AssemblyInfo()
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
			string startPage = WebConfigurationManager.AppSettings ["StartPage"];
			if (startPage != null)
				Redirect (startPage);
			ViewData ["Message"] = LocalizedText.Welcome;
			return View ();
		}

		public ActionResult Contact (string email, string reason, string body)
		{
			if (email==null)
				ModelState.AddModelError("email","Enter your email");

			if (reason==null)
				ModelState.AddModelError("reason","Please, fill in a reason");

			if (body==null)
				ModelState.AddModelError("body","Please, fill in a body");
			if (!ModelState.IsValid)
				return View ();

			// requires valid owner and admin email?
			if (OwnerEmail == null)
				throw new Exception ("No site owner!");

			using (System.Net.Mail.MailMessage msg = new MailMessage(email,OwnerEmail,"[Contact] "+reason,body)) 
			{
				msg.CC.Add(new MailAddress(Admail));
				using (System.Net.Mail.SmtpClient sc = new SmtpClient()) 
				{
					sc.Send (msg);
					ViewData ["Message"] = LocalizedText.Message_sent;
					return View (new { reason="", body="" });
				}
			}
		}

	}
}
