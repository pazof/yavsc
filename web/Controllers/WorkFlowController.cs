using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Http;
using WorkFlowProvider;
using Yavsc.Model.WorkFlow;
using System.Web.Http.Controllers;
using System.Web.Security;

namespace Yavsc.ApiControllers
{
	[HttpControllerConfiguration(ActionValueBinder=typeof(Basic.MvcActionValueBinder))]
	public class WorkFlowController : ApiController
    {
		string adminRoleName="Admin";

		protected override void Initialize (HttpControllerContext controllerContext)
		{
			// TODO move it in a module initialization
			base.Initialize (controllerContext);
			if (!Roles.RoleExists (adminRoleName)) {
				Roles.CreateRole (adminRoleName);
			} 
		}

		[HttpGet]
		[Authorize]
		public long CreateEstimate (string title)
		{
			return WorkFlowManager.CreateEstimate (
				Membership.GetUser().UserName,title);
		}

		[HttpGet]
		[Authorize]
		public void DropWritting(long wrid)
		{
			WorkFlowManager.DropWritting (wrid);
		}
		[HttpGet]
		[Authorize]
		public void UpdateWritting(Writting wr)
		{
			WorkFlowManager.UpdateWritting (wr);
		}

		[HttpGet]
		[Authorize]
		public void DropEstimate(long estid)
		{
			WorkFlowManager.DropEstimate (estid);
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
		[Authorize]
		public long Write (long estid, string desc, decimal ucost, int count, long productid=0) {
			// TODO ensure estid owner matches the current one

			return WorkFlowManager.Write(estid, desc, ucost, count, productid);
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
