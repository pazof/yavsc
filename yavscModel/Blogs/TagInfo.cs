﻿//
//  TagInfo.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
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

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Tag info.
	/// </summary>
	public abstract class TagInfo
	{
		string name;
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Blogs.TagInfo"/> class.
		/// </summary>
		/// <param name="tagname">Tagname.</param>
		public TagInfo (string tagname)
		{
			Name = tagname;
		}

		/// <summary>
		/// Gets the titles.
		/// </summary>
		/// <value>The titles.</value>
		public abstract IEnumerable<BasePostInfo> Titles { get; }
	}
}
