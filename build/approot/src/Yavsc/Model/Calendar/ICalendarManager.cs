//
//  ICalendarManager.cs
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


using Yavsc.Models.Booking;
using Yavsc.Models.Messaging;

namespace Yavsc.Models.Calendar
{
	/// <summary>
	/// I calendar manager.
	/// </summary>
	public interface ICalendarManager {
		/// <summary>
		/// Gets the free dates.
		/// </summary>
		/// <returns>The free dates.</returns>
		/// <param name="username">Username.</param>
		/// <param name="req">Req.</param>
		IFreeDateSet GetFreeDates(string username, BookQuery req);
		/// <summary>
		/// Book the specified username and ev.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="ev">Ev.</param>
		bool Book(string username, YaEvent ev);
	}
}
