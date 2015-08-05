//
//  UUBlogEntryCollection.cs
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
	/// Unique User blog entry collection.
	/// </summary>
	public class UUBlogEntryCollection: BlogEntryCollection {

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Blogs.UUBlogEntryCollection"/> class.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="items">Items.</param>
		public UUBlogEntryCollection(string username, 
			IEnumerable<BlogEntry> items = null) : base(items) {
			_username = username;
		}
		private string _username;
		/// <summary>
		/// Gets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		public string UserName { get { return _username; } }
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.Blogs.UUBlogEntryCollection"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.Blogs.UUBlogEntryCollection"/>.</returns>
		public override string ToString ()
		{
			return string.Format ("[UUBlogEntryCollection: UserName={0} Count={1}]", UserName, Count);
		}
	}
	
}
