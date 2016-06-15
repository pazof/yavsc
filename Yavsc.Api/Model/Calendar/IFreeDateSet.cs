//  FreeDate.cs
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
using System.Collections.Generic;

namespace Yavsc.Models.Calendar
{

	/// <summary>
	/// Free date.
	/// </summary>
	public interface IFreeDateSet
	{
		/// <summary>
		/// Gets or sets the reference.
		/// </summary>
		/// <value>The reference.</value>
		 IEnumerable<Period> Values { get; set; }
		/// <summary>
		/// Gets or sets the duration.
		/// </summary>
		/// <value>The duration.</value>
		 TimeSpan Duration { get; set; }
		/// <summary>
		/// Gets or sets the attendees.
		/// </summary>
		/// <value>The attendees.</value>
		 string UserName { get; set; }
		/// <summary>
		/// Gets or sets the location.
		/// </summary>
		/// <value>The location.</value>
		 string Location { get; set; }
	}
}

