using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Http;
using Yavsc.Model.WorkFlow;
using System.Collections.Specialized;
using Yavsc.Model.FrontOffice;
using System.Web.SessionState;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Basket controller.
	/// Maintains a collection of articles
	/// qualified with name value pairs
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
		/// Gets the current basket, creates a new one, if it doesn't exist.
		/// </summary>
		/// <value>The current basket.</value>
		protected Basket CurrentBasket {
			get {
				HttpSessionState session = HttpContext.Current.Session;
				Basket b = (Basket) session ["Basket"];
				if (b == null)
					session ["Basket"] = b = new Basket ();
				return b;
			}
		}

		/// <summary>
		/// Create the specified basket item using specified command parameters.
		/// </summary>
		/// <param name="cmdParams">Command parameters.</param>
		[Authorize]
		public long Create(NameValueCollection cmdParams)
		{
			// HttpContext.Current.Request.Files
			Commande cmd = new Commande(cmdParams, HttpContext.Current.Request.Files);
			CurrentBasket.Add (cmd);
			return cmd.Id;
		}

		/// <summary>
		/// Read the specified basket item.
		/// </summary>
		/// <param name="itemid">Itemid.</param>
		[Authorize]
		Commande Read(long itemid){
			return CurrentBasket[itemid];
		}

		/// <summary>
		/// Update the specified item parameter using the specified value.
		/// </summary>
		/// <param name="itemid">Item identifier.</param>
		/// <param name="param">Parameter name.</param>
		/// <param name="value">Value.</param>
		[Authorize]
		public void UpdateParam(long itemid, string param, string value)
		{
			CurrentBasket [itemid].Parameters [param] = value;
		}

		/// <summary>
		/// Delete the specified item.
		/// </summary>
		/// <param name="itemid">Item identifier.</param>
		[Authorize]
		public void Delete(long itemid)
		{
			CurrentBasket.Remove (itemid);
		}

    }
}