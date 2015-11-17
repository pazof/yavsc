using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Reflection;
using System.Resources;
using Yavsc.Model;
using Npgsql.Web;
using Npgsql.Web.Blog;
using Yavsc.Helpers;
using Yavsc;
using System.Web.Mvc;
using Yavsc.Model.Blogs;

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
			foreach (string tagname in new string[] {"Accueil","Yavsc","Événements","Mentions légales"})
			{
				TagInfo ti = BlogManager.GetTagInfo (tagname);
				// TODO specialyze BlogEntry creating a PhotoEntry 
				ViewData [tagname] = ti;
			}
			return View ();
		}
		/// <summary>
		/// Credits this instance.
		/// </summary>
		public ActionResult Credits ()
		{
			return View ();
		}

		public ActionResult RestrictedArea ()
		{
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
				msg.CC.Add(new MailAddress(YavscHelpers.Admail));
				using (System.Net.Mail.SmtpClient sc = new SmtpClient()) 
				{
					sc.Send (msg);
					YavscHelpers.Notify(ViewData, LocalizedText.Message_sent);
					return View (new { email=email, reason="", body="" });
				}
			}
		}

	}
}
