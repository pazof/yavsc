﻿//
//  RssFeeds.cs
//
//  Author:
//       paul <${AuthorEmail}>
//
//  Copyright (c) 2015 paul
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

namespace Yavsc.Model
{
	public class RssFeedsEntry {

			public string Title { get ; set; }
			public string Description { get ; set; }
		public DateTime PubDate { get ; set; }
			public string Link { get ; set; }
		}

	public class RssFeedsChannel {
		public string Title { get ; set; }
		public string Description { get ; set; }
		public DateTime LastBuildDate { get ; set; }
		public string Link { get ; set; }
		public RssFeedsEntry[] Entries;
		}

}
