using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Security;
using Yavsc;
using Yavsc.Model.WorkFlow;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Helpers;
using Yavsc.Model;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Work flow controller.
	/// </summary>
	public class WorkFlowController : ApiController
    {
		string adminRoleName="Admin";
		/// <summary>
		/// The wfmgr.
		/// </summary>
		protected WorkFlowManager wfmgr = null;
		/// <summary>
		/// Initialize the specified controllerContext.
		/// </summary>
		/// <param name="controllerContext">Controller context.</param>
		protected override void Initialize (HttpControllerContext controllerContext)
		{
			// TODO move it in a module initialization
			base.Initialize (controllerContext);
			if (!Roles.RoleExists (adminRoleName)) {
				Roles.CreateRole (adminRoleName);
			} 
			wfmgr = new WorkFlowManager ();
		}

		/// <summary>
		/// Creates the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="title">Title.</param>
		/// <param name="client">Client.</param>
		/// <param name="description">Description.</param>
		[HttpGet]
		[Authorize]
		public Estimate CreateEstimate (string title,string client,string description)
		{
			return wfmgr.CreateEstimate (
				Membership.GetUser().UserName,client,title,description);
		}

		/// <summary>
		/// Register the specified model and isapprouved.
		/// </summary>
		/// <param name="model">Model.</param>
		/// <param name="isapprouved">If set to <c>true</c> isapprouved.</param>
		[HttpGet]
		[ValidateAjax]
		[Authorize(Roles="Admin,FrontOffice")]
		public void Register([FromBody] RegisterModel model)
		{
			if (ModelState.IsValid) {
				MembershipCreateStatus mcs;
				var user = Membership.CreateUser (
					model.UserName,
					model.Password,
					model.Email,
					null,
					null,
					model.IsApprouved,
					out mcs);
				switch (mcs) {
				case MembershipCreateStatus.DuplicateEmail:
					ModelState.AddModelError ("Email", 
						string.Format(LocalizedText.DuplicateEmail,model.UserName) );
					return ;
				case MembershipCreateStatus.DuplicateUserName:
					ModelState.AddModelError ("UserName", 
						string.Format(LocalizedText.DuplicateUserName,model.Email));
					return ;
				case MembershipCreateStatus.Success:
					if (!model.IsApprouved)
						YavscHelpers.SendActivationEmail (user);
					return;
				default:
					throw new InvalidOperationException (string.Format("Unexpected user creation code :{0}",mcs));
				}
			}
		}

		/// <summary>
		/// Drops the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		[HttpGet]
		[Authorize]
		public void DropWritting(long wrid)
		{
			wfmgr.DropWritting (wrid);
		}

		/// <summary>
		/// Drops the estimate.
		/// </summary>
		/// <param name="estid">Estid.</param>
		[HttpGet]
		[Authorize]
		public void DropEstimate(long estid)
		{
			string username = Membership.GetUser().UserName;
			Estimate e = wfmgr.GetEstimate (estid);
			if (e == null)
				throw new InvalidOperationException("not an estimate id:"+estid);
			if (username != e.Responsible
				&& !Roles.IsUserInRole ("FrontOffice"))
				throw new UnauthorizedAccessException ("You're not allowed to drop this estimate");

			wfmgr.DropEstimate (estid);
		}

		/// <summary>
		/// Index this instance.
		/// </summary>
		[HttpGet]
		[Authorize]
		public object Index()
		{
			// TODO inform user on its roles and alerts
			string username = Membership.GetUser ().UserName;
			return new { test=string.Format("Hello {0}!",username) }; 
        }

		private HttpResponseMessage CreateModelStateErrorResponse () {
			// strip exceptions
			Dictionary<string,string[]> errs = new Dictionary<string, string[]> ();

			foreach (KeyValuePair<string,ModelState> st
				in ModelState.Where (x => x.Value.Errors.Count > 0))
				errs.Add(st.Key, st.Value.Errors.Select(x=>x.ErrorMessage).ToArray());

			return Request.CreateResponse(System.Net.HttpStatusCode.BadRequest,
				errs);
		}

		/// <summary>
		/// Updates the writting.
		/// </summary>
		/// <returns>The writting.</returns>
		/// <param name="wr">Wr.</param>
		[Authorize]
		[AcceptVerbs("POST")]
		[ValidateAjax]
		public HttpResponseMessage UpdateWritting([FromBody] Writting wr)
		{
			wfmgr.UpdateWritting (wr);
			return Request.CreateResponse<string> (System.Net.HttpStatusCode.OK,"WrittingUpdated:"+wr.Id);
		}

		/// <summary>
		/// Adds the specified imputation to the given estimation by estimation id.
		/// </summary>
		/// <param name="estid">Estimation identifier</param>
		/// <param name="wr">Imputation to add</param>
		[AcceptVerbs("POST")]
		[Authorize]
		[ValidateAjax]
		public HttpResponseMessage Write ([FromUri] long estid, [FromBody] Writting wr) {
			if (estid <= 0) {
				ModelState.AddModelError ("EstimationId", "Spécifier un identifiant d'estimation valide");
				return Request.CreateResponse (System.Net.HttpStatusCode.BadRequest,
					ValidateAjaxAttribute.GetErrorModelObject (ModelState));
			}
			try {
				return Request.CreateResponse(System.Net.HttpStatusCode.OK,
					wfmgr.Write(estid, wr.Description,
						wr.UnitaryCost, wr.Count, wr.ProductReference));
			}
			catch (Exception ex) {
				return Request.CreateResponse (
					System.Net.HttpStatusCode.InternalServerError,
					"Internal server error:" + ex.Message + "\n" + ex.StackTrace);

			}
		}
    }


}
