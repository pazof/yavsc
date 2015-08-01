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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Configuration;
using System.Web.Profile;
using System.Web.Security;
using Npgsql.Web.Blog;
using Yavsc;
using Yavsc.Model;
using Yavsc.Model.Blogs;
using Yavsc.ApiControllers;
using Yavsc.Model.RolesAndMembers;
using System.Net;
using System.Web.Mvc;
using Yavsc.Model.Circles;

namespace Yavsc.Controllers
{
	public class UUTBlogEntryCollection : BlogEntryCollection {
		
		UUTBlogEntryCollection(string username, string title, 
			public BlogEntryCollection items = null) {
			if (items != null) {
				if (!items.ConcernsAUniqueTitle && items.ConcernsAUniqueTitle)
					throw new InvalidOperationException ();
				this.AddRange (items);
			}
			_title = title;
			_username = username;
		}
		private string _title;
		private string _username;
		public string UserName { get { return _username; } }
		public string Title { get { return _title; } }

	}
	
}
