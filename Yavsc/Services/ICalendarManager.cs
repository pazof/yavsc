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
using Google.Apis.Calendar.v3.Data;

namespace Yavsc.Services
{
    using System.Threading.Tasks;
    using Yavsc.ViewModels.Calendar;

    /// <summary>
    /// I calendar manager.
    /// </summary>
    public interface ICalendarManager {
		Task<CalendarList> GetCalendarsAsync (string userId);
		Task<Events> GetCalendarAsync  (string calid, DateTime mindate, DateTime maxdate);
    Task<DateTimeChooserViewModel> CreateViewModelAsync(
			string inputId,
			string calid, DateTime mindate, DateTime maxdate);
    Task<Event> CreateEventAsync(string calid, 
    DateTime startDate, int lengthInSeconds, string summary, 
    string description, string location, bool available);
	}
}
