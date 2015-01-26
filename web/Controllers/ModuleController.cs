using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yavsc.Model;
using System.Configuration;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Module controller.
	/// </summary>
    public class ModuleController : Controller
    {
		/// <summary>
		/// Initialize the specified requestContext.
		/// </summary>
		/// <param name="requestContext">Request context.</param>
		protected override void Initialize (System.Web.Routing.RequestContext requestContext)
		{
			base.Initialize (requestContext);
			ConfigurationManager.GetSection ("ymodules");

		}

		// List<IModule> modules = new List<IModule> ();
		/// <summary>
		/// Index this instance.
		/// </summary>
        public ActionResult Index()
        {
            return View ();
        }
    }
}
