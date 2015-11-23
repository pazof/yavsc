//
//  UUTBlogEntryCollection.cs
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
	/// Unique User and Title blog entry collection.
	/// </summary>
	public class UUTBlogEntryCollection : UUBlogEntryCollection, ITitle {
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Blogs.UUTBlogEntryCollection"/> class.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="title">Title.</param>
		public UUTBlogEntryCollection(string username, string title) : base(username) {
			_title = title;
		}

		private string _title;
		/// <summary>
		/// Gets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title { get { return _title; }
			set { _title = value; } }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.Blogs.UUTBlogEntryCollection"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.Blogs.UUTBlogEntryCollection"/>.</returns>
		public override string ToString ()
		{
			return string.Format ("[UUTBlogEntryCollection: " +
				"Title={0} User={1} Count={2}]", Title, Author, Count);
		}

		
	}
	
}
