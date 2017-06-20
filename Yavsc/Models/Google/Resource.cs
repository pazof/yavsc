//
//  Resource.cs
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

namespace Yavsc.Models.Google
{
	/// <summary>
	/// Resource.
	/// </summary>
	public class Resource {
		public string id;
		public string location;
		public string status;
		public GDate start;
		public GDate end;
		public string recurence;

		public string description;

		public string summary;

		/// <summary>
		/// Avaible <=> transparency == "transparent"
		/// </summary>
		public string transparency;
	}

}
