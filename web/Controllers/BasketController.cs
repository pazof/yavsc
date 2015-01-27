using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Http;
using Yavsc.Model.WorkFlow;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Basket controller.
	/// </summary>
	public class BasketController : ApiController
    {
		/// <summary>
		/// The wfmgr.
		/// </summary>
		protected WorkFlowManager wfmgr = null;
		/// <summary>
		/// Initialize the specified controllerContext.
		/// </summary>
		/// <param name="controllerContext">Controller context.</param>
		protected override void Initialize (System.Web.Http.Controllers.HttpControllerContext controllerContext)
		{
			base.Initialize (controllerContext);
			wfmgr = new WorkFlowManager ();
		}
		/// <summary>
		/// Validates the order.
		/// 
		/// </summary>
		/// <returns><c>true</c>, if order was validated, <c>false</c> otherwise.</returns>
		/// <param name="orderid">Orderid.</param>
		bool ValidateOrder(long orderid) {
			throw new NotImplementedException ();
		}

		long CreateOrder(string title,string mesg)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Adds to basket, a product from the catalog, in the user's session.
		/// </summary>
		/// <returns>The to basket.</returns>
		[HttpGet]
		public long AddToOrder (long orderid, string prodref,int count, object prodparams=null)
		{
			//TODO find the basket for Membership.GetUser().UserName
			//return WFManager.Write(estid << from the basket, desc, ucost, count, productid);
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Yours the estimates.
		/// </summary>
		/// <returns>The estimates.</returns>
		[HttpGet]
		[Authorize]
		public Estimate[] YourEstimates()
		{
			return wfmgr.GetEstimates (
				Membership.GetUser().UserName);
		}
    }
}