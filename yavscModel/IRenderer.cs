//
//  IRenderer.cs
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
using System.Configuration;
using System.Collections.Specialized;
using System.Data;

namespace Yavsc.Model
{
	/// <summary>
	/// I renderer.
	/// </summary>
	public interface IRenderer {
		// Should set ViewData["Message|Author|Body"]
		// and return an ActionResult
		/// <summary>
		/// Get the specified c.
		/// </summary>
		/// <param name="c">C.</param>
		object Get(Controller c);
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; set; }
	}

}
