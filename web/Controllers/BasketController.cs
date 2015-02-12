using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Http;
using Yavsc.Model.WorkFlow;
using System.Collections.Specialized;
using Yavsc.Model.FrontOffice;

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
		/// Create the specified basket item using specified command parameters.
		/// </summary>
		/// <param name="cmdParams">Command parameters.</param>
		[AcceptVerbs("CREATE")]
		public long Create(NameValueCollection cmdParams)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Read the specified basket item.
		/// </summary>
		/// <param name="itemid">Itemid.</param>
		[AcceptVerbs("READ")]
		Commande Read(long itemid){
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Update the specified item parameter using the specified value.
		/// </summary>
		/// <param name="itemid">Item identifier.</param>
		/// <param name="param">Parameter name.</param>
		/// <param name="value">Value.</param>
		[AcceptVerbs("UPDATE")]
		public void Update(long itemid, string param, string value)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Delete the specified item.
		/// </summary>
		/// <param name="itemid">Item identifier.</param>
		public void Delete(long itemid)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Post a file, as attached document to the specified 
		/// Item
		/// </summary>
		[AcceptVerbs("POST")]
		public void Post(long itemId)
		{
			throw new NotImplementedException ();
		}
    }
}