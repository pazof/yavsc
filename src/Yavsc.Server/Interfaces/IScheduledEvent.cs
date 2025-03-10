//
//  IScheduledEvent.cs
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

using Yavsc.Server.Models.Calendar;

namespace Yavsc.Models.Calendar
{
    public interface IScheduledEvent
    {
		/// <summary>
		/// Gets or sets the period.
		/// </summary>
		/// <value>The period.</value>
		 Periodicity Reccurence { get; set; }
         Period Period { get; set; }
         
    }
}
