//
//  Calendar.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
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
using Yavsc.Helpers;
using System.Web.Profile;
using Yavsc.Model.Google;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Web;

namespace Yavsc.Helpers.Google
{
	/// <summary>
	/// Google Calendar API client.
	/// </summary>
	public class CalendarApi: ApiClient 
	{
		/// <summary>
		/// The get cal list URI.
		/// </summary>
		protected static string getCalListUri = "https://www.googleapis.com/calendar/v3/users/me/calendarList";
		/// <summary>
		/// The get cal entries URI.
		/// </summary>
		protected static string getCalEntriesUri = "https://www.googleapis.com/calendar/v3/calendars/{0}/events";

		/// <summary>
		/// The date format.
		/// </summary>
		private static string dateFormat = "yyyy-MM-ddTHH:mm:ss";

		/// <summary>
		/// The time zone. TODO Fixme with machine time zone
		/// </summary>
		private string timeZone = "+01:00";

		/// <summary>
		/// Gets the calendar list.
		/// </summary>
		/// <returns>The calendars.</returns>
		/// <param name="cred">Cred.</param>
		/// <param name="json">Json.</param>
		public CalendarList GetCalendars (string cred, out string json)
		{
			CalendarList res = null;
			HttpWebRequest webreq = WebRequest.CreateHttp (getCalListUri);
			webreq.Headers.Add (HttpRequestHeader.Authorization, cred);
			webreq.Method = "GET";
			webreq.ContentType = "application/http";
			using (WebResponse resp = webreq.GetResponse ()) {
				using (Stream respstream = resp.GetResponseStream ()) {
					using (StreamReader readresp = new StreamReader (respstream, Encoding.UTF8)) {
						json = readresp.ReadToEnd ();
						res = JsonConvert.DeserializeObject<CalendarList> (json);
					}
				}
				resp.Close ();
			}
			webreq.Abort ();
			return res;
		}

		/// <summary>
		/// Gets a calendar.
		/// </summary>
		/// <returns>The calendar.</returns>
		/// <param name="calid">Calid.</param>
		/// <param name="mindate">Mindate.</param>
		/// <param name="maxdate">Maxdate.</param>
		/// <param name="upr">Upr.</param>
		/// <param name="responseStr">Response string.</param>
		public CalendarEntryList GetCalendar  (string calid, DateTime mindate, DateTime maxdate, ProfileBase upr,  out string responseStr)
		{
			string uri = string.Format (
				getCalEntriesUri, HttpUtility.UrlEncode (calid)) +
				string.Format ("?orderBy=startTime&singleEvents=true&timeMin={0}&timeMax={1}&key=" + API_KEY,
					HttpUtility.UrlEncode (mindate.ToString (dateFormat) + timeZone), 
					HttpUtility.UrlEncode (maxdate.ToString (dateFormat) + timeZone));

			HttpWebRequest webreq = WebRequest.CreateHttp (uri);
			string cred = OAuth2.GetFreshGoogleCredential (upr);
			webreq.Headers.Add (HttpRequestHeader.Authorization, cred);
			webreq.Method = "GET";
			webreq.ContentType = "application/http";
			CalendarEntryList res = null;
			try {
				using (WebResponse resp = webreq.GetResponse ()) {
					using (Stream respstream = resp.GetResponseStream ()) {
						using (StreamReader readresp = new StreamReader (respstream, Encoding.UTF8)) {
							 responseStr = readresp.ReadToEnd ();
							try {
								res = JsonConvert.DeserializeObject<CalendarEntryList> (responseStr);
							} catch (JsonReaderException ex) {
								respstream.Close ();
								resp.Close ();
								webreq.Abort ();
								throw new GoogleErrorException(ex,responseStr);
						}
						}
						respstream.Close ();
					}
					resp.Close ();
				}
			} catch (WebException ex) {
				webreq.Abort ();
				throw new GoogleErrorException (ex);
			}
			webreq.Abort ();
				return res;
		}
	}
	
}
