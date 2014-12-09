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

namespace Yavsc.Controllers
{
	public class GoogleController : Controller
	{

		string API_KEY="AIzaSyBV_LQHb22nGgjNvFzZwnQHjao3Q7IewRw";

		string CLIENT_ID="325408689282-6bekh7p3guj4k0f3301a6frf025cnrk1.apps.googleusercontent.com";

		string CLIENT_SECRET="MaxYcvJJCs2gDGvaELZbzwfL";

		string [] SCOPES = { 
			"profile",
			"email"
		} ; 

		string getTokenUrl = "https://accounts.google.com/o/oauth2/token"; 
		// "https://www.googleapis.com/oauth2/v3/token";https://accounts.google.com/o/oauth2/token
		string getCodeUrl = "https://accounts.google.com/o/oauth2/auth";

		public void Login()
		{
			Random rand = new Random ();
			string state = "security_token"+rand.Next (100000).ToString()+rand.Next (100000).ToString();
			Session ["state"] = state;

			string redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/Auth";

			string prms = String.Format("response_type=code&" +
				"client_id={0}&" +
				"redirect_uri={1}&" +
				"scope={2}&" +
				"state={3}&" +
				"access_type=offline&" +
				"include_granted_scopes=false",
				CLIENT_ID,
				redirectUri,
				string.Join("%20",SCOPES),
				state
			);

			WebRequest wr = WebRequest.Create(getCodeUrl+"?"+prms);

			wr.Method = "GET";
			// Get the response.
			try {
				WebResponse response = wr.GetResponse();
				string resQuery = response.ResponseUri.Query;
				string cont = HttpUtility.ParseQueryString(resQuery)["continue"];
				Response.Redirect (cont);
			}
			catch (WebException we) {
				Response.Redirect(we.Response.ResponseUri.AbsoluteUri);
			}

		}
		public void Auth() {
			string redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/Code";
			string code = Request.Params ["code"];
			string error = Request.Params ["error"];
			if (error != null) {
				ViewData ["Message"] = 
					string.Format(LocalizedText.Google_error,
						LocalizedText.ResourceManager.GetString(error));
				return;
			}
			string state = Request.Params ["state"];
			if (state!=null && string.Compare((string)Session ["state"],state)!=0) {
				ViewData ["Message"] = 
					LocalizedText.ResourceManager.GetString("invalid request state");
				return;
			}
			HttpWebRequest webreq = WebRequest.CreateHttp(getTokenUrl);
			webreq.Method = "POST";
			webreq.ContentType = "application/x-www-form-urlencoded";
			webreq.SendChunked = true;
			string postData = String.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code", 
				code, 
				CLIENT_ID, 
				CLIENT_SECRET,
				redirectUri);
			Encoding encr = new UTF8Encoding();
			Byte[] bytes = encr.GetBytes(postData);
			webreq.ContentLength = bytes.Length;
			using (Stream dataStream = webreq.GetRequestStream()) {
				dataStream.Write(bytes,0,bytes.Length);
				dataStream.Close();
			}
			try {
				WebResponse response = webreq.GetResponse();
				string resQuery = response.ResponseUri.Query;
				string cont = HttpUtility.ParseQueryString(resQuery)["continue"];
				Response.Redirect (cont);
			}
			catch (WebException wex) {
				Response.Redirect(wex.Response.ResponseUri.AbsoluteUri);
			}
		}

		public void Code()
		{

		}
	}
}

