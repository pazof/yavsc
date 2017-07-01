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
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace Yavsc.Models.Google.Calendar
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Models.Google;
    using Newtonsoft.Json.Linq;
    using Yavsc.Helpers;
    using Yavsc.Models.Calendar;
    using Yavsc.ViewModels.Calendar;

    /// <summary>
    /// Google Calendar API client.
    /// </summary>
    public class CalendarManager : ICalendarManager
	{
        protected static string scopeCalendar = "https://www.googleapis.com/auth/calendar";
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

        private readonly IDataStore dataStore = new FileDataStore(GoogleWebAuthorizationBroker.Folder);



		/// <summary>
		/// Gets the calendar list.
		/// </summary>
		/// <returns>The calendars.</returns>
		/// <param name="userId">Yavsc user id</param>
		public async Task<CalendarList> GetCalendarsAsync (string userId)
		{
			
			CalendarList res = null;
			var login = await _userManager.GetGoogleUserLoginAsync(_dbContext,userId);
			var token = await _dbContext.GetTokensAsync(login.ProviderKey);
            if (token==null)
              throw new InvalidOperationException("No Google token");

			HttpWebRequest webreq = WebRequest.CreateHttp(getCalListUri);
			webreq.Headers.Add("Authorization", "Bearer "+ token.AccessToken);
			webreq.Method = "GET";
			webreq.ContentType = "application/http";
			using (WebResponse resp = webreq.GetResponse ()) {
				using (Stream respstream = resp.GetResponseStream ()) {
					using (var rdr = new StreamReader(respstream)) {
						string json = rdr.ReadToEnd();
						_logger.LogInformation(">> Json calendar list : "+json);
						res = JsonConvert.DeserializeObject<CalendarList>(json);
					}
				}
				resp.Close ();
			}
			webreq.Abort ();
			return res;
		}


		private ServiceAccountCredential GetServiceAccountCredential()
		{
			var creds = GoogleHelpers.GetCredentialForApi(new string[]{scopeCalendar});
            if (creds==null)
              throw new InvalidOperationException("No credential");

			return creds;
		}
		/// <summary>
		/// Gets a calendar event list, between the given dates.
		/// </summary>
		/// <returns>The calendar.</returns>
		/// <param name="calid">Calendar identifier.</param>
		/// <param name="mindate">Mindate.</param>
		/// <param name="maxdate">Maxdate.</param>
		/// <param name="cred">credential string.</param>
		public async Task<CalendarEventList> GetCalendarAsync  (string calid, DateTime mindate, DateTime maxdate)
		{
			if (string.IsNullOrWhiteSpace (calid))
				throw new Exception ("the calendar identifier is not specified");
			
			var creds = GetServiceAccountCredential();
			string uri = string.Format (
				getCalEntriesUri, HttpUtility.UrlEncode (calid)) +
				string.Format ("?orderBy=startTime&singleEvents=true&timeMin={0}&timeMax={1}&key=" + _ApiKey,
					HttpUtility.UrlEncode (mindate.ToString (dateFormat) + timeZone),
					HttpUtility.UrlEncode (maxdate.ToString (dateFormat) + timeZone));

			HttpWebRequest webreq = WebRequest.CreateHttp (uri);

			webreq.Headers.Add (HttpRequestHeader.Authorization, "Bearer "+ await creds.GetAccessTokenForRequestAsync());
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
								res = JsonConvert.DeserializeObject<CalendarEventList>(json);
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
		public async Task<DateTimeChooserViewModel> CreateViewModelAsync(
			string inputId,
			string calid, DateTime mindate, DateTime maxdate)
		{
			if (calid ==null) return new DateTimeChooserViewModel {
				InputId = inputId,
				MinDate  = mindate,
				MaxDate = maxdate
			};

			var eventList = await GetCalendarAsync(calid, mindate, maxdate);
			List<Period> free = new List<Period> ();
            List<Period> busy = new List<Period> ();
            
            foreach (var ev in eventList.items)
            {
                if (ev.transparency == "transparent" )
                {
                    free.Add(new Period { Start =  ev.start.datetime, End = ev.end.datetime });
                }
                else busy.Add(new Period { Start =  ev.start.datetime, End = ev.end.datetime });
            }

			return new DateTimeChooserViewModel {
				InputId = inputId,
				MinDate  = mindate,
				MaxDate = maxdate,
				Free = free.ToArray(),
				Busy = busy.ToArray(),
				FreeDates = free.SelectMany( p => new string [] { p.Start.ToString("DD/mm/yyyy"), p.End.ToString("DD/mm/yyyy") }).Distinct().ToArray(),
				BusyDates = busy.SelectMany( p => new string [] { p.Start.ToString("DD/mm/yyyy"), p.End.ToString("DD/mm/yyyy") }).Distinct().ToArray()
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
        public async Task<Resource> CreateResourceAsync(string calid, DateTime startDate, int lengthInSeconds, string summary, string description, string location, bool available)
        {	

			/* "insert": {
     "id": "calendar.events.insert",
     "path": "calendars/{calendarId}/events",
     "httpMethod": "POST",
     "description": "Creates an event.",
     "parameters": {
      "calendarId": {
       "type": "string",
       "description": "Calendar identifier. To retrieve calendar IDs call the calendarList.list method. If you want to access the primary calendar of the currently logged in user, use the \"primary\" keyword.",
       "required": true,
       "location": "path"
      },
      "maxAttendees": {
       "type": "integer",
       "description": "The maximum number of attendees to include in the response. If there are more than the specified number of attendees, only the participant is returned. Optional.",
       "format": "int32",
       "minimum": "1",
       "location": "query"
      },
      "sendNotifications": {
       "type": "boolean",
       "description": "Whether to send notifications about the creation of the new event. Optional. The default is False.",
       "location": "query"
      },
      "supportsAttachments": {
       "type": "boolean",
       "description": "Whether API client performing operation supports event attachments. Optional. The default is False.",
       "location": "query"
      }
     },
     "parameterOrder": [
      "calendarId"
     ],
     "request": {
      "$ref": "Event"
     },
     "response": {
      "$ref": "Event"
     },
     "scopes": [
      "https://www.googleapis.com/auth/calendar"
     ]
    },  	
	
	  "Event": {
   "id": "Event",
   "type": "object",
   "properties": {
    "anyoneCanAddSelf": {
     "type": "boolean",
     "description": "Whether anyone can invite themselves to the event (currently works for Google+ events only). Optional. The default is False.",
     "default": "false"
    },
    "attachments": {
     "type": "array",
     "description": "File attachments for the event. Currently only Google Drive attachments are supported.\nIn order to modify attachments the supportsAttachments request parameter should be set to true.\nThere can be at most 25 attachments per event,",
     "items": {
      "$ref": "EventAttachment"
     }
    },
    "attendees": {
     "type": "array",
     "description": "The attendees of the event. See the Events with attendees guide for more information on scheduling events with other calendar users.",
     "items": {
      "$ref": "EventAttendee"
     }
    },
    "attendeesOmitted": {
     "type": "boolean",
     "description": "Whether attendees may have been omitted from the event's representation. When retrieving an event, this may be due to a restriction specified by the maxAttendee query parameter. When updating an event, this can be used to only update the participant's response. Optional. The default is False.",
     "default": "false"
    },
    "colorId": {
     "type": "string",
     "description": "The color of the event. This is an ID referring to an entry in the event section of the colors definition (see the  colors endpoint). Optional."
    },
    "created": {
     "type": "string",
     "description": "Creation time of the event (as a RFC3339 timestamp). Read-only.",
     "format": "date-time"
    },
    "creator": {
     "type": "object",
     "description": "The creator of the event. Read-only.",
     "properties": {
      "displayName": {
       "type": "string",
       "description": "The creator's name, if available."
      },
      "email": {
       "type": "string",
       "description": "The creator's email address, if available."
      },
      "id": {
       "type": "string",
       "description": "The creator's Profile ID, if available. It corresponds to theid field in the People collection of the Google+ API"
      },
      "self": {
       "type": "boolean",
       "description": "Whether the creator corresponds to the calendar on which this copy of the event appears. Read-only. The default is False.",
       "default": "false"
      }
     }
    },
    "description": {
     "type": "string",
     "description": "Description of the event. Optional."
    },
    "end": {
     "$ref": "EventDateTime",
     "description": "The (exclusive) end time of the event. For a recurring event, this is the end time of the first instance.",
     "annotations": {
      "required": [
       "calendar.events.import",
       "calendar.events.insert",
       "calendar.events.update"
      ]
     }
    },
    "endTimeUnspecified": {
     "type": "boolean",
     "description": "Whether the end time is actually unspecified. An end time is still provided for compatibility reasons, even if this attribute is set to True. The default is False.",
     "default": "false"
    },
    "etag": {
     "type": "string",
     "description": "ETag of the resource."
    },
    "extendedProperties": {
     "type": "object",
     "description": "Extended properties of the event.",
     "properties": {
      "private": {
       "type": "object",
       "description": "Properties that are private to the copy of the event that appears on this calendar.",
       "additionalProperties": {
        "type": "string",
        "description": "The name of the private property and the corresponding value."
       }
      },
      "shared": {
       "type": "object",
       "description": "Properties that are shared between copies of the event on other attendees' calendars.",
       "additionalProperties": {
        "type": "string",
        "description": "The name of the shared property and the corresponding value."
       }
      }
     }
    },
    "gadget": {
     "type": "object",
     "description": "A gadget that extends this event.",
     "properties": {
      "display": {
       "type": "string",
       "description": "The gadget's display mode. Optional. Possible values are:  \n- \"icon\" - The gadget displays next to the event's title in the calendar view. \n- \"chip\" - The gadget displays when the event is clicked."
      },
      "height": {
       "type": "integer",
       "description": "The gadget's height in pixels. The height must be an integer greater than 0. Optional.",
       "format": "int32"
      },
      "iconLink": {
       "type": "string",
       "description": "The gadget's icon URL. The URL scheme must be HTTPS."
      },
      "link": {
       "type": "string",
       "description": "The gadget's URL. The URL scheme must be HTTPS."
      },
      "preferences": {
       "type": "object",
       "description": "Preferences.",
       "additionalProperties": {
        "type": "string",
        "description": "The preference name and corresponding value."
       }
      },
      "title": {
       "type": "string",
       "description": "The gadget's title."
      },
      "type": {
       "type": "string",
       "description": "The gadget's type."
      },
      "width": {
       "type": "integer",
       "description": "The gadget's width in pixels. The width must be an integer greater than 0. Optional.",
       "format": "int32"
      }
     }
    },
    "guestsCanInviteOthers": {
     "type": "boolean",
     "description": "Whether attendees other than the organizer can invite others to the event. Optional. The default is True.",
     "default": "true"
    },
    "guestsCanModify": {
     "type": "boolean",
     "description": "Whether attendees other than the organizer can modify the event. Optional. The default is False.",
     "default": "false"
    },
    "guestsCanSeeOtherGuests": {
     "type": "boolean",
     "description": "Whether attendees other than the organizer can see who the event's attendees are. Optional. The default is True.",
     "default": "true"
    },
    "hangoutLink": {
     "type": "string",
     "description": "An absolute link to the Google+ hangout associated with this event. Read-only."
    },
    "htmlLink": {
     "type": "string",
     "description": "An absolute link to this event in the Google Calendar Web UI. Read-only."
    },
    "iCalUID": {
     "type": "string",
     "description": "Event unique identifier as defined in RFC5545. It is used to uniquely identify events accross calendaring systems and must be supplied when importing events via the import method.\nNote that the icalUID and the id are not identical and only one of them should be supplied at event creation time. One difference in their semantics is that in recurring events, all occurrences of one event have different ids while they all share the same icalUIDs.",
     "annotations": {
      "required": [
       "calendar.events.import"
      ]
     }
    },
    "id": {
     "type": "string",
     "description": "Opaque identifier of the event. When creating new single or recurring events, you can specify their IDs. Provided IDs must follow these rules:  \n- characters allowed in the ID are those used in base32hex encoding, i.e. lowercase letters a-v and digits 0-9, see section 3.1.2 in RFC2938 \n- the length of the ID must be between 5 and 1024 characters \n- the ID must be unique per calendar  Due to the globally distributed nature of the system, we cannot guarantee that ID collisions will be detected at event creation time. To minimize the risk of collisions we recommend using an established UUID algorithm such as one described in RFC4122.\nIf you do not specify an ID, it will be automatically generated by the server.\nNote that the icalUID and the id are not identical and only one of them should be supplied at event creation time. One difference in their semantics is that in recurring events, all occurrences of one event have different ids while they all share the same icalUIDs."
    },
    "kind": {
     "type": "string",
     "description": "Type of the resource (\"calendar#event\").",
     "default": "calendar#event"
    },
    "location": {
     "type": "string",
     "description": "Geographic location of the event as free-form text. Optional."
    },
    "locked": {
     "type": "boolean",
     "description": "Whether this is a locked event copy where no changes can be made to the main event fields \"summary\", \"description\", \"location\", \"start\", \"end\" or \"recurrence\". The default is False. Read-Only.",
     "default": "false"
    },
    "organizer": {
     "type": "object",
     "description": "The organizer of the event. If the organizer is also an attendee, this is indicated with a separate entry in attendees with the organizer field set to True. To change the organizer, use the move operation. Read-only, except when importing an event.",
     "properties": {
      "displayName": {
       "type": "string",
       "description": "The organizer's name, if available."
      },
      "email": {
       "type": "string",
       "description": "The organizer's email address, if available. It must be a valid email address as per RFC5322."
      },
      "id": {
       "type": "string",
       "description": "The organizer's Profile ID, if available. It corresponds to theid field in the People collection of the Google+ API"
      },
      "self": {
       "type": "boolean",
       "description": "Whether the organizer corresponds to the calendar on which this copy of the event appears. Read-only. The default is False.",
       "default": "false"
      }
     }
    },
    "originalStartTime": {
     "$ref": "EventDateTime",
     "description": "For an instance of a recurring event, this is the time at which this event would start according to the recurrence data in the recurring event identified by recurringEventId. Immutable."
    },
    "privateCopy": {
     "type": "boolean",
     "description": "Whether this is a private event copy where changes are not shared with other copies on other calendars. Optional. Immutable. The default is False.",
     "default": "false"
    },
    "recurrence": {
     "type": "array",
     "description": "List of RRULE, EXRULE, RDATE and EXDATE lines for a recurring event, as specified in RFC5545. Note that DTSTART and DTEND lines are not allowed in this field; event start and end times are specified in the start and end fields. This field is omitted for single events or instances of recurring events.",
     "items": {
      "type": "string"
     }
    },
    "recurringEventId": {
     "type": "string",
     "description": "For an instance of a recurring event, this is the id of the recurring event to which this instance belongs. Immutable."
    },
    "reminders": {
     "type": "object",
     "description": "Information about the event's reminders for the authenticated user.",
     "properties": {
      "overrides": {
       "type": "array",
       "description": "If the event doesn't use the default reminders, this lists the reminders specific to the event, or, if not set, indicates that no reminders are set for this event. The maximum number of override reminders is 5.",
       "items": {
        "$ref": "EventReminder"
       }
      },
      "useDefault": {
       "type": "boolean",
       "description": "Whether the default reminders of the calendar apply to the event."
      }
     }
    },
    "sequence": {
     "type": "integer",
     "description": "Sequence number as per iCalendar.",
     "format": "int32"
    },
    "source": {
     "type": "object",
     "description": "Source from which the event was created. For example, a web page, an email message or any document identifiable by an URL with HTTP or HTTPS scheme. Can only be seen or modified by the creator of the event.",
     "properties": {
      "title": {
       "type": "string",
       "description": "Title of the source; for example a title of a web page or an email subject."
      },
      "url": {
       "type": "string",
       "description": "URL of the source pointing to a resource. The URL scheme must be HTTP or HTTPS."
      }
     }
    },
    "start": {
     "$ref": "EventDateTime",
     "description": "The (inclusive) start time of the event. For a recurring event, this is the start time of the first instance.",
     "annotations": {
      "required": [
       "calendar.events.import",
       "calendar.events.insert",
       "calendar.events.update"
      ]
     }
    },
    "status": {
     "type": "string",
     "description": "Status of the event. Optional. Possible values are:  \n- \"confirmed\" - The event is confirmed. This is the default status. \n- \"tentative\" - The event is tentatively confirmed. \n- \"cancelled\" - The event is cancelled."
    },
    "summary": {
     "type": "string",
     "description": "Title of the event."
    },
    "transparency": {
     "type": "string",
     "description": "Whether the event blocks time on the calendar. Optional. Possible values are:  \n- \"opaque\" - The event blocks time on the calendar. This is the default value. \n- \"transparent\" - The event does not block time on the calendar.",
     "default": "opaque"
    },
    "updated": {
     "type": "string",
     "description": "Last modification time of the event (as a RFC3339 timestamp). Read-only.",
     "format": "date-time"
    },
    "visibility": {
     "type": "string",
     "description": "Visibility of the event. Optional. Possible values are:  \n- \"default\" - Uses the default visibility for events on the calendar. This is the default value. \n- \"public\" - The event is public and event details are visible to all readers of the calendar. \n- \"private\" - The event is private and only event attendees may view event details. \n- \"confidential\" - The event is private. This value is provided for compatibility reasons.",
     "default": "default"
    }
   }
  },

	*/

			if (string.IsNullOrWhiteSpace (calid))
				throw new Exception ("the calendar identifier is not specified");
			var creds = GetServiceAccountCredential();

			using (var client = new HttpClient()) {
				using (var request = new HttpRequestMessage(HttpMethod.Post,
					string.Format("calendars/{calendarId}/events",calid))) {
						request.Content = new StringContent( 
							JsonConvert.SerializeObject(
							new Resource {
								summary = "",
								description = "",
								start = new GDate{ datetime = startDate },
								end = new GDate{ datetime = startDate.AddSeconds(lengthInSeconds) },
								location = location,
								transparency = available ? "transparent" : null
							}
						));
					using (var response = await client.SendAsync(request)) {
						var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
						return payload.ToObject<Resource>();
					}
				}

			}
        }
    }
}
