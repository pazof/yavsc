using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Http;
using Yavsc.Model.WorkFlow;
using System.Collections.Specialized;
using Yavsc.Model.FrontOffice;
using Yavsc.Helpers;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Basket controller.
	/// Maintains a collection of articles
	/// qualified with name value pairs
	/// </summary>
	[Authorize]
	public class BasketController : ApiController
    {

		/// <summary>
		/// Gets the current basket, creates a new one, if it doesn't exist.
		/// </summary>
		/// <value>The current basket.</value>
		protected CommandSet CurrentBasket {
			get {
				CommandSet b = WorkFlowManager.GetCommands (
					Membership.GetUser ().UserName);
				if (b == null) b = new CommandSet ();
				return b;
			}
		}
		/// <summary>
		/// Get the basket.
		/// </summary>
		public CommandSet Get() {
			return CurrentBasket;
		}

		/// <summary>
		/// Create the specified basket item using specified command parameters.
		/// </summary>
		public long Create()
		{
			return YavscHelpers.CreateCommandFromRequest ();
		}

		/// <summary>
		/// Read the specified basket item.
		/// </summary>
		/// <param name="itemid">Itemid.</param>
		Command Read(long itemid){
			return CurrentBasket[itemid];
		}

		/// <summary>
		/// Update the specified item parameter using the specified value.
		/// </summary>
		/// <param name="itemid">Item identifier.</param>
		/// <param name="param">Parameter name.</param>
		/// <param name="value">Value.</param>
		public void UpdateParam(long itemid, string param, string value)
		{
			CurrentBasket [itemid].Parameters [param] = value;
		}

		/// <summary>
		/// Delete the specified item.
		/// </summary>
		/// <param name="itemid">Item identifier.</param>
		public void Delete(long itemid)
		{
			CurrentBasket.Remove (itemid);
		}

    }
}