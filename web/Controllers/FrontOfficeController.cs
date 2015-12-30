using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Profile;
using System.Web.Security;
using Yavsc;
using Yavsc.Controllers;
using Yavsc.Helpers;
using Yavsc.Model;
using Yavsc.Model.Calendar;
using Yavsc.Model.Circles;
using Yavsc.Model.FileSystem;
using Yavsc.Model.FrontOffice;
using Yavsc.Model.FrontOffice.Catalog;
using Yavsc.Model.Google.Api;
using Yavsc.Model.Skill;
using Yavsc.Model.WorkFlow;

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

		public ActionResult AskForAnEstimate()
		{
			return View();
		}
		
		/// <summary>
		/// Pub the Event
		/// </summary>
		/// <returns>The pub.</returns>
		/// <param name="model">Model.</param>
		public ActionResult EventPub (EventCirclesPub model)
		{
			return View (model);
		}

		/// <summary>
		/// Estimates released to this client
		/// </summary>
		[Authorize]
		public ActionResult YourEstimates (string client)
		{
			var u = Membership.GetUser ();
			if (u == null) // There was no redirection to any login page
				throw new ConfigurationErrorsException ("no redirection to any login page");
			
			string username = u.UserName;
			Estimate[] estims = WorkFlowManager.GetUserEstimates (username);
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
				return View (new Estimate ()  { Id = id });
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
			string username = Membership.GetUser ().UserName;
			// Obsolete, set in master page
			ViewData ["WebApiBase"] = Url.Content (Yavsc.WebApiConfig.UrlPrefixRelative);
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
					if (string.IsNullOrWhiteSpace (model.Responsible))
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
				throw new Exception ("Not a brand id: " + brandid);
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
				ViewData.Notify( "Catalog introuvable");
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
		/// Contact the specified user.
		/// </summary>
		/// <param name="user">User.</param>
		[Authorize]
		public ActionResult Contact(PerformerContact model)
		{
			return View (model);
		}

		/// <summary>
		/// Send the specified pmsg.
		/// </summary>
		/// <param name="pmsg">Pmsg.</param>
		[Authorize]
		public ActionResult Send(PerformerContact pmsg)
		{
			var performer = Membership.GetUser (pmsg.Performer);
			if (performer == null)
				ModelState.AddModelError ("Performer", "This user doesn't exist");
			var user = Membership.GetUser();
			if (ModelState.IsValid) {
				using (System.Net.Mail.MailMessage msg = new MailMessage(
					YavscHelpers.OwnerEmail, 
					performer.Email,"[Contact] ("+user.UserName+") "+pmsg.Reason,pmsg.Body)) 
				{
					msg.CC.Add(new MailAddress(YavscHelpers.Admail));
					using (System.Net.Mail.SmtpClient sc = new SmtpClient()) 
					{
						sc.Send (msg);
						ViewData.Notify(LocalizedText.Message_sent);
					}
				}
				return View ("Sent");
			}
			else
				return View ("Contact",pmsg);
		}

		/// <summary>
		/// Command the specified collection.
		/// </summary>
		/// <param name="collection">Collection.</param>
		[HttpPost]
		[Authorize]
		public ActionResult DoCommand (FormCollection collection)
		{
			try {
				// Add specified product command to the basket,
				// saves it in db
				// 
				// * check the validity of this request
				// by finding the "type" parameter between
				// the allowed command types
				// * instanciate the given command type, passing it the form data
				// * Make the workflow register this command
				// * Render the resulting basket

				NominativeCommandRegistration cmdreg = YavscHelpers.CreateCommandFromRequest () 
					as NominativeCommandRegistration;
				var basket = WorkFlowManager.GetCommands (User.Identity.Name);
				ViewData["Commanded"] = basket[cmdreg.CommandId]; 
				ViewData["CommandResult"]  = cmdreg ;
				ViewData.Notify( 
					LocalizedText.Item_added_to_basket);
				return View ("Basket",basket);
			} catch (Exception e) {
				YavscHelpers.Notify (ViewData, "Exception:" + e.Message);
				return View (collection);
			}
		}

		/// <summary>
		/// Book the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		public ActionResult EavyBooking (BookingQuery model)
		{
			return View (model);
		}

		private void SetMEACodeViewData(string MEACode) {
			var activities = WorkFlowManager.FindActivity ("%", false);
			var items = new List<SelectListItem> ();
			foreach (var a in activities) {
				items.Add(new SelectListItem() { Selected = MEACode == a.Id, 
					Text = string.Format("{1} : {0}",a.Title,a.Id), 
					Value = a.Id });
			}
			ViewData ["MEACode"] = items;
		}

		/// <summary>
		/// Skills the specified model.
		/// </summary>
		[Authorize (Roles = "Admin")]
		public ActionResult SiteSkills (string MEACode)
		{
			SetMEACodeViewData (MEACode);
			var skills = SkillManager.FindSkill ("%",MEACode);
			return View (skills);
		}

		/// <summary>
		/// Performers on this MEA.
		/// [fr]
		/// Liste des prestataires dont 
		/// l'activité principale est celle spécifiée
		/// </summary>
		/// <param name="id">Identifiant APE de l'activité.</param>
		public ActionResult Performers (string id)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Activities the specified search and toPower.
		/// </summary>
		/// <param name="id">Activity identifier.</param>
		/// <param name="toPower">If set to <c>true</c> to power.</param>
		public ActionResult Activities (string id, bool toPower = false)
		{
			if (id == null)
				id = "%";
			var activities = WorkFlowManager.FindActivity (id, !toPower);
			return View (activities);
		}


		/// <summary>
		/// Activity at the specified id.
		/// </summary>
		/// <param name="MEACode">Identifier.</param>
		public ActionResult Activity (string MEACode)
		{
			return View (WorkFlowManager.GetActivity (MEACode));
		}

		/// <summary>
		/// Display and should
		/// offer Ajax edition of 
		/// user's skills.
		/// </summary>
		/// <param name="id">the User name.</param>
		[Authorize ()]
		public ActionResult UserSkills (string id)
		{
			if (id == null)
				id = User.Identity.Name;
			// TODO or not to do, handle a skills profile update,
			//      actually performed via the Web API :-°
			// else if (ModelState.IsValid) {}
			var usp = SkillManager.GetUserSkills (id);
			var mea = usp.MEACode;
			// TODO add a route parameter to the profile method,
			// named "fs" (standing for fieldset)
			// That filters the view in order to only edit the given fieldset

			if (mea == "none")
				ViewData.Notify(  "Vous devez choisir une activité avant de pouvoir déclarer vos compétences " +
				"(Editez la rubrique <a href=\"" +
					Url.RouteUrl ("Default", new { controller = "Account", action = "Profile", id = User.Identity.Name, fs="infopub" }) + "\">Informations publiques</a> votre profile)");
				
			var skills = SkillManager.FindSkill ("%",usp.MEACode);
			ViewData ["SiteSkills"] = skills;
			return View (usp);
		}

		/// <summary>
		/// Dates the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="model">Model.</param>
		[Authorize,HttpPost]
		public ActionResult Book (BookingQuery model)
		{
			DateTime mindate = DateTime.Now;
			if (model.StartDate.Date < mindate.Date) {
				ModelState.AddModelError ("StartDate", LocalizedText.FillInAFutureDate);
			}
			if (model.EndDate < model.StartDate)
				ModelState.AddModelError ("EndDate", LocalizedText.StartDateAfterEndDate);

			if (ModelState.IsValid) {

				var result = new List<PerformerProfile> ();

				foreach (string meacode in model.MEACodes) {
					foreach (PerformerProfile profile in  WorkFlowManager.FindPerformer(meacode,null)) {
						if (profile.HasCalendar())
						try {
							var events = ProfileBase.Create (profile.UserName).GetEvents (model.StartDate, model.EndDate);
							if (events.items.Length == 0)
								result.Add (profile);
						} catch (WebException ex) {
							string response;
							using (var stream = ex.Response.GetResponseStream ()) {
								using (var reader = new StreamReader (stream)) {
									response = reader.ReadToEnd ();
									stream.Close ();
								}
								ViewData.Notify( 
									string.Format (
										"Google calendar API exception {0} : {1}<br><pre>{2}</pre>",
										ex.Status.ToString (),
										ex.Message,
										response));
							}
						}
					}
				}
				return View ("Performers", result.ToArray ());
			}
			return View (model);
		}

		/// <summary>
		/// Booking the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		public ActionResult Booking (SimpleBookingQuery model)
		{
			// assert (model.MEACode!=null), since it's the required part of the route data
			var needs = SkillManager.FindSkill ("%", model.MEACode);
			ViewBag.Activity = WorkFlowManager.GetActivity (model.MEACode);
			var specification = new List<SkillRating> ();
			ViewData ["Needs"] = needs;
			if (model.Need != null) {
				if (model.Need != "none")
				foreach (string specitem in model.Need.Split(',')) {
					string[] specvals = specitem.Split (' ');
					specification.Add (new SkillRating () { Id = long.Parse (specvals [0]),
						Rate = int.Parse (specvals [1])
					});
				}
			} 
			// In order to present this form 
			// with no need selected and without 
			// validation error display,
			// we only check the need here, not at validation time.
			// Although, the need is indeed cruxial requirement,
			// but we already have got a MEA code  
			if (ModelState.IsValid && model.Need!=null) {
				var result = new List<PerformerAvailability> ();
				foreach (PerformerProfile profile in WorkFlowManager.FindPerformer(model.MEACode,specification.ToArray())) 
					if (profile.HasCalendar ()) {
						try {
							var events = ProfileBase.Create (profile.UserName).GetEvents (
								             model.PreferedDate.Date,
								             model.PreferedDate.AddDays (1).Date);
							// TODO replace (events.items.Length == 0) 
							// with a descent computing of dates and times claims, calendar,
							// AND performer preferences, perhaps also client preferences
							result.Add (profile.CreateAvailability (model.PreferedDate, (events.items.Length == 0)));
						} catch (WebException ex) {
							HandleWebException (ex, "Google Calendar Api");
						}
					} else
						result.Add (profile.CreateAvailability (model.PreferedDate, false));
				ViewData["Circles"] = CircleManager.ListAvailableCircles(); 
				ViewBag.SimpleBookingQuery = model;
				ViewBag.ClientName = User.Identity.IsAuthenticated ?
					User.Identity.Name : User.Identity.Name;

				return View ("Performers", result.ToArray ());
			} 
			if (model.Need==null) {
				// A first Get, or no skill available
				model.Need = 
					string.Join(",", needs.Select (x => string.Format ("{0}:{1}", x.Id, x.Rate)).ToArray ());
				if (string.IsNullOrWhiteSpace (model.Need))
					model.Need = "none";
				
			}

			var activity = WorkFlowManager.GetActivity (model.MEACode);
			ViewData ["Activity"] = activity;
			ViewData ["Title"] = activity.Title;
			ViewData ["Comment"] = activity.Comment;
			ViewData ["Photo"] = activity.Photo;
			if (model.PreferedDate < DateTime.Now)
				model.PreferedDate = DateTime.Now;
			return View (model);
		}

		private void HandleWebException (WebException ex, string context)
		{

			string response = "";
			using (var stream = ex.Response.GetResponseStream ()) { 
				if (stream.CanRead) {
					var reader = new StreamReader (stream);
						response = reader.ReadToEnd ();
						reader.Close ();
						reader.DiscardBufferedData ();
						reader.Dispose ();
					}
				stream.Close ();
				stream.Dispose ();

			}
			ViewData.Notify( 
				string.Format (
					"{3} exception {0} : {1}<br><pre>{2}</pre>",
					ex.Status.ToString (),
					ex.Message,
					response,
					context
				));
		}

	}
}
