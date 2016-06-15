//
//  CalendarEventList.cs
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
namespace Yavsc.Models.Google
{

	/// <summary>
	/// Calendar event list.
	/// </summary>
	public class CalendarEventList
	{
		/// <summary>
		/// The next page token.
		/// </summary>
		public string nextPageToken;
		/// <summary>
		/// The next sync token.
		/// </summary>
		public string nextSyncToken;
		/// <summary>
		/// The items.
		/// </summary>
		public Resource [] items ;
	}
}

