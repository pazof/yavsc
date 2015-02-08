using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Http;
using Yavsc.Model.WorkFlow;
using System.Collections.Specialized;

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
		[AcceptVerbs("GET")]
		public long Create(string productId , NameValueCollection cmdParams)
		{
			throw new NotImplementedException ();
		}

    }
}