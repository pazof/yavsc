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
using Npgsql.Web;
using ITContentProvider;
using WorkFlowProvider;
using Npgsql.Web.Blog;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Home controller.
	/// </summary>
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
		/// <summary>
		/// Lists the referenced assemblies.
		/// </summary>
		/// <returns>The info.</returns>
		public ActionResult AssemblyInfo()
		{
			Assembly[] aslist = {
				GetType ().Assembly,
				typeof(ITCPNpgsqlProvider).Assembly,
				typeof(NpgsqlMembershipProvider).Assembly,
				typeof(NpgsqlContentProvider).Assembly,
				typeof(NpgsqlBlogProvider).Assembly
			};

			List <AssemblyName> asnlist = new List<AssemblyName> ();
			foreach (Assembly asse in aslist) {
				foreach (AssemblyName an in asse.GetReferencedAssemblies ()) {
					if (asnlist.All(x=> string.Compare(x.Name,an.Name)!=0))
						asnlist.Add (an);
				}
			}
			asnlist.Sort (delegate(AssemblyName x, AssemblyName y) {
				return string.Compare (x.Name, y.Name);
			});
			return View (asnlist.ToArray()) ;
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
		/// <summary>
		/// Index this instance.
		/// </summary>
		public ActionResult Index ()
		{
			string startPage = WebConfigurationManager.AppSettings ["StartPage"];
			if (startPage != null)
				Redirect (startPage);
			ViewData ["Message"] = LocalizedText.Welcome;
			return View ();
		}
		/// <summary>
		/// Contact the specified email, reason and body.
		/// </summary>
		/// <param name="email">Email.</param>
		/// <param name="reason">Reason.</param>
		/// <param name="body">Body.</param>
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

					return View (new { email=email, reason="", body="" });
				}
			}
		}

	}
}
