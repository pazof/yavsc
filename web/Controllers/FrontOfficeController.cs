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
using System.Web.Security;
using System.Threading;
using Yavsc.Model.FrontOffice;
using Yavsc.Model.FileSystem;
using Yavsc.Model.Calendar;
using System.Configuration;
using Yavsc.Helpers;
using Yavsc.Model.FrontOffice.Catalog;
using Yavsc.Model.Skill;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Front office controller.
	/// Access granted to all
	/// </summary>
	public class FrontOfficeController : Controller
	{
		/// <summary>
		/// Index this instance.
		/// </summary>
		public ActionResult Index ()
		{
			return View ();
		}
		/// <summary>
		/// Pub the Event
		/// </summary>
		/// <returns>The pub.</returns>
		/// <param name="model">Model.</param>
		public ActionResult EventPub (EventPub model)
		{
			return View (model);
		}
		/// <summary>
		/// Estimates released to this client
		/// </summary>
		[Authorize]
		public ActionResult Estimates (string client)
		{
			var u = Membership.GetUser ();
			if (u == null) // There was no redirection to any login page
				throw new ConfigurationErrorsException ("no redirection to any login page");
			
			string username = u.UserName;
			Estimate [] estims = WorkFlowManager.GetUserEstimates (username);
			ViewData ["UserName"] = username;
			ViewData ["ResponsibleCount"] = 
				Array.FindAll (
				estims,
				x => x.Responsible == username).Length;
			ViewData ["ClientCount"] = 
				Array.FindAll (
					estims,
					x => x.Client == username).Length;
			return View (estims);
		}


		/// <summary>
		/// Estimate wearing the specified id.
		/// </summary>
		/// <param name="id">Estim Identifier.</param>
		public ActionResult Get (long id)
		{
			Estimate f = WorkFlowManager.GetEstimate (id);
			if (f == null) {
				ModelState.AddModelError ("Id", "Wrong Id");
				return View (new Estimate ()  { Id=id } );
			}
			return View (f);
		}

		/// <summary>
		/// Estimate the specified model and submit.
		/// </summary>
		/// <param name="model">Model.</param>
		/// <param name="submit">Submit.</param>
		[Authorize]
		public ActionResult Estimate (Estimate model, string submit)
		{
			string username = Membership.GetUser().UserName;
			// Obsolete, set in master page
			ViewData ["WebApiBase"] = Url.Content(Yavsc.WebApiConfig.UrlPrefixRelative);
			ViewData ["WABASEWF"] = ViewData ["WebApiBase"] + "/WorkFlow";
			if (submit == null) {
				if (model.Id > 0) {
					Estimate f = WorkFlowManager.GetEstimate (model.Id);
					if (f == null) {
						ModelState.AddModelError ("Id", "Wrong Id");
						return View (model);
					}
					model = f;
					ModelState.Clear ();
					if (username != model.Responsible
					    && username != model.Client
					    && !Roles.IsUserInRole ("FrontOffice"))
						throw new UnauthorizedAccessException ("You're not allowed to view this estimate");
				} else if (model.Id == 0) {
					if (string.IsNullOrWhiteSpace(model.Responsible))
						model.Responsible = username;
				}
			} else {

				if (model.Id == 0) // if (submit == "Create")
				if (string.IsNullOrWhiteSpace (model.Responsible))
					model.Responsible = username;
				if (username != model.Responsible
				&& !Roles.IsUserInRole ("FrontOffice"))
					throw new UnauthorizedAccessException ("You're not allowed to modify this estimate");

				if (ModelState.IsValid) {
					if (model.Id == 0)
						model = WorkFlowManager.CreateEstimate (
							username,
							model.Client, model.Title, model.Description);
					else {
						WorkFlowManager.UpdateEstimate (model);
						model = WorkFlowManager.GetEstimate (model.Id);
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
				CatalogManager.GetCatalog ()
			);
		}

		/// <summary>
		/// Catalog this instance.
		/// </summary>
		[AcceptVerbs ("GET")]
		public ActionResult Brand (string brandid)
		{
			Catalog c = CatalogManager.GetCatalog ();
			ViewData ["BrandName"] = brandid;
			return View (c.GetBrand (brandid));
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

			var cat = CatalogManager.GetCatalog ();
			if (cat == null)
				throw new Exception ("No catalog");
			var brand = cat.GetBrand (brandid);
			if (brand == null)
				throw new Exception ("Not a brand id: "+brandid);
			var pcat = brand.GetProductCategory (pcid);
			if (pcat == null)
				throw new Exception ("Not a product category id in this brand: " + pcid);
			return View (pcat);
		}

		/// <summary>
		/// Product the specified id, pc and pref.
		/// </summary>
		/// <param name="id">Brand Identifier.</param>
		/// <param name="pc">Production catalog reference.</param>
		/// <param name="pref">Preference.</param>
		[AcceptVerbs ("GET")]
		public ActionResult Product (string id, string pc, string pref)
		{
			Product p = null;
			ViewData ["BrandName"] = id;
			ViewData ["ProdCatRef"] = pc;
			ViewData ["ProdRef"] = pref;
			Catalog cat = CatalogManager.GetCatalog ();
			if (cat == null) {
				YavscHelpers.Notify(ViewData, "Catalog introuvable");
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
			return View (WorkFlowManager.GetCommands (Membership.GetUser ().UserName));
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
				YavscHelpers.Notify(ViewData, LocalizedText.Item_added_to_basket);
				return View (collection);
			} catch (Exception e) {
				YavscHelpers.Notify(ViewData,"Exception:" + e.Message);
				return View (collection);
			}
		}

		/// <summary>
		/// Booking the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		public ActionResult EavyBooking (BookingQuery model)
		{
			return View (model);
		}

		/// <summary>
		/// Skills the specified model.
		/// </summary>
		[Authorize(Roles="Admin")]
		public ActionResult Skills (string search)
		{
			if (search == null)
				search = "%";
			var skills = SkillManager.FindSkill (search);
			return View (skills);
		}

		/// <summary>
		/// Activities the specified search and toPower.
		/// </summary>
		/// <param name="search">Search.</param>
		/// <param name="toPower">If set to <c>true</c> to power.</param>
		public ActionResult Activities (string search, bool toPower = false)
		{
			if (search == null)
				search = "%";
			var activities = WorkFlowManager.FindActivity(search,!toPower);
			return View (activities);
		}

		/// <summary>
		/// Activity at the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public ActionResult Activity(string id)
		{
			return View(WorkFlowManager.GetActivity (id));
		}
		/// <summary>
		/// Display and should
		/// offer Ajax edition of 
		/// user's skills.
		/// </summary>
		/// <param name="id">the User name.</param>
		[Authorize()]
		public ActionResult UserSkills (string id)
		{
			if (id == null) 
				id = User.Identity.Name ;
			// TODO or not to do, handle a skills profile update,
			//      actually performed via the Web API :-°
			// else if (ModelState.IsValid) {}
			var usp = SkillManager.GetUserSkills (id);
			var skills = SkillManager.FindSkill ("%");
			ViewData ["SiteSkills"] = skills;
			return View (usp);
		}

		/// <summary>
		/// Booking the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		public ActionResult Booking (string id, SimpleBookingQuery model)
		{
			if (model.Needs == null)
				model.Needs = SkillManager.FindSkill ("%");
			model.MAECode = id;
			return View (model);
		}
	}
}
