//
//  Schedule.cs
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

using System;
using System.Web.Http;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Calendar
{
	/// <summary>
	/// Schedule.
	/// </summary>
	public class Schedule {
		/// <summary>
		/// Gets or sets the period.
		/// </summary>
		/// <value>The period.</value>
		public Periodicity Period { get; set; }

		/// <summary>
		/// Gets or sets the schedule of an open week.
		/// One item by bay in the week, 
		/// </summary>
		/// <value>The weekly workdays.</value>
		public OpenDay [] WeekDays { get; set; }
		/// <summary>
		/// Gets or sets the hollydays.
		/// </summary>
		/// <value>The hollydays.</value>
		[Required]
		public Period [] Validity { get; set; }
	}
	
}
