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

using System;

namespace Yavsc.Models.Calendar
{
    using System.Threading.Tasks;
    using Yavsc.Models.Google.Calendar;
    using Yavsc.Models.Google;
    using Yavsc.ViewModels.Calendar;


    /// <summary>
    /// I calendar manager.
    /// </summary>
    public interface ICalendarManager {
		Task<CalendarList> GetCalendarsAsync (string userId);
		Task<CalendarEventList> GetCalendarAsync  (string calid, DateTime mindate, DateTime maxdate);
    Task<DateTimeChooserViewModel> CreateViewModelAsync(
			string inputId,
			string calid, DateTime mindate, DateTime maxdate);
    Task<Resource> CreateResourceAsync(string calid, 
    DateTime startDate, int lengthInSeconds, string summary, 
    string description, string location, bool available);
	}
}
