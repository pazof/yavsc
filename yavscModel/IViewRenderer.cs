//
//  IViewRenderer.cs
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
using System.Web.Mvc;

namespace Yavsc.Model
{
	/// <summary>
	/// I view renderer.
	/// </summary>
	public interface IViewRenderer : IRenderer {
		/// <summary>
		/// Gets the template route part.
		/// </summary>
		/// <value>The template.</value>
		string Template { get; }
	}

}
