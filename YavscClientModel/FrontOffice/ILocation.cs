//
//  ILocation.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
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

namespace YavscClientModel.FrontOffice
{
	/// <summary>
	/// a location interface.
	/// </summary>
	public interface ILocation
	{
		/// <summary>
		/// Gets or sets the address.
		/// </summary>
		/// <value>The address.</value>
		 string Address { get; set; }

		/// <summary>
		/// Gets or sets the latitude.
		/// </summary>
		/// <value>The latitude.</value>
		 double Latitude { get; set; }

		/// <summary>
		/// Gets or sets the longitude.
		/// </summary>
		/// <value>The longitude.</value>
		 double Longitude { get; set; }
	}
}

