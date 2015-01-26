using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yavsc.Admin;


namespace Yavsc.Controllers
{
	/// <summary>
	/// Back office controller.
	/// </summary>
    public class BackOfficeController : Controller
    {
		/// <summary>
		/// Index this instance.
		/// </summary>
		[Authorize(Roles="Admin,Providers")]
		public ActionResult Index()
		{
			return View ();
		}
    }
}
