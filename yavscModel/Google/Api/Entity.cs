//
//  Entity.cs
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
using System.Web.Profile;
using Yavsc.Model.Google;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Yavsc.Model.Google.Api
{
	/// <summary>
	/// Entity.
	/// </summary>
	public class Entity
	{
		/// <summary>
		/// The I.
		/// </summary>
		public string ID;
		/// <summary>
		/// The name.
		/// </summary>
		public string Name; 

		/// <summary>
		/// The type:  AUTOMOBILE: A car or passenger vehicle.
		/// * TRUCK: A truck or cargo vehicle.
		/// * WATERCRAFT: A boat or other waterborne vehicle.
		/// * PERSON: A person.
		/// </summary>
		public string Type;
	}

}
