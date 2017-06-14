//
//  CalendarListEntry.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2014 Paul Schneider
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

namespace Yavsc.Models.Google.Calendar
{
	/// <summary>
	/// Calendar list entry.
	/// </summary>
	public class CalendarListEntry {
		/// <summary>
		/// Gets or sets the kind.
		/// </summary>
		/// <value>The kind.</value>
		public string kind { get; set;}
		/// <summary>
		/// Gets or sets the etag.
		/// </summary>
		/// <value>The etag.</value>
		public string etag { get; set; }
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string id { get; set; }
		/// <summary>
		/// Gets or sets the summary.
		/// </summary>
		/// <value>The summary.</value>
		public string summary { get; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		public string description { get; set; }
		/// <summary>
		/// Gets or sets the time zone.
		/// </summary>
		/// <value>The time zone.</value>
		public string timeZone { get; set; }
		/// <summary>
		/// Gets or sets the color identifier.
		/// </summary>
		/// <value>The color identifier.</value>
		public string colorId { get; set; }
		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		/// <value>The color of the background.</value>
		public string backgroundColor { get; set; }
		/// <summary>
		/// Gets or sets the color of the foreground.
		/// </summary>
		/// <value>The color of the foreground.</value>
		public string foregroundColor { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Google.CalendarListEntry"/> is selected.
		/// </summary>
		/// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
		public bool selected { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.Google.CalendarListEntry"/> is primary.
		/// </summary>
		/// <value><c>true</c> if primary; otherwise, <c>false</c>.</value>
		public bool primary { get; set; }
		/// <summary>
		/// Gets or sets the access role.
		/// </summary>
		/// <value>The access role.</value>
		public string accessRole { get; set; }
		/// <summary>
		/// Reminder.
		/// </summary>

		/// <summary>
		/// Gets or sets the default reminders.
		/// </summary>
		/// <value>The default reminders.</value>
		public Reminder[] defaultReminders { get; set; }
		/*   "notificationSettings": { "notifications":
      [ { "type": "eventCreation", "method": "email" },
      { "type": "eventChange", "method": "email" },
      { "type": "eventCancellation", "method": "email" },
      { "type": "eventResponse", "method": "email" } ] }, "primary": true },

*/
	}
	/// <summary>
	/// Reminder.
	/// </summary>


}
