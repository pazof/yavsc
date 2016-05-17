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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using Yavsc.Models;
using Yavsc.Models.Auth;
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Messaging;
namespace Yavsc.Helpers
{
    /// <summary>
    /// Google helpers.
    /// </summary>
    public static class GoogleHelpers
	{

		/// <summary>
		/// Notifies the event.
		/// </summary>
		/// <returns>The event.</returns>
		/// <param name="evpub">Evpub.</param>
		public static async Task<MessageWithPayloadResponse> NotifyEvent
        (this HttpClient channel, GoogleAuthSettings googleSettings, CircleEvent evpub) {
    // ASSERT ModelState.IsValid implies evpub.Circles != null
				//send a  MessageWithPayload<YaEvent> to circle members
                // receive MessageWithPayloadResponse
                // "https://gcm-http.googleapis.com/gcm/send"
			var request = new HttpRequestMessage(HttpMethod.Get, Constants.GCMNotificationUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Key", googleSettings.ApiKey);

            var regids = new List<string> ();
            var to = new List<string> ();
            foreach (var c in evpub.Circles)
            foreach (var u in c.Members) {
                    if (u.Member.GoogleRegId == null)
                        to.Add (u.Member.Email);
                    else
                        regids.Add ((string)u.Member.GoogleRegId);
            }

            if (regids.Count == 0)
                throw new InvalidOperationException
                ("No recipient where found for this circle list");

            var msg = new MessageWithPayload<YaEvent> () {
                notification = new Notification() { title = evpub.Title, body = evpub.Description, icon = "event" },
                data = evpub , registration_ids = regids.ToArray() };

                var response = await channel.SendAsync(request);
                var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                return  payload.Value<MessageWithPayloadResponse>();
		}

        public static async Task<MessageWithPayloadResponse> NotifyEvent<Event>
         (this HttpClient channel, GoogleAuthSettings googleSettings, string regid, Event ev)
           where Event : YaEvent
		{
            if (regid == null)
                throw new NotImplementedException ("Notify & No GCM reg id");
			var request = new HttpRequestMessage(HttpMethod.Get, Constants.GCMNotificationUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Key", googleSettings.ApiKey);
            var msg = new MessageWithPayload<Event> () {
                notification = new Notification() { title = ev.Title,
                    body = ev.Description + ev.Comment==null?
                        "":"("+ev.Comment+")", icon = "icon" },
                data = ev, registration_ids = new string[] { regid }  };

            var response = await channel.SendAsync(request);
            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            return  payload.Value<MessageWithPayloadResponse>();
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

