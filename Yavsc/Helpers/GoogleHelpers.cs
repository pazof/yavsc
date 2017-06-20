//
//  GoogleHelpers.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Yavsc.Helpers
{
    using Models.Google.Messaging;
    using Models.Messaging;
    using Models;
    using Interfaces.Workflow;
    using Yavsc.Models.Google;
    using Yavsc.Models.Calendar;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Auth.OAuth2.Responses;
    using Microsoft.Data.Entity;
    using Google.Apis.Auth.OAuth2.Flows;
    using Microsoft.AspNet.Identity.EntityFramework;


    /// <summary>
    /// Google helpers.
    /// </summary>
    public static class GoogleHelpers
    {
        public static async Task <MessageWithPayloadResponse> NotifyEvent<Event>
         (this GoogleAuthSettings googleSettings, IEnumerable<string> regids, Event ev)
           where Event : IEvent
        {
            if (ev == null)
                throw new Exception("Spécifier un évènement");
            if (ev.Message == null)
                throw new Exception("Spécifier un message");
            if (ev.Sender == null)
                throw new Exception("Spécifier un expéditeur");

            if (regids == null)
                throw new NotImplementedException("Notify & No GCM reg ids");

            var msg = new MessageWithPayload<Event>()
            {
                notification = new Notification()
                {
                    title = ev.Topic+" "+ev.Sender,
                    body =  ev.Message,
                    icon = "icon"
                },
                data = ev,
                registration_ids = regids.ToArray()
            };
            try {
                using (var m = new SimpleJsonPostMethod("https://gcm-http.googleapis.com/gcm/send",$"key={googleSettings.ApiKey}")) {
                    return await m.Invoke<MessageWithPayloadResponse>(msg);
                }
            }
            catch (Exception ex) {
                throw new Exception ("Quelque chose s'est mal passé à l'envoi",ex);
            }
        }
        public  static ServiceAccountCredential GetCredentialForApi(IEnumerable<string> scopes)
        {
			var initializer = new ServiceAccountCredential.Initializer(Startup.GoogleSettings.Account.client_email);
            initializer = initializer.FromPrivateKey(Startup.GoogleSettings.Account.private_key);
            initializer.Scopes = scopes;
            var credential = new ServiceAccountCredential(initializer);
            return credential;
        }

        public static async Task<IdentityUserLogin<string>> GetGoogleUserLoginAsync(
            this UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context, 
            string yavscUserId)
        {
            var user = await userManager.FindByIdAsync(yavscUserId);
            var googleLogin = await context.UserLogins.FirstOrDefaultAsync(
                x => x.UserId == yavscUserId && x.LoginProvider == "Google"
            );
            return googleLogin;
        }
        public static UserCredential GetGoogleCredential(IdentityUserLogin<string> googleUserLogin)
        {
            var googleId = googleUserLogin.ProviderKey;
            if (string.IsNullOrEmpty(googleId))
                throw new InvalidOperationException("No Google login");
            TokenResponse resp = null;
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer());
            return new UserCredential(flow, googleId, resp);
        }
        static string evStatusDispo = "Dispo";

        public static async Task<Period[]> GetFreeTime (this ICalendarManager manager, string calId, DateTime startDate, DateTime endDate) 
        {
            CalendarEventList evlist = await manager.GetCalendarAsync(calId, startDate, endDate) ;
            var result = evlist.items
            .Where(
                ev => ev.status == evStatusDispo
            )
            .Select( 
                ev => new Period {
                     Start = ev.start.datetime,
                     End = ev.end.datetime
                  }
            );
            return result.ToArray();
        }

        const string jwtHeader="{\"alg\":\"RS256\",\"typ\":\"JWT\"}";
        const string tokenEndPoint = "https://www.googleapis.com/oauth2/v4/token";

        static long GetTimeSpan(long seconds) {
            var zero = new DateTime(1970,1,1);
            return zero.AddSeconds(seconds).ToFileTimeUtc();
        }

        static object CreateGoogleServiceClaimSet(string scope, int expiresInSeconds) {
            return new { 
                iss = Startup.GoogleSettings.Account.client_email,
                scope = scope,
                aud = "https://www.googleapis.com/oauth2/v4/token",
                exp = GetTimeSpan(expiresInSeconds),
                iat = DateTime.Now.ToFileTimeUtc()
            };
        } 

        public static async Task<string> GetJsonTokenAsync(string scope)
        {

            var claimSet = CreateGoogleServiceClaimSet(scope, 3600);
            string jsonClaims =  JsonConvert.SerializeObject(claimSet);
            string encClaims =  Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonClaims));
            string tokenHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes(jwtHeader))+"."+encClaims;

            X509Certificate2 cert = new X509Certificate2();
            cert.Import(Convert.FromBase64String(Startup.GoogleSettings.Account.private_key));
            RSACryptoServiceProvider key = new RSACryptoServiceProvider();
            key.FromXmlString(cert.PrivateKey.ToXmlString(true));
            byte[] sig = key.SignData(Encoding.UTF8.GetBytes(tokenHeader), CryptoConfig.MapNameToOID("SHA256"));
            string assertion = tokenHeader+"."+Convert.ToBase64String(sig);
            HttpWebRequest webreq = WebRequest.CreateHttp ("https://www.googleapis.com/oauth2/v4/token");
            webreq.ContentType = "application/x-www-form-urlencoded";
            using (var inputstream = await webreq.GetRequestStreamAsync()) {
                var content = Encoding.UTF8.GetBytes( "grant_type="+ HttpUtility.UrlEncode(" urn:ietf:params:oauth:grant-type:jwt-bearer")+
                "&assertion="+HttpUtility.UrlEncode(assertion));
                await inputstream.WriteAsync(content,0,content.Length);
            }
            using (WebResponse resp = await webreq.GetResponseAsync ()) {
                using (Stream respstream = resp.GetResponseStream ()) {
                        using (var rdr = new StreamReader(respstream)) {
                            return await rdr.ReadToEndAsync();
                    }
                }
            }
        }

    }
}

