using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Pay pal controller.
	/// </summary>
    public class PayPalController : Controller
    {
		/// <summary>
		/// Index this instance.
		/// </summary>
        public ActionResult Index()
        {
            return View ();
        }
		/// <summary>
		/// Commit this instance.
		/// </summary>
		public ActionResult Commit()
		{
			return View ();
		}
		/// <summary>
		/// Abort this instance.
		/// </summary>
		public ActionResult Abort()
		{
			return View ();
		}
		/// <summary>
		/// IP this instance.
		/// </summary>
		public ActionResult IPN()
		{
			return View ();
		}
    }
}
