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
using Yavsc.Model;

namespace Yavsc.Helpers.Google
{
	/// <summary>
	/// Google Calendar API client.
	/// </summary>
	public class CalendarApi : ApiClient 
	{
		public CalendarApi(string apiKey)
		{
			API_KEY = apiKey;
		}
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
		public CalendarList GetCalendars (string cred)
		{
			CalendarList res = null;
			HttpWebRequest webreq = WebRequest.CreateHttp (getCalListUri);
			webreq.Headers.Add (HttpRequestHeader.Authorization, cred);
			webreq.Method = "GET";
			webreq.ContentType = "application/http";
			using (WebResponse resp = webreq.GetResponse ()) {
				using (Stream respstream = resp.GetResponseStream ()) {
					var cr = new Newtonsoft.Json.JsonSerializer ();
					using (var rdr = new StreamReader (respstream))
						res = (CalendarList) cr.Deserialize (rdr, typeof(CalendarList));
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
		public CalendarEventList GetCalendar  (string calid, DateTime mindate, DateTime maxdate,string cred)
		{
			if (string.IsNullOrWhiteSpace (calid))
				throw new Exception ("the calendar identifier is not specified");
			
			string uri = string.Format (
				getCalEntriesUri, HttpUtility.UrlEncode (calid)) +
				string.Format ("?orderBy=startTime&singleEvents=true&timeMin={0}&timeMax={1}&key=" + API_KEY,
					HttpUtility.UrlEncode (mindate.ToString (dateFormat) + timeZone), 
					HttpUtility.UrlEncode (maxdate.ToString (dateFormat) + timeZone));

			HttpWebRequest webreq = WebRequest.CreateHttp (uri);
			var cr = new Newtonsoft.Json.JsonSerializer ();
			webreq.Headers.Add (HttpRequestHeader.Authorization, cred);
			webreq.Method = "GET";
			webreq.ContentType = "application/http";
			CalendarEventList res = null;
			try {
				using (WebResponse resp = webreq.GetResponse ()) {
					using (Stream respstream = resp.GetResponseStream ()) {
						try {
							using (var rdr = new StreamReader(respstream)) {
							res = (CalendarEventList) 
									cr.Deserialize(rdr,typeof(CalendarEventList));
							}
						} catch (Exception ) {
							respstream.Close ();
							resp.Close ();
							webreq.Abort ();
							throw ;
						}
					}
					resp.Close ();
				}
			} catch (WebException ) {
				webreq.Abort ();
				throw;
			}
			webreq.Abort ();
			return res;
		}

	}
}