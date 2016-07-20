//
//  OpenDay.cs
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

namespace BookAStar.Model.Workflow.Calendar
{

	/// <summary>
	/// Open day.
	/// </summary>
	public class OpenDay {
		/// <summary>
		/// The day.
		/// </summary>
		public WeekDay Day;

		/// <summary>
		/// Gets or sets the s.
		/// </summary>
		/// <value>The s.</value>
		public TimeSpan S { get; set; }

		// ASSERT Start <= End
		/// <summary>
		/// Gets or sets the start hour.
		/// </summary>
		/// <value>The start.</value>
		public TimeSpan Start { get; set; }
		/// <summary>
		/// Gets or sets the end hour
		/// (from the next day if lower than the Start).
		/// </summary>
		/// <value>The end.</value>
		public TimeSpan End { get; set; }
	}

}
