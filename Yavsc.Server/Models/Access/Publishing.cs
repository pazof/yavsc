//
//  Publishing.cs
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



namespace Yavsc.Models.Access
{

	/// <summary>
	/// Publishing.
	/// </summary>
	public enum Publishing {
		/// <summary>
		/// In the context of immediate use, with no related stored content.
		/// </summary>
        None,

		/// <summary>
		/// In the context of private use of an uploaded content.
		/// </summary>
		Private,
		/// <summary>
		/// In the context of restricted access areas, like circle members views.
		/// </summary>
        Restricted,
		/// <summary>
		/// Publishing a content in a public access area.
		/// </summary>
		Public
	}

}
