using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Yavsc.Model;
using Yavsc.Model.Google;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Model.Calendar;
using Yavsc.Helpers;
using Yavsc.Model.WorkFlow;
using Yavsc.Model.FrontOffice;
using Yavsc.Helpers.Google.Api;
using Yavsc.Model.Skill;
using Yavsc.Client.FrontOffice;
using System.Web.Profile;
using Yavsc.Helpers.OAuth.Api;
using Yavsc.Model.Identity;
using Yavsc.Helpers.OAuth;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Google controller.
	/// </summary>
	public class GoogleController : BaseController
	{
		/// <summary>
		/// Index this instance.
		/// </summary>
		public ActionResult Index()
		{
			return View ();
		}

		private string AuthGRU {
			get {
				return Request.Url.Scheme + "://" +
				Request.Url.Authority + "/Google/Auth";
			}
		}

		private string CalendarGRU {
			get {
				return Request.Url.Scheme + "://" +
				Request.Url.Authority + "/Google/CalAuth";
			}
		}
		/// <summary>
		/// Login the specified returnUrl.
		/// </summary>
		/// <param name="returnUrl">Return URL.</param>
		public void Login (string returnUrl)
		{
			if (string.IsNullOrWhiteSpace (returnUrl))
				returnUrl = "/";
			Session ["returnUrl"] = returnUrl;
			string state = Session.SetSessionSate ();
			Response.Login (state, AuthGRU);
		}
		/// <summary>
		/// Gets the cal auth.
		/// </summary>
		/// <param name="returnUrl">Return URL.</param>
		public void GetCalAuth (string returnUrl)
		{
			if (string.IsNullOrWhiteSpace (returnUrl))
				returnUrl = "/";
			Session ["returnUrl"] = returnUrl;
			Response.CalLogin (Session.SetSessionSate (), CalendarGRU);
		}

		/// <summary>
		/// Called after the Google authorizations screen,
		/// we assume that <c>Session</c> contains a redirectUrl entry
		/// </summary>
		/// <returns>The auth.</returns>
		[HttpGet]
		[Authorize]
		public ActionResult CalAuth ()
		{
			string msg;
			CalendarApi oa = GoogleHelpers.CreateOAuth2<CalendarApi>(CalendarGRU);
			AuthToken gat = oa.GetToken<AuthToken> (Request, (string) Session ["state"], out msg);
			if (gat == null) {
				ViewData.Notify( msg);
				return View ("Auth");
			}
			SaveToken (HttpContext.Profile,gat);
			HttpContext.Profile.SetPropertyValue ("gcalapi", true);
			string returnUrl = (string) Session ["returnUrl"];
			Session ["returnUrl"] = null;
			return Redirect (returnUrl);
		}

		/// <summary>
		/// Saves the token.
		/// This calls the Profile.Save() method.
		/// It should be called immediatly after getting the token from Google, in
		/// order to save a descent value as expiration date.
		/// </summary>
		/// <param name="pr">pr.</param>
		/// <param name="gat">Gat.</param>
		private void SaveToken (ProfileBase pr, AuthToken gat)
		{	
			UserNameManager.SaveToken (pr.UserName, gat);
		}



		[Authorize]
		[HttpGet]
		ActionResult PushPos ()
		{
			return View ();
		}

		/// <summary>
		/// Chooses the calendar.
		/// </summary>
		/// <returns>The calendar.</returns>
		/// <param name="returnUrl">Return URL.</param>
		[Authorize]
		[HttpGet]
		public ActionResult ChooseCalendar (string returnUrl)
		{
			if (returnUrl != null) { 
				Session ["chooseCalReturnUrl"] = returnUrl;
				return RedirectToAction ("GetCalAuth",
					new { 
						returnUrl = Url.Action ("ChooseCalendar") // "ChooseCalendar?returnUrl="+HttpUtility.UrlEncode(returnUrl)
					});
			}
			CalendarList cl = GoogleHelpers.GetCalendars(HttpContext.Profile);
			ViewData ["returnUrl"] = Session ["chooseCalReturnUrl"];
			return View (cl);
		}
		
		/// <summary>
		/// Sets the calendar.
		/// </summary>
		/// <returns>The calendar.</returns>
		/// <param name="calchoice">Calchoice.</param>
		/// <param name="returnUrl">return Url.</param>
		[HttpPost]
		[Authorize]
		public ActionResult SetCalendar (string calchoice,string returnUrl)
		{
			HttpContext.Profile.SetPropertyValue ("gcalid", calchoice);
			HttpContext.Profile.Save ();

			if (returnUrl != null) {
				return Redirect (returnUrl);
			}
			return Redirect ("/");
		}
		
		/// <summary>
		/// Dates the query.
		/// </summary>
		/// <returns>The query.</returns>
		[Authorize,HttpGet]
		public ActionResult Book ()
		{
			var model = new BookingQuery ();
			model.StartDate = DateTime.Now;
			model.EndDate = model.StartDate.AddDays(2);
			model.StartHour = DateTime.Now.ToString("HH:mm");
			model.EndHour = DateTime.Now.AddHours(1).ToString("HH:mm");
			return View (model);
		}
		


		public ActionResult Book (SimpleBookingQuery model)
		{
			return View (model);
		}
	}
}
