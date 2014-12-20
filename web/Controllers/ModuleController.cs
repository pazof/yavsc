using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yavsc.Model;
using System.Configuration;

namespace Yavsc.Controllers
{
    public class ModuleController : Controller
    {
		protected override void Initialize (System.Web.Routing.RequestContext requestContext)
		{
			base.Initialize (requestContext);
			ConfigurationManager.GetSection ("ymodules");

		}

		// List<IModule> modules = new List<IModule> ();

        public ActionResult Index()
        {
            return View ();
        }
    }
}
