﻿//
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
using System.Web.Mvc;
using Yavsc.Client.Events;
using Yavsc.Client.Messaging;
using Yavsc.Helpers.Google.Api;
using Yavsc.Model;

namespace Yavsc.Helpers
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
			CalendarApi c = new CalendarApi ();
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
			CalendarApi c = new CalendarApi ();
			return c.GetCalendars (cred);
		}

		/// <summary>
		/// Login the specified response, state and callBack.
		/// </summary>
		/// <param name="response">Response.</param>
		/// <param name="state">State.</param>
		/// <param name="callBack">Call back.</param>
		public static void Login(this HttpResponseBase response, string state, string callBack)
		{
			OAuth2 oa = new OAuth2 (callBack);
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
			OAuth2 oa = new OAuth2 (callBack);
			oa.GetCalendarScope (response, state);
		}
		/// <summary>
		/// Creates the O auth2.
		/// </summary>
		/// <returns>The O auth2.</returns>
		/// <param name="callBack">Call back.</param>
		public static Yavsc.Helpers.Google.Api.OAuth2 CreateOAuth2(string callBack)
		{
			return new OAuth2 (callBack);
		}

		/// <summary>
		/// Notifies the event.
		/// </summary>
		/// <returns>The event.</returns>
		/// <param name="evpub">Evpub.</param>
		public static MessageWithPayloadResponse NotifyEvent(EventCirclesPub evpub) {
			using (var r = 
				new SimpleJsonPostMethod<MessageWithPayload<YaEvent>,MessageWithPayloadResponse>(
					"https://gcm-http.googleapis.com/gcm/send")) { 
				r.SetCredential (ConfigurationManager.AppSettings ["GOOGLE_GCM_API_KEY"]);
				if (evpub.CircleIds != null) {
					var users = Circle.Union (evpub.CircleIds);
					var regids = new List<string> ();
					var to = new List<string> ();
					foreach (var u in users) {
						var p = ProfileBase.Create (u);
						if (p != null) {
							var regid = p.GetPropertyValue ("gregid");
							if (regid == null) {
								var muser = Membership.GetUser (u);
								to.Add (muser.Email);
							} else
								regids.Add ((string)regid);
						}
					}
					if (regids.Count == 0)
						throw new InvalidOperationException 
						("No recipient where found for this circle list");
					
					var msg = new MessageWithPayload<YaEvent> () { 
						notification = new Notification() { title = evpub.Title, body = evpub.Description, icon = "event" },
						data = evpub , registration_ids = regids.ToArray() };
					return r.Invoke (msg);

				} else {
					throw new NotImplementedException ();
				}
			}
		}
		/// <summary>
		/// Notifies the event.
		/// </summary>
		/// <returns>The event.</returns>
		/// <param name="evpub">Evpub.</param>
		public static MessageWithPayloadResponse NotifyEvent (NominativeEventPub evpub)
		{
			MessageWithPayloadResponse result = null;
			using (var r = 
				new SimpleJsonPostMethod<MessageWithPayload<NominativeEventPub>,MessageWithPayloadResponse> (
					       "https://gcm-http.googleapis.com/gcm/send")) { 
				r.SetCredential ("key="+ConfigurationManager.AppSettings ["GOOGLE_API_KEY"]);
				var userprofile = ProfileBase.Create (evpub.PerformerName);
				var regid = userprofile.GetPropertyValue ("gregid") as string;
				if (regid == null)
					throw new NotImplementedException ("Notification via e-mail");
				var msg = new MessageWithPayload<NominativeEventPub> () { 
					notification = new Notification() { title = evpub.Title, 
						body = evpub.Description + 
							evpub.Comment==null?
							"":"("+evpub.Comment+")", icon = "icon" },
					data = evpub, registration_ids = new string[] { regid }  };
				result = r.Invoke (msg);
				if (result != null)
				if (result.success > 0)
				if (result.results.Length > 0)
				if (result.results[0].registration_id != null) {
					// update the registration id in db
					userprofile.SetPropertyValue("gregid",result.results [0].registration_id);
					userprofile.Save ();
				}
			}
			return result;
		}

		/// <summary>
		/// Validate the specified modelState.
		/// </summary>
		/// <param name="modelState">Model state.</param>
		public static void Validate<T>(this HtmlHelper helper, MessageWithPayload<T> msg) {
			var modelState = helper.ViewData.ModelState ;
			if (msg.to==null && msg.registration_ids == null) {
				modelState.AddModelError ("to", "One of \"to\" or \"registration_ids\" parameters must be specified");
				modelState.AddModelError ("registration_ids", "*");
				modelState.AddModelError ("to", "*");
			}
			if (msg.notification == null && msg.data == null) {
				modelState.AddModelError ("notification", "At least one of \"notification\" or \"data\" parameters must be specified");
				modelState.AddModelError ("data", "*");
			}
			if (msg.notification != null) {
				if (msg.notification.icon == null)
					modelState.AddModelError ("notification.icon", "please, specify an icon resoure name");
				if (msg.notification.title == null)
					modelState.AddModelError ("notification.title", "please, specify a title");
			}
		}

	}
}
