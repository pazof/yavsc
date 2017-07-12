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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yavsc.Helpers
{
    using Models.Google.Messaging;
    using Models.Messaging;
    using Models;
    using Interfaces.Workflow;
    using Yavsc.Models.Calendar;
    using Google.Apis.Auth.OAuth2;
    using Microsoft.Data.Entity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Yavsc.Services;
    using Newtonsoft.Json;
    using Google.Apis.Services;
    using Google.Apis.Compute.v1;




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
        public  static ServiceAccountCredential OupsGetCredentialForApi(IEnumerable<string> scopes)
        {
			var initializer = new ServiceAccountCredential.Initializer(Startup.GoogleSettings.Account.client_email);
            initializer = initializer.FromPrivateKey(Startup.GoogleSettings.Account.private_key);
            initializer.Scopes = scopes;
            var credential = new ServiceAccountCredential(initializer);
            return credential;
        }

        public static async Task<GoogleCredential> GetCredentialForApi(IEnumerable<string> scopes)
        {
            GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();
			var baseClientService = new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential
				};
			var compute = new ComputeService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential
				});
			if (credential.IsCreateScopedRequired)
			{
				credential = credential.CreateScoped(scopes);
			}
            return credential;
        }
        public static async Task<IdentityUserLogin<string>> GetGoogleUserLoginAsync(
            this ApplicationDbContext context, 
            string yavscUserId)
        {
            var user = context.Users.FirstOrDefaultAsync(u=>u.Id==yavscUserId);
            if (user==null) return null;
            var googleLogin = await context.UserLogins.FirstOrDefaultAsync(
                x => x.UserId == yavscUserId && x.LoginProvider == "Google"
            );
            return googleLogin;
        }

        public static async Task<Period[]> GetFreeTime (this ICalendarManager manager, string calId, DateTime startDate, DateTime endDate) 
        {
            var evlist = await manager.GetCalendarAsync(calId, startDate, endDate, null) ;
            var result = evlist.Items
            .Where(
                ev => ev.Transparency == "transparent"
                         )
            .Select( 
                ev => new Period {
                     Start = ev.Start.DateTime.Value,
                     End = ev.End.DateTime.Value
                  }
            );
            return result.ToArray();
        }

    }
}

