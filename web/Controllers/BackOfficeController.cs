using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yavsc.Admin;
using Yavsc.Model.Calendar;
using Yavsc.Model.Circles;
using System.Web.Security;
using Yavsc.Model.Google.Api;


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

		/// <summary>
		/// Notifies the event.
		/// </summary>
		/// <returns>The event.</returns>
		/// <param name="evpub">Evpub.</param>
		public ActionResult NotifyEvent(EventPub evpub)
		{
			if (ModelState.IsValid) {
				ViewData ["NotifyEvent"] = evpub;
				return View ("NotifyEventResponse", GoogleHelpers.NotifyEvent (evpub));
			}

			ViewData["CircleIds"] = CircleManager.DefaultProvider.List (
				Membership.GetUser ().UserName).Select (x => new SelectListItem {
					Value = x.Id.ToString(),
					Text = x.Title,
					Selected = (evpub.CircleIds==null) ? false : evpub.CircleIds.Contains (x.Id)
				});
			return View (evpub);
		}
    }
}
