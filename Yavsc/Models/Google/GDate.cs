//
//  GDate.cs
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

namespace Yavsc.Models.Google
{
	/// <summary>
	/// G date.
	/// </summary>
	public class GDate {
		/// <summary>
		/// The date.
		/// </summary>
		public DateTime? date;
		/// <summary>
		/// The datetime.
		/// </summary>
		public DateTime? datetime;
		/// <summary>
		/// The time zone.
		/// </summary>
		public string timeZone;
	}

}
