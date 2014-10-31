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
		class Error {}

		[Authorize]
		[AcceptVerbs("POST")]
		public object UpdateWritting([FromBody] Writting model)
		{
			if (!ModelState.IsValid) {
				return ModelState.Where ( k => k.Value.Errors.Count>0) ; 
			}
			WorkFlowManager.UpdateWritting (model);
			return null;
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
		/// <summary>
		/// Adds the specified imputation to the given estimation by estimation id.
		/// </summary>
		/// <param name="estid">Estimation identifier</param>
		/// <param name="wr">Imputation to add</param>
		public long Write ([FromUri] long estid, Writting wr) {
			return WorkFlowManager.Write(estid, wr.Description,
				wr.UnitaryCost, wr.Count, wr.ProductReference);
		}
    }
}
