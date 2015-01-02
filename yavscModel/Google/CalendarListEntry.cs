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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Mvc;
using System.Configuration;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.IO;
using Yavsc.Model;

namespace Yavsc.Model.Google
{

	public class CalendarListEntry {
		public string kind { get; set;}
		public string etag { get; set; }
		public string id { get; set; }
		public string summary { get; set; }
		public string description { get; set; }
		public string timeZone { get; set; }
		public string colorId { get; set; }
		public string backgroundColor { get; set; }
		public string foregroundColor { get; set; }
		public bool selected { get; set; }
		public bool primary { get; set; }
		public string accessRole { get; set; }
		public class Reminder {
			public string method { get; set; }
			public int minutes { get; set; }
		}
		public Reminder[] defaultReminders { get; set; }
		/*   "notificationSettings": { "notifications": 
      [ { "type": "eventCreation", "method": "email" }, 
      { "type": "eventChange", "method": "email" }, 
      { "type": "eventCancellation", "method": "email" }, 
      { "type": "eventResponse", "method": "email" } ] }, "primary": true },

*/
	}
	
}
