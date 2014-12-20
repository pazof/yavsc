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

		// private string API_KEY="AIzaSyBV_LQHb22nGgjNvFzZwnQHjao3Q7IewRw";

		private string getPeopleUri = "https://www.googleapis.com/plus/v1/people";

		private string CLIENT_ID="325408689282-6bekh7p3guj4k0f3301a6frf025cnrk1.apps.googleusercontent.com";

		private string CLIENT_SECRET="MaxYcvJJCs2gDGvaELZbzwfL";

		string [] SCOPES = { 
			"openid" ,
			"profile",
			"email"
		} ; 

		string tokenUri = "https://accounts.google.com/o/oauth2/token"; 
		string authUri = "https://accounts.google.com/o/oauth2/auth";

		public void Login(string returnUrl)
		{
			if (string.IsNullOrWhiteSpace (returnUrl))
				returnUrl = "/";
			Random rand = new Random ();
			string state = "security_token"+rand.Next (100000).ToString()+rand.Next (100000).ToString();
			Session ["state"] = state;
			Session ["returnUrl"] = returnUrl;

			string redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/Auth";

			string prms = String.Format("response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}&access_type=offline&include_granted_scopes=false",
				CLIENT_ID, redirectUri, string.Join("%20",SCOPES), state);

			WebRequest wr = WebRequest.Create(authUri+"?"+prms);
			wr.Method = "GET";
			// Get the response.

				WebResponse response = wr.GetResponse();
				string resQuery = response.ResponseUri.Query;
				string cont = HttpUtility.ParseQueryString(resQuery)["continue"];
				Response.Redirect (cont);


		}

		[HttpGet]
		public ActionResult Auth() 
		{

			string returnUrl = (string) Session ["returnUrl"];
			string redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/Auth";
			string code = Request.Params ["code"];
			string error = Request.Params ["error"];
			if (error != null) {
				ViewData ["Message"] = 
					string.Format(LocalizedText.Google_error,
						LocalizedText.ResourceManager.GetString(error));
				return View();
			}
			string state = Request.Params ["state"];
			if (state!=null && string.Compare((string)Session ["state"],state)!=0) {
				ViewData ["Message"] = 
					LocalizedText.ResourceManager.GetString("invalid request state");
				return View();
			}

			string postdata = 
				string.Format(
					"redirect_uri={0}&client_id={1}&client_secret={2}&code={3}&grant_type=authorization_code",
					HttpUtility.UrlEncode(redirectUri),
					HttpUtility.UrlEncode(CLIENT_ID),
					HttpUtility.UrlEncode(CLIENT_SECRET),
					HttpUtility.UrlEncode(code));

			Byte[] bytes = System.Text.Encoding.UTF8.GetBytes (postdata);
			HttpWebRequest webreq = WebRequest.CreateHttp (tokenUri);
			webreq.Method = "POST";
			webreq.Accept = "application/json";
			webreq.ContentType = "application/x-www-form-urlencoded";
			webreq.ContentLength = bytes.Length;
			using (Stream dataStream = webreq.GetRequestStream ()) {
				dataStream.Write (bytes, 0, bytes.Length);
			};

			using (WebResponse response = webreq.GetResponse ()) {
				using (Stream responseStream = response.GetResponseStream ()) {
					using (StreamReader readStream = new StreamReader (responseStream, Encoding.UTF8)) {
						string responseStr = readStream.ReadToEnd ();
						AuthToken gat = JsonConvert.DeserializeObject<AuthToken>(responseStr);
						Session ["GoogleAuthToken"] = gat;
						SignIn regmod = new SignIn ();
						HttpWebRequest webreppro = WebRequest.CreateHttp (getPeopleUri+"/me");
						webreppro.ContentType = "application/http";
						webreppro.Headers.Add (HttpRequestHeader.Authorization, gat.token_type + " " + gat.access_token);
						webreppro.Method = "GET";
						using (WebResponse proresp = webreppro.GetResponse ()) {
							using (Stream prresponseStream = proresp.GetResponseStream ()) {
								using (StreamReader readproresp = new StreamReader (prresponseStream, Encoding.UTF8)) {
									string prresponseStr = readproresp.ReadToEnd ();
									People me = JsonConvert.DeserializeObject<People> (prresponseStr);
									// TODO use me.id to retreive an existing user
									string accEmail = me.emails.Where (x => x.type == "account").First().value;
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
									return Auth(regmod);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates an account using the Google authentification.
		/// </summary>
		/// <param name="regmod">Regmod.</param>
		[HttpPost]
		public ActionResult Auth(SignIn regmod)
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
					HttpContext.Profile.SetPropertyValue ("gtoken", gat.access_token);
					HttpContext.Profile.SetPropertyValue ("Name", me.displayName);
					// TODO use image
					if (me.image != null) {
						HttpContext.Profile.SetPropertyValue ("avatar", me.image.url);
					}
					if (me.placesLived != null) {
						People.Place pplace = me.placesLived.Where (x => x.primary).First ();
						if (pplace != null)
							HttpContext.Profile.SetPropertyValue ("Address", pplace.value);
					}
					if (me.url != null)
						HttpContext.Profile.SetPropertyValue ("WebSite", me.url);
					HttpContext.Profile.Save ();
					return Redirect (returnUrl);
				}
				ViewData ["returnUrl"] = returnUrl;
			}
			return View (regmod);
		}

		public void ChooseCalendar()	
		{
			throw new NotImplementedException();
		}
	}
}

