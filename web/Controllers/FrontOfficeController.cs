using System;
using Yavsc;
using System.Web.Mvc;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using Yavsc.Controllers;
using System.Collections.Generic;
using Yavsc.Model;
using Yavsc.Model.WorkFlow;
using WorkFlowProvider;
using System.Web.Security;
using System.Threading;
using Yavsc.Model.FrontOffice;
using Yavsc.Model.FileSystem;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Front office controller.
	/// Access granted to all
	/// </summary>
	public class FrontOfficeController : Controller
	{
		/// <summary>
		/// The wfmgr.
		/// </summary>
		protected WorkFlowManager wfmgr = null;

		/// <summary>
		/// Initialize the specified requestContext.
		/// </summary>
		/// <param name="requestContext">Request context.</param>
		protected override void Initialize (System.Web.Routing.RequestContext requestContext)
		{
			base.Initialize (requestContext);
			wfmgr = new WorkFlowManager ();
		}

		/// <summary>
		/// Index this instance.
		/// </summary>
		public ActionResult Index ()
		{
			return View ();
		}

		/// <summary>
		/// Estimates this instance.
		/// </summary>
		[Authorize]
		public ActionResult Estimates ()
		{
			string username = Membership.GetUser ().UserName;

			return View (wfmgr.GetEstimates (username));
		}

		/// <summary>
		/// Estimate the specified model and submit.
		/// </summary>
		/// <param name="model">Model.</param>
		/// <param name="submit">Submit.</param>
		[Authorize]
		public ActionResult Estimate (Estimate model, string submit)
		{
			ViewData ["WebApiBase"] = "http://" + Request.Url.Authority + "/api";
			ViewData ["WABASEWF"] = ViewData ["WebApiBase"] + "/WorkFlow";
			if (submit == null) {
				if (model.Id > 0) {
					Estimate f = wfmgr.GetEstimate (model.Id);
					if (f == null) {
						ModelState.AddModelError ("Id", "Wrong Id");
						return View (model);
					}
					model = f;
					ModelState.Clear ();
					string username = HttpContext.User.Identity.Name;
					if (username != model.Responsible
					    && username != model.Client
					    && !Roles.IsUserInRole ("FrontOffice"))
						throw new UnauthorizedAccessException ("You're not allowed to view this estimate");
				} 
			} else {
				string username = HttpContext.User.Identity.Name;
				if (model.Id == 0) {
					model.Responsible = username;
					ModelState.Clear ();
				}
				if (ModelState.IsValid) {
					if (username != model.Responsible
					    && !Roles.IsUserInRole ("FrontOffice"))
						throw new UnauthorizedAccessException ("You're not allowed to modify this estimate");

					if (model.Id == 0)
						model = wfmgr.CreateEstimate (
							username,
							model.Client, model.Title, model.Description);
					else {
						wfmgr.UpdateEstimate (model);
						model = wfmgr.GetEstimate (model.Id);
					}
				}
			}
			return View (model);
		}

		/// <summary>
		/// Catalog this instance.
		/// </summary>
		[AcceptVerbs ("GET")]
		public ActionResult Catalog ()
		{
			return View (
				CatalogManager.GetCatalog (Request.Url.AbsolutePath)
			);
		}

		/// <summary>
		/// Catalog this instance.
		/// </summary>
		[AcceptVerbs ("GET")]
		public ActionResult Brand (string id)
		{
			Catalog c = CatalogManager.GetCatalog (Request.Url.AbsolutePath);
			ViewData ["BrandName"] = id;
			return View (c.GetBrand (id));
		}

		/// <summary>
		/// get the product category
		/// </summary>
		/// <returns>The category object.</returns>
		/// <param name="brandid">Brand id.</param>
		/// <param name="pcid">Product category Id.</param>
		[AcceptVerbs ("GET")]
		public ActionResult ProductCategory (string brandid, string pcid)
		{
			ViewData ["BrandId"] = brandid;
			ViewData ["ProductCategoryId"] = pcid;
			return View (
				CatalogManager.GetCatalog (Request.Url.AbsolutePath).GetBrand (brandid).GetProductCategory (pcid)
			);
		}

		/// <summary>
		/// Product the specified id, pc and pref.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="pc">Pc.</param>
		/// <param name="pref">Preference.</param>
		[AcceptVerbs ("GET")]
		public ActionResult Product (string id, string pc, string pref)
		{
			Product p = null;
			ViewData ["BrandName"] = id;
			ViewData ["ProdCatRef"] = pc;
			ViewData ["ProdRef"] = pref;
			Catalog cat = CatalogManager.GetCatalog (Request.Url.AbsolutePath);
			if (cat == null) {
				ViewData ["Message"] = "Catalog introuvable";
				ViewData ["RefType"] = "Catalog";
				return View ("ReferenceNotFound");
			}
			Brand b = cat.GetBrand (id);
			if (b == null) {
				ViewData ["RefType"] = "Brand";
				return View ("ReferenceNotFound");
			}
			ProductCategory pcat =	b.GetProductCategory (pc);
			if (pcat == null) {
				ViewData ["RefType"] = "ProductCategory";
				return View ("ReferenceNotFound");
			}
			ViewData ["ProdCatName"] = pcat.Name;
			p = pcat.GetProduct (pref);
			if (p.CommandForm == null)
				p.CommandForm = b.DefaultForm;

			return View ((p is Service) ? "Service" : "Product", p);
		}

		/// <summary>
		/// Basket this instance.
		/// </summary>
		[Authorize]
		public ActionResult Basket ()
		{
			return View (wfmgr.GetCommands (Membership.GetUser ().UserName));
		}

		/// <summary>
		/// Command the specified collection.
		/// </summary>
		/// <param name="collection">Collection.</param>
		[HttpPost]
		[Authorize]
		public ActionResult Command (FormCollection collection)
		{
			try {
				// Add specified product command to the basket,
				// saves it in db
				new Command(collection,HttpContext.Request.Files);
				ViewData ["Message"] = LocalizedText.Item_added_to_basket;
				return View (collection);
			} catch (Exception e) {
				ViewData ["Message"] = "Exception:" + e.Message;
				return View (collection);
			}
		}

	}
}
