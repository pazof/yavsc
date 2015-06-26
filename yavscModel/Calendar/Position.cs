//
//  Position.cs
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
	/// Position.
	/// </summary>
	public class Position
	{
		/// <summary>
		/// The longitude.
		/// </summary>
		public double Longitude { get; set; }
		/// <summary>
		/// The latitude.
		/// </summary>
		public double Latitude { get; set; }

	}
	
}
