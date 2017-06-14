//
//  CalendarApi.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 - 2017 Paul Schneider
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
using System.Net;
using System.IO;
using System.Web;
using Yavsc.Models.Auth;
using Newtonsoft.Json;

namespace Yavsc.Models.Google.Calendar
{
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.OptionsModel;
    using Models.Google;
    using Yavsc.Helpers;
    using Yavsc.Models.Calendar;
    using Yavsc.ViewModels.Calendar;



    /// <summary>
    /// Google Calendar API client.
    /// </summary>
    public class CalendarManager : ICalendarManager
	{
        // protected static string scopeCalendar = "https://www.googleapis.com/auth/calendar";
        private string _ApiKey;

		private readonly UserManager<ApplicationUser> _userManager;

		ApplicationDbContext _dbContext;
		ILogger _logger;

		public CalendarManager(IOptions<GoogleAuthSettings> settings,
		UserManager<ApplicationUser> userManager,
		ApplicationDbContext dbContext,
		ILoggerFactory loggerFactory)
		{
            _ApiKey = settings.Value.ApiKey;
			_userManager = userManager;
			_dbContext = dbContext;
			_logger = loggerFactory.CreateLogger<CalendarManager>();
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
		public async Task<CalendarList> GetCalendarsAsync (string userId)
		{
			UserCredential creds = await _userManager.GetCredentialForGoogleApiAsync(
                _dbContext, userId);
            if (creds==null)
              throw new InvalidOperationException("No credential");
			CalendarList res = null;
			HttpWebRequest webreq = WebRequest.CreateHttp (getCalListUri);
			webreq.Headers.Add (HttpRequestHeader.Authorization, creds.GetHeader());
			webreq.Method = "GET";
			webreq.ContentType = "application/http";
			using (WebResponse resp = webreq.GetResponse ()) {
				using (Stream respstream = resp.GetResponseStream ()) {
					using (var rdr = new StreamReader(respstream)) {
						res = JsonConvert.DeserializeObject<CalendarList>(rdr.ReadToEnd());
					}
				}
				resp.Close ();
			}
			webreq.Abort ();
			return res;
		}


		/// <summary>
		/// Gets a calendar event list, between the given dates.
		/// </summary>
		/// <returns>The calendar.</returns>
		/// <param name="calid">Calendar identifier.</param>
		/// <param name="mindate">Mindate.</param>
		/// <param name="maxdate">Maxdate.</param>
		/// <param name="cred">credential string.</param>
		public async Task<CalendarEventList> GetCalendarAsync  (string calid, DateTime mindate, DateTime maxdate,string userId)
		{
			UserCredential creds = await _userManager.GetCredentialForGoogleApiAsync(
                _dbContext, userId);
            if (creds==null)
              throw new InvalidOperationException("No credential");
			if (string.IsNullOrWhiteSpace (calid))
				throw new Exception ("the calendar identifier is not specified");

			string uri = string.Format (
				getCalEntriesUri, HttpUtility.UrlEncode (calid)) +
				string.Format ("?orderBy=startTime&singleEvents=true&timeMin={0}&timeMax={1}&key=" + _ApiKey,
					HttpUtility.UrlEncode (mindate.ToString (dateFormat) + timeZone),
					HttpUtility.UrlEncode (maxdate.ToString (dateFormat) + timeZone));

			HttpWebRequest webreq = WebRequest.CreateHttp (uri);

			webreq.Headers.Add (HttpRequestHeader.Authorization, creds.GetHeader());
			webreq.Method = "GET";
			webreq.ContentType = "application/http";
			CalendarEventList res = null;
			try {
				using (WebResponse resp = await webreq.GetResponseAsync ()) {
					using (Stream respstream = resp.GetResponseStream ()) {
						try {
							using (var rdr = new StreamReader(respstream)) {
								string json = rdr.ReadToEnd();
								_logger.LogVerbose(">> Calendar: "+json);
								res= JsonConvert.DeserializeObject<CalendarEventList>(json);
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
		public async Task<DateTimeChooserViewModel> CreateViewModel(
			string inputId,
			string calid, DateTime mindate, DateTime maxdate, string userId)
		{
			var eventList = await GetCalendarAsync(calid, mindate, maxdate, userId);
			
			return new DateTimeChooserViewModel {
				InputId = inputId,
				MinDate  = mindate,
				MaxDate = maxdate,
				DisabledTimeIntervals = null
			};
		}
	}
}
