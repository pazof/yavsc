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
using System.Web.Profile;
using System.Web.Security;
using Newtonsoft.Json;
using Yavsc.Model;
using Yavsc.Model.Google;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Helpers.Google;
using Yavsc.Model.Calendar;
using Yavsc.Helpers;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Google controller.
	/// </summary>
	public class GoogleController : Controller
	{
		/// <summary>
		/// Index this instance.
		/// </summary>
		public ActionResult Index()
		{
			return View ();
		}

		private string SetSessionSate ()
		{
			string state = "security_token";
			Random rand = new Random ();
			for (int l = 0; l < 32; l++) {
				int r = rand.Next (62);
				char c;
				if (r < 10) {
					c = (char)('0' +  r);
				} else if (r < 36) {
					r -= 10;
					c = (char) ('a' + r);
				} else {
					r -= 36;
					c = (char) ('A' + r);
				}
				state += c;
			}
			Session ["state"] = state;
			return state;
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
			OAuth2 oa = new OAuth2 (AuthGRU);
			oa.Login (Response, SetSessionSate ());
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
			OAuth2 oa = new OAuth2 (CalendarGRU);
			oa.GetCalAuth (Response, SetSessionSate ());
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
			OAuth2 oa = new OAuth2 (CalendarGRU);

			AuthToken gat = oa.GetToken (Request, (string)Session ["state"], out msg);
			if (gat == null) {
				YavscHelpers.Notify(ViewData,  msg);
				return View ("Auth");
			}
			SaveToken (HttpContext.Profile,gat);
			HttpContext.Profile.SetPropertyValue ("gcalapi", true);
			string returnUrl = (string)Session ["returnUrl"];
			Session ["returnUrl"] = null;
			return Redirect (returnUrl);
		}

		/// <summary>
		/// Saves the token.
		/// This calls the Profile.Save() method.
		/// It should be called immediatly after getting the token from Google, in
		/// order to save a descent value as expiration date.
		/// </summary>
		/// <param name="gat">Gat.</param>
		private void SaveToken (ProfileBase pr, AuthToken gat)
		{	
			pr.SetPropertyValue ("gtoken", gat.access_token);
			if (gat.refresh_token != null)
				pr.SetPropertyValue ("grefreshtoken", gat.refresh_token);
			pr.SetPropertyValue ("gtokentype", gat.token_type);
			pr.SetPropertyValue ("gtokenexpir", DateTime.Now.AddSeconds (gat.expires_in));
			pr.Save ();
		}

		/// <summary>
		/// Auth this instance.
		/// </summary>
		[HttpGet]
		public ActionResult Auth ()
		{
			string msg;
			OAuth2 oa = new OAuth2 (AuthGRU);
			AuthToken gat = oa.GetToken (Request, (string)Session ["state"], out msg);
			if (gat == null) {
				YavscHelpers.Notify(ViewData,  msg);
				return View ();
			}
			string returnUrl = (string)Session ["returnUrl"];
			SignIn regmod = new SignIn ();

			People me = PeopleApi.GetMe (gat);
			// TODO use me.id to retreive an existing user
			string accEmail = me.emails.Where (x => x.type == "account").First ().value;
			MembershipUserCollection mbrs = Membership.FindUsersByEmail (accEmail);
			if (mbrs.Count == 1) {
				// TODO check the google id
				// just set this user as logged on
				foreach (MembershipUser u in mbrs) {
				string username = u.UserName;
				    FormsAuthentication.SetAuthCookie (username, true);
					/* var upr = ProfileBase.Create (username);
					SaveToken (upr,gat); */
				}
				Session ["returnUrl"] = null;
				return Redirect (returnUrl);
			}
			// else create the account
			regmod.Email = accEmail;
			regmod.UserName = me.displayName;
			Session ["me"] = me;
			Session ["GoogleAuthToken"] = gat;
			return Auth (regmod);
		}

		/// <summary>
		/// Creates an account using the Google authentification.
		/// </summary>
		/// <param name="regmod">Regmod.</param>
		[HttpPost]
		public ActionResult Auth (SignIn regmod)
		{
			if (ModelState.IsValid) {
				if (Membership.GetUser (regmod.UserName) != null) {
					ModelState.AddModelError ("UserName", "This user name already is in use");
					return View ();
				}
				string returnUrl = (string)Session ["returnUrl"];
				AuthToken gat = (AuthToken)Session ["GoogleAuthToken"];
				People me = (People)Session ["me"];
				if (gat == null || me == null)
					throw new InvalidDataException ();

				Random rand = new Random ();
				string passwd = rand.Next (100000).ToString () + rand.Next (100000).ToString ();

				MembershipCreateStatus mcs;
				Membership.CreateUser (
					regmod.UserName,
					passwd,
					regmod.Email,
					null,
					null,
					true,
					out mcs);
				switch (mcs) {
				case MembershipCreateStatus.DuplicateEmail:
					ModelState.AddModelError ("Email", "Cette adresse e-mail correspond " +
					"à un compte utilisateur existant");
					return View (regmod);
				case MembershipCreateStatus.DuplicateUserName:
					ModelState.AddModelError ("UserName", "Ce nom d'utilisateur est " +
					"déjà enregistré");
					return View (regmod);
				case MembershipCreateStatus.Success:
					Membership.ValidateUser (regmod.UserName, passwd);
					FormsAuthentication.SetAuthCookie (regmod.UserName, true);

					HttpContext.Profile.Initialize (regmod.UserName, true);
					HttpContext.Profile.SetPropertyValue ("Name", me.displayName);
					// TODO use image
					if (me.image != null) {
						HttpContext.Profile.SetPropertyValue ("Avatar", me.image.url);
					}
					if (me.placesLived != null) {
						People.Place pplace = me.placesLived.Where (x => x.primary).First ();
						if (pplace != null)
							HttpContext.Profile.SetPropertyValue ("CityAndState", pplace.value);
					}
					if (me.url != null)
						HttpContext.Profile.SetPropertyValue ("WebSite", me.url);
					SaveToken (HttpContext.Profile,gat);
					// already done in SaveToken: HttpContext.Profile.Save ();
					return Redirect (returnUrl);
				}
				ViewData ["returnUrl"] = returnUrl;
			}
			return View (regmod);
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
			bool hasCalAuth = (bool)HttpContext.Profile.GetPropertyValue ("gcalapi");
			if (!hasCalAuth) {
				Session ["returnUrl"] = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/ChooseCalendar";
				return RedirectToAction ("GetCalAuth",
					new { 
						returnUrl = "ChooseCalendar?returnUrl="+HttpUtility.UrlEncode(returnUrl)
					});
			}
			string cred = OAuth2.GetFreshGoogleCredential (HttpContext.Profile);

			CalendarApi c = new CalendarApi ();
			CalendarList cl = c.GetCalendars (cred);
			ViewData ["returnUrl"] = returnUrl;
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
			return View (new BookQuery ());
		}
		
		/// <summary>
		/// Dates the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="model">Model.</param>
		[Authorize,HttpPost]
		public ActionResult Book (BookQuery model)
		{
			if (ModelState.IsValid) {
				DateTime mindate = DateTime.Now;
				if (model.StartDate < mindate){
					ModelState.AddModelError ("StartDate", LocalizedText.FillInAFutureDate);
				}
				if (model.EndDate < model.StartDate)
					ModelState.AddModelError ("EndDate", LocalizedText.StartDateAfterEndDate);

				var muc = Membership.FindUsersByName (model.Person);
				if (muc.Count == 0) {
					ModelState.AddModelError ("Person", LocalizedText.Non_existent_user);
				}
				if (!Roles.IsUserInRole (model.Role)) {
					ModelState.AddModelError ("Role", LocalizedText.UserNotInThisRole);
				}
				ProfileBase upr = ProfileBase.Create (model.Person);
				var gcalid = upr.GetPropertyValue ("gcalid");
				if (gcalid is DBNull)
					ModelState.AddModelError ("Person", LocalizedText.No_calendar_for_this_user);
				if (ModelState.IsValid) {
					string calid = (string) gcalid; 
					DateTime maxdate = model.EndDate;
					CalendarApi c = new CalendarApi ();
					CalendarEventList events;
					try {
						events = c.GetCalendar (calid, mindate, maxdate, upr);
						YavscHelpers.Notify (ViewData, "Google calendar API call success");
					} catch (WebException ex) {
						string response;
						using (var stream = ex.Response.GetResponseStream())
						using (var reader = new StreamReader(stream))
						{
							response = reader.ReadToEnd();
						}
						YavscHelpers.Notify (ViewData, 
							string.Format(
								"Google calendar API exception {0} : {1}<br><pre>{2}</pre>",
								ex.Status.ToString(),
								ex.Message,
								response));
					}
				}
			}
			return View (model);
		}
	}
}
