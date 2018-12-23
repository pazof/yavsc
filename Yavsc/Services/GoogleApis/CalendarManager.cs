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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Services;
using System.Threading;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;

namespace Yavsc.Services
{
    using Yavsc.Helpers;
    using Yavsc.Models;
    using Yavsc.Models.Calendar;
    using Yavsc.Server.Helpers;
    using Yavsc.ViewModels.Calendar;

    /// <summary>
    /// Google Calendar API client.
    /// </summary>
    public class CalendarManager : ICalendarManager 
    {
        public class ExpiredTokenException : Exception { }
        protected static string [] scopesCalendar = 
        { "https://www.googleapis.com/auth/calendar",
          "https://www.googleapis.com/auth/calendar.events"
        };
        private string _ApiKey;
        private IAuthorizationCodeFlow _flow;
        ApplicationDbContext _dbContext;

        IDataStore _dataStore;
        ILogger _logger;

        GoogleAuthSettings _googleSettings ;

        public CalendarManager(IOptions<GoogleAuthSettings> settings,
        ApplicationDbContext dbContext,
        IDataStore dataStore,
        ILoggerFactory loggerFactory,
        IOptions<GoogleAuthSettings> googleSettingsOptions)
        {
            _ApiKey = settings.Value.ApiKey;
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<CalendarManager>();
            _dataStore = dataStore;
            _googleSettings = googleSettingsOptions.Value;
            _flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _googleSettings.ServiceAccount.client_id,
                        ClientSecret = _googleSettings.ServiceAccount.client_secret
                    },
                    Scopes = scopesCalendar ,
                    DataStore = dataStore
                });
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
        /// Gets the calendar list.
        /// </summary>
        /// <returns>The calendars.</returns>
        /// <param name="userId">Yavsc user id</param>
        public async Task<CalendarList> GetCalendarsAsync(string userId, string pageToken)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("the user id is not specified");
			var service = await CreateUserCalendarServiceAsync(userId);
#if Debug
      if (service==null) throw new Exception("Could not get service");
#endif
      _logger.LogInformation("Got a service");

#if Debug
      if (service.CalendarList==null) throw new Exception("Could not get calendar list");
#endif
			CalendarListResource.ListRequest calListReq = service.CalendarList.List ();

#if Debug
      if (calListReq==null) throw new Exception ("list is null");
#endif

      calListReq.PageToken = pageToken;
			return calListReq.Execute ();
        }
        
        /// <summary>
        /// Gets a calendar event list, between the given dates.
        /// </summary>
        /// <returns>The calendar.</returns>
        /// <param name="calid">Calendar identifier.</param>
        /// <param name="mindate">Mindate.</param>
        /// <param name="maxdate">Maxdate.</param>
        /// <param name="cred">credential string.</param>
        public async Task<Events> GetCalendarAsync(string calid, DateTime minDate, DateTime maxDate, string pageToken)
        {
            var service = await GetServiceAsync();

            var listRequest = service.Events.List(calid);
            listRequest.PageToken = pageToken;
            listRequest.TimeMin = minDate;
            listRequest.TimeMax = maxDate;
            listRequest.SingleEvents = true;
            return await listRequest.ExecuteAsync();
        }
        public async Task<DateTimeChooserViewModel> CreateViewModelAsync(
            string inputId,
            string calid, DateTime mindate, DateTime maxdate)
        {
            if (calid == null)
                return new DateTimeChooserViewModel
                {
                    InputId = inputId,
                    MinDate = mindate,
                    MaxDate = maxdate
                };

            var eventList = await GetCalendarAsync(calid, mindate, maxdate, null);
            List<Period> free = new List<Period>();
            List<Period> busy = new List<Period>();

            foreach (var ev in eventList.Items)
            {
                if (ev.Start.DateTime.HasValue && ev.End.DateTime.HasValue ) {
                DateTime start = ev.Start.DateTime.Value;
                DateTime end = ev.End.DateTime.Value;

                if (ev.Transparency == "transparent")
                {

                    free.Add(new Period { Start = start, End = end });
                }
                else busy.Add(new Period { Start = start, End = end });
                }
            }

            return new DateTimeChooserViewModel
            {
                InputId = inputId,
                MinDate = mindate,
                MaxDate = maxdate,
                Free = free.ToArray(),
                Busy = busy.ToArray(),
                FreeDates = free.SelectMany(p => new string[] { p.Start.ToString("dd/MM/yyyy HH:mm"), p.End.ToString("dd/MM/yyyy HH:mm") }).Distinct().ToArray(),
                BusyDates = busy.SelectMany(p => new string[] { p.Start.ToString("dd/MM/yyyy HH:mm"), p.End.ToString("dd/MM/yyyy HH:mm") }).Distinct().ToArray()
            };
        }

        /// <summary>
        /// Creates a event in a calendar
        /// <c>calendar.events.insert</c>
        /// </summary>
        /// <param name="calid"></param>
        /// <param name="startDate"></param>
        /// <param name="lengthInSeconds"></param>
        /// <param name="summary"></param>
        /// <param name="description"></param>
        /// <param name="location"></param>
        /// <param name="available"></param>
        /// <returns></returns>
        public async Task<Event> CreateEventAsync(string userId, string calid, DateTime startDate, int lengthInSeconds, string summary, string description, string location, bool available)
        {

            if (string.IsNullOrWhiteSpace(calid))
                throw new Exception("the calendar identifier is not specified");
            
            var service = await GetServiceAsync();
            Event ev = new Event
            {
                Start = new EventDateTime { DateTime = startDate },
                End = new EventDateTime { DateTime = startDate.AddSeconds(lengthInSeconds) },
                Summary = summary,
                Description = description
            };
            var insert = service.Events.Insert(ev, calid);
            var inserted = await insert.ExecuteAsync();

            return inserted;
        }
        CalendarService _service = null;
        public async Task<CalendarService> GetServiceAsync()
        {
            if (_service==null) {
                GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();
                var baseClientService = new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential
                    };
                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped( scopesCalendar );
                }/* 
                var credential = await GoogleHelpers.GetCredentialForApi(new string [] { scopeCalendar });
                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped(scopeCalendar);
                }
                _service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Yavsc"
                });
                }*/
                _service = new CalendarService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Yavsc"
                    });
            }
            return _service;
        }

		/// <summary>
		/// Creates Google User Credential
		/// </summary>
		/// <param name="userId">Yavsc use id</param>
		/// <returns></returns>
        public async Task<CalendarService> CreateUserCalendarServiceAsync(string userId)
        {
            GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();

            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped( scopesCalendar);
            }


            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "yavsc-001"
            });

			return service; 
		}

        public async Task<TokenResponse> RefreshToken(TokenResponse oldResponse)
        {
            string ep = " https://www.googleapis.com/oauth2/v4/token";
            _logger.LogInformation($"rt:{oldResponse.RefreshToken}");
            // refresh_token client_id client_secret grant_type=refresh_token
            try {
                using (var m = new SimpleJsonPostMethod(ep)) {
                    return await m.Invoke<TokenResponse>(
                        new { refresh_token= oldResponse.RefreshToken, client_id=_googleSettings.ServiceAccount.client_id,
                         client_secret=_googleSettings.ServiceAccount.client_secret,
                          grant_type="refresh_token" }
                    );
                }
            }
            catch (Exception ex) {
                throw new Exception ("Quelque chose s'est mal passé à l'envoi",ex);
            }
        }
    }
}
