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
using Microsoft.AspNet.Identity;
namespace Yavsc.Helpers
{
    using Models.Auth;
    using Models.Google.Messaging;
    using Models.Messaging;
    using Models;
    using Interfaces.Workflow;


    /// <summary>
    /// Google helpers.
    /// </summary>
    public static class GoogleHelpers
    {
/* WAZA
        /// <summary>
        /// Notifies the event.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="evpub">Evpub.</param>
        public static async Task<MessageWithPayloadResponse> NotifyEvent
        (this HttpClient channel, GoogleAuthSettings googleSettings, CircleEvent evpub)
        {
            // ASSERT ModelState.IsValid implies evpub.Circles != null
            //send a  MessageWithPayload<YaEvent> to circle members
            // receive MessageWithPayloadResponse
            // "https://gcm-http.googleapis.com/gcm/send"

            var regids = new List<string>();
            foreach (var c in evpub.Circles)
                foreach (var u in c.Members)
                {
                    regids.AddRange(u.Member.Devices.Select(d => d.GCMRegistrationId));
                }
            if (regids.Count > 0) return null;
            var request = new HttpRequestMessage(HttpMethod.Get, Constants.GCMNotificationUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("key", googleSettings.ApiKey);
            var msg = new MessageWithPayload<YaEvent>()
            {
                notification = new Notification() { title = evpub.Title, body = evpub.Description, icon = "icon" },
                data = evpub,
                registration_ids = regids.ToArray()
            };
            var response = await channel.SendAsync(request);
            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            return payload.Value<MessageWithPayloadResponse>();
        }
*/
        public static MessageWithPayloadResponse NotifyEvent<Event>
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
                return m.Invoke<MessageWithPayloadResponse>(msg);
            }
            }
            catch (Exception ex) {
                throw new Exception ("Quelque chose s'est mal passé à l'envoi",ex);
            }
        }
        public static async Task<UserCredential> GetCredentialForGoogleApiAsync(this UserManager<ApplicationUser> userManager, ApplicationDbContext context, string uid)
        {
            var user = await userManager.FindByIdAsync(uid);
            var googleId = context.UserLogins.FirstOrDefault(
                x => x.UserId == uid
            ).ProviderKey;
            if (string.IsNullOrEmpty(googleId))
                throw new InvalidOperationException("No Google login");
            var token = await context.GetTokensAsync(googleId);
            return new UserCredential(uid, token);
        }

    }
}

