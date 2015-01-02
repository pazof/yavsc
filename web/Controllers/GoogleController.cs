using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Mvc;
using System.Configuration;
using System.Threading.Tasks;
using System.Text;
using Mono.Security.Protocol.Tls;
using System.Net;
using System.IO;
using Yavsc.Model;
using Newtonsoft.Json;
using Yavsc.Model.Google;
using Yavsc.Model.RolesAndMembers;
using System.Web.Security;
using System.Web.Profile;

namespace Yavsc.Controllers
{

	public class GoogleController : Controller
	{
		// datetime format : yyyy-mm-ddTHH:MM:ss
		// 2015-01-01T10:00:00-07:00
		// private string API_KEY="AIzaSyBV_LQHb22nGgjNvFzZwnQHjao3Q7IewRw";

		private string getPeopleUri = "https://www.googleapis.com/plus/v1/people";
		private string getCalListUri = "https://www.googleapis.com/calendar/v3/users/me/calendarList";
		private string getCalEntriesUri = "https://developers.google.com/google-apps/calendar/v3/reference/events/list";

		private string CLIENT_ID = "325408689282-6bekh7p3guj4k0f3301a6frf025cnrk1.apps.googleusercontent.com";
		private string CLIENT_SECRET = "MaxYcvJJCs2gDGvaELZbzwfL";

		string[] SCOPES = { 
			"openid",
			"profile",
			"email"
		};

		string tokenUri = "https://accounts.google.com/o/oauth2/token";
		string authUri  = "https://accounts.google.com/o/oauth2/auth";

		private string SetSessionSate ()
		{
			Random rand = new Random ();
			string state = "security_token" + rand.Next (100000).ToString () + rand.Next (100000).ToString ();
			Session ["state"] = state;
			return state;
		}

		public void Login (string returnUrl)
		{
			if (string.IsNullOrWhiteSpace (returnUrl))
				returnUrl = "/";
			Session ["returnUrl"] = returnUrl;

			string redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/Auth";
			string scope = string.Join ("%20", SCOPES);

			string prms = String.Format ("response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}&include_granted_scopes=false",
				              CLIENT_ID, redirectUri, scope, SetSessionSate ());

			GetAuthResponse (prms);
		}

		private void GetAuthResponse (string prms)
		{
			WebRequest wr = WebRequest.Create (authUri + "?" + prms);
			wr.Method = "GET";
			WebResponse response = wr.GetResponse ();
			string resQuery = response.ResponseUri.Query;
			string cont = HttpUtility.ParseQueryString (resQuery) ["continue"];
			Response.Redirect (cont);
		}

		[Authorize]
		public void GetCalAuth ()
		{
			string redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/CalAuth";
			string scope = string.Join ("%20", SCOPES);
			scope += "%20https://www.googleapis.com/auth/calendar";
			string prms = String.Format ("response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}&include_granted_scopes=false&access_type=offline",
				              CLIENT_ID, redirectUri, scope, SetSessionSate ());
			Session ["calasked"] = true;
			GetAuthResponse (prms);
		}

		[HttpGet]
		[Authorize]
		public ActionResult CalAuth ()
		{
			string redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/CalAuth";
			AuthToken gat = GetToken (TokenPostDataFromCode(redirectUri, GetCodeFromRequest()));
			if (gat == null) {
				return View ("Auth");
			}
			SaveToken (gat);
			HttpContext.Profile.SetPropertyValue ("gcalapi", true);
			string returnUrl = (string)Session ["returnUrl"];
			Session ["returnUrl"]=null;
			return Redirect (returnUrl);
		}

		/// <summary>
		/// Saves the token.
		/// This calls the Profile.Save() method.
		/// It should be called immediatly after getting the token from Google, in
		/// order to save a descent value as expiration date.
		/// </summary>
		/// <param name="gat">Gat.</param>
		private void SaveToken(AuthToken gat)
		{		
			HttpContext.Profile.SetPropertyValue ("gtoken", gat.access_token);
			if (gat.refresh_token!=null)
				HttpContext.Profile.SetPropertyValue ("grefreshtoken", gat.refresh_token);
			HttpContext.Profile.SetPropertyValue ("gtokentype", gat.token_type);
			HttpContext.Profile.SetPropertyValue ("gtokenexpir", DateTime.Now.AddSeconds(gat.expires_in));
			HttpContext.Profile.Save ();
		}

		private string GetCodeFromRequest()
		{
			string code = Request.Params ["code"];
			string error = Request.Params ["error"];
			if (error != null) {
				ViewData ["Message"] = 
					string.Format (LocalizedText.Google_error,
						LocalizedText.ResourceManager.GetString (error));
				return null;
			}
			string state = Request.Params ["state"];
			if (state != null && string.Compare ((string)Session ["state"], state) != 0) {
				ViewData ["Message"] = 
					LocalizedText.ResourceManager.GetString ("invalid request state");
				return null;
			}
			return code;
		}

		private AuthToken GetToken (string postdata)
		{
			Byte[] bytes = System.Text.Encoding.UTF8.GetBytes (postdata);
			HttpWebRequest webreq = WebRequest.CreateHttp (tokenUri);
			webreq.Method = "POST";
			webreq.Accept = "application/json";
			webreq.ContentType = "application/x-www-form-urlencoded";
			webreq.ContentLength = bytes.Length;
			using (Stream dataStream = webreq.GetRequestStream ()) {
				dataStream.Write (bytes, 0, bytes.Length);
			}
			AuthToken gat =null;

			using (WebResponse response = webreq.GetResponse ()) {
				using (Stream responseStream = response.GetResponseStream ()) {
					using (StreamReader readStream = new StreamReader (responseStream, Encoding.UTF8)) {
						string responseStr = readStream.ReadToEnd ();
						gat = JsonConvert.DeserializeObject<AuthToken> (responseStr);
					}
				}
			}
			return gat;
		}

		private string TokenPostDataFromCode(string redirectUri, string code)
		{
			string postdata = 
				string.Format (
					"redirect_uri={0}&client_id={1}&client_secret={2}&code={3}&grant_type=authorization_code",
					HttpUtility.UrlEncode (redirectUri),
					HttpUtility.UrlEncode (CLIENT_ID),
					HttpUtility.UrlEncode (CLIENT_SECRET),
					HttpUtility.UrlEncode (code));
			return postdata;
		}

		[HttpGet]
		public ActionResult Auth ()
		{
			string redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/Auth";
			AuthToken gat = GetToken (TokenPostDataFromCode( redirectUri, GetCodeFromRequest()));
			if (gat == null) {
				return View ();
			}
			string returnUrl = (string)Session ["returnUrl"];

			SignIn regmod = new SignIn ();
			HttpWebRequest webreppro = WebRequest.CreateHttp (getPeopleUri + "/me");
			webreppro.ContentType = "application/http";
			webreppro.Headers.Add (HttpRequestHeader.Authorization, gat.token_type + " " + gat.access_token);
			webreppro.Method = "GET";
			using (WebResponse proresp = webreppro.GetResponse ()) {
				using (Stream prresponseStream = proresp.GetResponseStream ()) {
					using (StreamReader readproresp = new StreamReader (prresponseStream, Encoding.UTF8)) {
						string prresponseStr = readproresp.ReadToEnd ();
						People me = JsonConvert.DeserializeObject<People> (prresponseStr);
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
				}
			}	
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

		private string GetFreshGoogleCredential (ProfileBase pr)
		{
			string token = (string) pr.GetPropertyValue ("gtoken");
			string token_type = (string) pr.GetPropertyValue ("gtokentype");
			DateTime token_exp = (DateTime) pr.GetPropertyValue ("gtokenexpir");
			if (token_exp < DateTime.Now) {
				string refresh_token = (string) pr.GetPropertyValue ("grefreshtoken");
				AuthToken gat = GetToken(
					string.Format("grant_type=refresh_token&client_id={0}&client_secret={1}&refresh_token={2}",
						CLIENT_ID, CLIENT_SECRET, refresh_token));
				token = gat.access_token;
				pr.SetPropertyValue ("gtoken", token);
				pr.Save ();
				// assert gat.token_type == token_type
			}
			return token_type + " " + token;
		}

		[Authorize]
		[HttpGet]
		public ActionResult ChooseCalendar (string returnUrl)
		{
			Session ["ChooseCalReturnUrl"] = returnUrl;
			bool hasCalAuth = (bool)HttpContext.Profile.GetPropertyValue ("gcalapi");
			if (!hasCalAuth) {
				Session["returnUrl"] = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/ChooseCalendar";
				return RedirectToAction ("GetCalAuth");
			}

			string cred = GetFreshGoogleCredential (HttpContext.Profile);

			HttpWebRequest webreq = WebRequest.CreateHttp (getCalListUri);
			webreq.Headers.Add (HttpRequestHeader.Authorization, cred);
			webreq.Method = "GET";
			webreq.ContentType = "application/http";
			using (WebResponse resp = webreq.GetResponse ()) {
				using (Stream respstream = resp.GetResponseStream ()) {
					using (StreamReader readresp = new StreamReader (respstream, Encoding.UTF8)) {
						string responseStr = readresp.ReadToEnd ();
						CalendarList res = JsonConvert.DeserializeObject<CalendarList> (responseStr);
						ViewData ["json"] = responseStr;
						return View (res);
					}
				}
			}
		}

		[HttpPost]
		[Authorize]
		public ActionResult SetCalendar (string calchoice)
		{
			HttpContext.Profile.SetPropertyValue ("gcalid", calchoice);
			HttpContext.Profile.Save ();

			string returnUrl = (string) Session ["ChooseCalReturnUrl"];
			if (returnUrl != null) {
				Session ["ChooseCalReturnUrl"] = null;
				return Redirect (returnUrl);
			}
			return Redirect ("/");
		}

		[Authorize]
		[HttpGet]
		public ActionResult DateQuery()
		{
			return View (new AskForADate ());
		}

		[Authorize]
		[HttpPost]
		public ActionResult DateQuery(AskForADate model)
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
				ProfileBase upr = ProfileBase.Create (model.UserName);
				if (upr == null) {
					ModelState.AddModelError ("UserName", "Non existent user");
					return View (model);
				}


				HttpWebRequest webreq = WebRequest.CreateHttp (getCalEntriesUri);
				webreq.Headers.Add (HttpRequestHeader.Authorization, GetFreshGoogleCredential(upr));
				webreq.Method = "GET";
				webreq.ContentType = "application/http";
				using (WebResponse resp = webreq.GetResponse ()) {
					using (Stream respstream = resp.GetResponseStream ()) {
						using (StreamReader readresp = new StreamReader (respstream, Encoding.UTF8)) {
							string responseStr = readresp.ReadToEnd ();
							CalendarList res = JsonConvert.DeserializeObject<CalendarList> (responseStr);
							ViewData ["json"] = responseStr;
							return View (res);
						}
					}
				}
			}
			return View (model);
		}
	}
}