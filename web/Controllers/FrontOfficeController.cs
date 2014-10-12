using System;
using Yavsc;
using SalesCatalog;
using SalesCatalog.Model;
using System.Web.Mvc;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using Yavsc.Controllers;
using System.Collections.Generic;
using Yavsc.Model.WorkFlow;
using WorkFlowProvider;
using System.Web.Security;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Front office controller.
	/// Access granted to all
	/// </summary>
	public class FrontOfficeController : Controller
	{
		[Authorize]
		public ActionResult Estimate(Estimate model,string submit) 
		{
			if (ModelState.IsValid) {
				string username = HttpContext.User.Identity.Name;
				if (model.Id > 0) {
					Estimate f = WorkFlowManager.GetEstimate (model.Id);
					if (f == null) {
						ModelState.AddModelError ("Id", "Wrong Id");
						return View (model);
					}

					if (username != f.Owner)
					if (!Roles.IsUserInRole ("FrontOffice"))
						throw new UnauthorizedAccessException ("You're not allowed to view/modify this estimate");
					if (submit == "Update") {
						if (model != f) {
							WorkFlowManager.SetTitle (model.Id, model.Title);
						}
					} else if (submit == null) {
						model = f;
					}

				} else if (model.Id == 0 && submit=="Create") {
					// Create the estimate
					model.Id=WorkFlowManager.CreateEstimate (username,
						model.Title);
					model.Owner = username;
				}
			}
			return View(model);
		}

		[AcceptVerbs("GET")]
		public ActionResult Catalog ()
		{
			return View (
				CatalogManager.GetCatalog ()
			);
		}
		/// <summary>
		/// Catalog this instance.
		/// </summary>
		[AcceptVerbs("GET")]
		public ActionResult Brand (string id)
		{
			Catalog c = CatalogManager.GetCatalog ();
			ViewData ["BrandName"] = id;
			return View ( c.GetBrand (id) );
		}

		/// <summary>
		/// get the product category
		/// </summary>
		/// <returns>The category.</returns>
		/// <param name="bn">Bn.</param>
		/// <param name="pc">Pc.</param>
		[AcceptVerbs("GET")]
		public ActionResult ProductCategory (string id, string pc)
		{
			ViewData ["BrandName"] = id;
			return View (
				CatalogManager.GetCatalog ().GetBrand (id).GetProductCategory (pc)
			);
		}

		[AcceptVerbs("GET")]
		public ActionResult Product (string id, string pc, string pref)
		{
			Product p = null;
			ViewData ["BrandName"] = id;
			ViewData ["ProdCatRef"] = pc;
			ViewData ["ProdRef"] = pref;
			Catalog cat = CatalogManager.GetCatalog ();
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
			if (p.CommandForm==null)
				p.CommandForm = b.DefaultForm;

			return View ((p is Service)?"Service":"Product", p);

		}

		public ActionResult Command()
		{
			return View ();
		}

		[HttpPost]
		[Authorize]
		public ActionResult Command(FormCollection collection)
		{
			try {
				// get files from the request 
				string fnre = "[A-Za-z0-9~\\-.]+";
				HttpFileCollectionBase hfc = Request.Files;

				foreach (String h in hfc.AllKeys)
				{
					if (!Regex.Match(hfc[h].FileName,fnre).Success)
					{
						ViewData ["Message"] = "File name refused";
						ModelState.AddModelError(
							h,
							string.Format(
								"The file name {0} dosn't match an acceptable file name {1}",
								hfc[h].FileName,fnre))
						;
						return View(collection);
					}
				}
				foreach (String h in hfc.AllKeys)
				{
					// TODO Limit with hfc[h].ContentLength 
					hfc[h].SaveAs(Path.Combine(FileSystemController.BaseDir,hfc[h].FileName));
				}
				if (Session["Basket"]==null)
					Session["Basket"]=new List<Commande>();
				List<Commande> basket = Session["Basket"] as List<Commande>;
				// Add specified product command to the basket,
				basket.Add(new Commande(0,0,collection));
				return View (collection);
			} catch (Exception e) {
				ViewData ["Message"] = "Exception:"+e.Message;
				return View (collection);
			}
		}

	}
}

