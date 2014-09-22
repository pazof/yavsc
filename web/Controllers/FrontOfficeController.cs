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

namespace Yavsc.Controllers
{
	public class FrontOfficeController : Controller
	{
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
				basket.Add(new Commande(collection));
				return View (collection);
			} catch (Exception e) {
				ViewData ["Message"] = "Exception:"+e.Message;
				return View (collection);
			}
		}

	}
}
