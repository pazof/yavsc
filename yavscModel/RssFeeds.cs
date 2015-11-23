//
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
	/// <summary>
	/// Rss feeds entry.
	/// </summary>
	public class RssFeedsEntry: ITitle {
		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
			public string Title { get ; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
			public string Description { get ; set; }
		/// <summary>
		/// Gets or sets the pub date.
		/// </summary>
		/// <value>The pub date.</value>
		public DateTime PubDate { get ; set; }
		/// <summary>
		/// Gets or sets the link.
		/// </summary>
		/// <value>The link.</value>
			public string Link { get ; set; }
		}

	/// <summary>
	/// Rss feeds channel.
	/// </summary>
	public class RssFeedsChannel {
		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title { get ; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get ; set; }
		/// <summary>
		/// Gets or sets the last build date.
		/// </summary>
		/// <value>The last build date.</value>
		public DateTime LastBuildDate { get ; set; }
		/// <summary>
		/// Gets or sets the link.
		/// </summary>
		/// <value>The link.</value>
		public string Link { get ; set; }
		/// <summary>
		/// The entries.
		/// </summary>
		public RssFeedsEntry[] Entries;
		}

}

