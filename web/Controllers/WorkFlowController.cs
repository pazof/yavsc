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
		public Estimate CreateEstimate (string title,string client,string description)
		{
			return WorkFlowManager.CreateEstimate (
				Membership.GetUser().UserName,client,title,description);
		}

		[HttpGet]
		[Authorize]
		public void DropWritting(long wrid)
		{
			WorkFlowManager.DropWritting (wrid);
		}

		[Authorize]
		[AcceptVerbs("POST")]
		public void UpdateWritting([FromBody] Writting wr)
		{
			if (!ModelState.IsValid)
				throw new Exception ("Modèle invalide");
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


		[AcceptVerbs("POST")]
		[Authorize]
		public long Write ([FromUri] long estid, [FromBody] Writting wr) {
			// TODO ensure estid owner matches the current one
			return WorkFlowManager.Write(estid, wr.Description,
				wr.UnitaryCost, wr.Count, wr.ProductReference);
		}
    }
}
