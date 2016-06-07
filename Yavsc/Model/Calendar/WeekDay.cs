//
//  WeekDay.cs
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

namespace Yavsc.Models.Calendar
{
	/// <summary>
	/// Week day.
	/// </summary>
	public enum WeekDay:int {
		/// <summary>
		/// The monday (0).
		/// </summary>
		Monday=0,
		/// <summary>
		/// The tuesday.
		/// </summary>
		Tuesday,
		/// <summary>
		/// The wednesday.
		/// </summary>
		Wednesday,
		/// <summary>
		/// The thursday.
		/// </summary>
		Thursday,
		/// <summary>
		/// The friday.
		/// </summary>
		Friday,
		/// <summary>
		/// The saturday.
		/// </summary>
		Saturday,
		/// <summary>
		/// The sunday.
		/// </summary>
		Sunday
	}

}
