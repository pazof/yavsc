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
using Mono.Security.Protocol.Tls;
using Newtonsoft.Json;
using Yavsc.Model;
using Yavsc.Model.Google;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Helpers.Google;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Google controller.
	/// </summary>
	public class GoogleController : Controller
	{

		private string SetSessionSate ()
		{
			Random rand = new Random ();
			string state = "security_token" + rand.Next (100000).ToString () + rand.Next (100000).ToString ();
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
				ViewData ["Message"] = msg;
				return View ("Auth");
			}
			SaveToken (gat);
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
		private void SaveToken (AuthToken gat)
		{		
			HttpContext.Profile.SetPropertyValue ("gtoken", gat.access_token);
			if (gat.refresh_token != null)
				HttpContext.Profile.SetPropertyValue ("grefreshtoken", gat.refresh_token);
			HttpContext.Profile.SetPropertyValue ("gtokentype", gat.token_type);
			HttpContext.Profile.SetPropertyValue ("gtokenexpir", DateTime.Now.AddSeconds (gat.expires_in));
			HttpContext.Profile.Save ();
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
				ViewData ["Message"] = msg;
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
				FormsAuthentication.SetAuthCookie (me.displayName, true);
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
						HttpContext.Profile.SetPropertyValue ("avatar", me.image.url);
					}
					if (me.placesLived != null) {
						People.Place pplace = me.placesLived.Where (x => x.primary).First ();
						if (pplace != null)
							HttpContext.Profile.SetPropertyValue ("CityAndState", pplace.value);
					}
					if (me.url != null)
						HttpContext.Profile.SetPropertyValue ("WebSite", me.url);
					SaveToken (gat);
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
			Session ["ChooseCalReturnUrl"] = returnUrl;
			bool hasCalAuth = (bool)HttpContext.Profile.GetPropertyValue ("gcalapi");
			if (!hasCalAuth) {
				Session ["returnUrl"] = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/ChooseCalendar";
				return RedirectToAction ("GetCalAuth");
			}
			string cred = OAuth2.GetFreshGoogleCredential (HttpContext.Profile);
			string json;
			CalendarApi c = new CalendarApi ();
			CalendarList cl = c.GetCalendars (cred, out json);
			ViewData ["json"] = json;
			return View (cl);
		}
		
		/// <summary>
		/// Sets the calendar.
		/// </summary>
		/// <returns>The calendar.</returns>
		/// <param name="calchoice">Calchoice.</param>
		[HttpPost]
		[Authorize]
		public ActionResult SetCalendar (string calchoice)
		{
			HttpContext.Profile.SetPropertyValue ("gcalid", calchoice);
			HttpContext.Profile.Save ();

			string returnUrl = (string)Session ["ChooseCalReturnUrl"];
			if (returnUrl != null) {
				Session ["ChooseCalReturnUrl"] = null;
				return Redirect (returnUrl);
			}
			return Redirect ("/");
		}
		
		/// <summary>
		/// Dates the query.
		/// </summary>
		/// <returns>The query.</returns>
		[Authorize]
		[HttpGet]
		public ActionResult DateQuery ()
		{
			return View (new AskForADate ());
		}
		
		/// <summary>
		/// Dates the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="model">Model.</param>
		[Authorize]
		[HttpPost]
		public ActionResult DateQuery (AskForADate model)
		{
			if (ModelState.IsValid) {
				if (model.MinDate < DateTime.Now) {
					ModelState.AddModelError ("MinTime", "This first date must be in the future.");
					return View (model);
				}
				if (model.MinDate > model.MaxDate) {
					ModelState.AddModelError ("MinTime", "This first date must be lower than the second one.");
					return View (model);
				}
				var muc = Membership.FindUsersByName (model.UserName);
				if (muc.Count == 0) {
					ModelState.AddModelError ("UserName", "Non existent user");
					return View (model);
				}
				ProfileBase upr = ProfileBase.Create (model.UserName);

				string calid = (string)upr.GetPropertyValue ("gcalid");
				if (string.IsNullOrWhiteSpace (calid)) {
					ModelState.AddModelError ("UserName", "L'utilisateur n'a pas de calendrier Google associé.");
					return View (model);
				}

				DateTime mindate = model.MinDate;
				mindate = mindate.AddHours (int.Parse (model.MinTime.Substring (0, 2)));
				mindate = mindate.AddMinutes (int.Parse (model.MinTime.Substring (3, 2)));
				DateTime maxdate = model.MaxDate;
				maxdate = maxdate.AddHours (int.Parse (model.MaxTime.Substring (0, 2)));
				maxdate = maxdate.AddMinutes (int.Parse (model.MaxTime.Substring (3, 2)));

				CalendarApi c = new CalendarApi ();
				CalendarEntryList res;
				string responseStr;
				try {
					res = c.GetCalendar (calid, mindate, maxdate, upr, out responseStr);
				} catch (GoogleErrorException ex) {
					ViewData ["Title"] = ex.Title;
					ViewData ["Content"] = ex.Content;
					return View ("GoogleErrorMessage", ex);
				}
				return View (res);
			}
			return View (model);
		}
	}
}