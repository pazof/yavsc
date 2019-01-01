//
//  CalendarList.cs
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

namespace Yavsc.Models.Google.Calendar
{
	/// <summary>
	/// Calendar list.
	/// </summary>
	[Obsolete("use Google.Apis")]
	public class CalendarList {
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
		/// Gets or sets the next sync token.
		/// </summary>
		/// <value>The next sync token.</value>
		public string description { get; set; }
		public string summpary { get; set; }
		public string nextSyncToken { get; set; }
		/// <summary>
		/// Gets or sets the items.
		/// </summary>
		/// <value>The items.</value>
		public CalendarListEntry[] items { get; set; }
	}


}
