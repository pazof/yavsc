using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Http;
using WorkFlowProvider;
using yavscModel.WorkFlow;
using System.Web.Http.Controllers;
using System.Web.Security;

namespace Yavsc.ApiControllers
{
	[HttpControllerConfiguration(ActionValueBinder=typeof(Basic.MvcActionValueBinder))]
	public class WorkFlowController : ApiController
    {
		[HttpGet]
		[Authorize]
		public long CreateEstimate (string title)
		{
			return WFManager.CreateEstimate (
				Membership.GetUser().UserName,title);
		}
		[HttpGet]
		[Authorize]
		public void DropWritting(long wrid)
		{
			WFManager.DropWritting (wrid);
		}
		[HttpGet]
		[Authorize]
		public void UpdateWritting(Writting wr)
		{
			WFManager.UpdateWritting (wr);
		}

		[HttpGet]
		[Authorize]
		public void DropEstimate(long estid)
		{
			WFManager.DropEstimate (estid);
		}
		[HttpGet]
		[Authorize]
		public object Index()
        {
			// TODO inform user on its roles and alerts
			string username = Membership.GetUser ().UserName;
			return new { test=string.Format("Hello {0}!",username) }; 
        }
	
		[HttpGet]
		public object Order (BasketImpact bi)
		{
			return new { c="lmk,", message="Panier impacté", impactRef=bi.ProductRef, count=bi.count};
		}

		[HttpGet]
		[Authorize]
		public long Write (long estid, string desc, decimal ucost, int count, long productid=0) {
			// TODO ensure estid owner matches the current one

			return WFManager.Write(estid, desc, ucost, count, productid);
		}

		[Authorize]
		[HttpGet]
		/// <summary>
		/// Gets the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="estid">Estid.</param>
		public Estimate GetEstimate (long estid)
		{
			Estimate est = WFManager.ContentProvider.GetEstimate (estid);
			return est;
		}
		/*
	public object Details(int id)
        {
			throw new NotImplementedException ();
        }

		public object Create()
        {
			throw new NotImplementedException ();
        } 

		public object Edit(int id)
        {
			throw new NotImplementedException ();
        }

		public object Delete(int id)
        {
			throw new NotImplementedException ();
        }

		IContentProvider contentProvider = null;
		IContentProvider ContentProvider {
			get {
				if (contentProvider == null )
					contentProvider = WFManager.GetContentProviderFWC ();
				return contentProvider;
			}
		}

*/
    }
}
