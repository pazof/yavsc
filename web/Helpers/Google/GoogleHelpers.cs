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
using Yavsc.Model.Google;
using System.Web.Profile;
using System.Configuration;
using System.Web;
using Yavsc.Model.Calendar;
using Yavsc.Model.Circles;
using System.Collections.Generic;
using System.Web.Security;

namespace Yavsc.Helpers.Google
{
	/// <summary>
	/// Google helpers.
	/// </summary>
	public static class GoogleHelpers
	{
		/// <summary>
		/// Gets the events.
		/// </summary>
		/// <returns>The events.</returns>
		/// <param name="profile">Profile.</param>
		/// <param name="mindate">Mindate.</param>
		/// <param name="maxdate">Maxdate.</param>
		public static CalendarEventList GetEvents(this ProfileBase profile, DateTime mindate, DateTime maxdate)
		{
			string gcalid = (string) profile.GetPropertyValue ("gcalid");
			if (string.IsNullOrWhiteSpace (gcalid))
				throw new ArgumentException ("NULL gcalid");
			CalendarApi c = new CalendarApi (clientApiKey);
			string creds = OAuth2.GetFreshGoogleCredential (profile);
			return c.GetCalendar (gcalid, mindate, maxdate, creds);
		}
		/// <summary>
		/// Gets the calendars.
		/// </summary>
		/// <returns>The calendars.</returns>
		/// <param name="profile">Profile.</param>
		public static CalendarList GetCalendars (this ProfileBase profile)
		{
			string cred = OAuth2.GetFreshGoogleCredential (profile);
			CalendarApi c = new CalendarApi (clientApiKey);
			return c.GetCalendars (cred);
		}

		private static string clientId = ConfigurationManager.AppSettings ["GOOGLE_CLIENT_ID"];
		private static string clientSecret = ConfigurationManager.AppSettings ["GOOGLE_CLIENT_SECRET"];
		private static string clientApiKey = ConfigurationManager.AppSettings ["GOOGLE_API_KEY"];
		/// <summary>
		/// Login the specified response, state and callBack.
		/// </summary>
		/// <param name="response">Response.</param>
		/// <param name="state">State.</param>
		/// <param name="callBack">Call back.</param>
		public static void Login(this HttpResponseBase response, string state, string callBack)
		{
			OAuth2 oa = new OAuth2 (callBack, clientId,clientSecret);
			oa.Login (response, state);
		}
		/// <summary>
		/// Cals the login.
		/// </summary>
		/// <param name="response">Response.</param>
		/// <param name="state">State.</param>
		/// <param name="callBack">Call back.</param>
		public static void CalLogin(this HttpResponseBase response, string state, string callBack)
		{
			OAuth2 oa = new OAuth2 (callBack,clientId,clientSecret);
			oa.GetCalendarScope (response, state);
		}
		/// <summary>
		/// Creates the O auth2.
		/// </summary>
		/// <returns>The O auth2.</returns>
		/// <param name="callBack">Call back.</param>
		public static OAuth2 CreateOAuth2(string callBack)
		{
			return new OAuth2 (callBack,clientId,clientSecret);
		}

		/// <summary>
		/// Notifies the event.
		/// </summary>
		/// <returns>The event.</returns>
		/// <param name="evpub">Evpub.</param>
		public static MessageWithPayloadResponse NotifyEvent(EventPub evpub) {
			using (SimpleJsonPostMethod<MessageWithPayload<YaEvent>,MessageWithPayloadResponse> r = 
				new SimpleJsonPostMethod<MessageWithPayload<YaEvent>,MessageWithPayloadResponse>(
					"https://gcm-http.googleapis.com/gcm/send")) { 
				var users  = Circle.Union (evpub.CircleIds);
				var regids = new List<string> ();
				var to = new List<string> ();
				foreach (var u in users) {
					var p = ProfileBase.Create (u);
					if (p != null) {
						var regid = p.GetPropertyValue("gregid");
						if (regid == null) {
							var muser = Membership.GetUser (u);
							to.Add (muser.Email);
						}
						else regids.Add ((string)regid);
					}
				}
				if (regids.Count == 0)
					throw new InvalidOperationException 
					("No recipient where found for this circle list");
				
				var msg = new MessageWithPayload<YaEvent> () { 
					notification = new Notification() { title = evpub.Title, body = evpub.Description, icon = "event" },
					data = new YaEvent[] { (YaEvent)evpub }, registration_ids = regids.ToArray() };
				return r.Invoke (msg);
			}
		}

	}
}

