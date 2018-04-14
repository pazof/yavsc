//
//  Periodicity.cs
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

namespace Yavsc.Models.Calendar
{
	/// <summary>
	/// Periodicity.
	/// </summary>
	public enum Periodicity {

		/// <summary>
		/// On Demand.
		/// </summary>
		OnDemand=-1,
		/// <summary>
		/// The daily.
		/// </summary>
		Daily,
		/// <summary>
		/// The weekly.
		/// </summary>
		Weekly,
		/// <summary>
		/// The monthly.
		/// </summary>
		Monthly,
		/// <summary>
		/// The three m.
		/// </summary>
		ThreeM,
		/// <summary>
		/// The six m.
		/// </summary>
		SixM,
		/// <summary>
		/// The yearly.
		/// </summary>
		Yearly
	}

}
