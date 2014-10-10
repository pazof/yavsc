using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yavsc.Admin;


namespace Yavsc.Controllers
{
    public class BackOfficeController : Controller
    {
		[Authorize(Roles="Admin,Providers")]
		public ActionResult Index()
		{
			return View ();
		}
    }
}
