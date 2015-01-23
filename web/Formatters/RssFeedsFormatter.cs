//
//  RssFormatter.cs
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
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.Mvc;
using System.Net;
using Yavsc.Model;
using System.Text;

namespace Yavsc.Formatters
{

	public class RssFeedsFormatter:SimpleFormatter
	{
		string doctype = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

		public RssFeedsFormatter 
		() : base ("application/rss+xml")
		{
		}

		private const string dateformat = "ddd, dd MMM yyyy HH:mm:ss K";

		public override void WriteToStream (Type type, object value, Stream stream, HttpContentHeaders contentHeaders)
		{
			RssFeedsChannel feeds = value as RssFeedsChannel;
			using (var writer = new StreamWriter (stream)) {
				TagBuilder rss = new TagBuilder ("rss");
				rss.Attributes.Add ("version", "2.0");
				TagBuilder channel = new TagBuilder ("channel");
				TagBuilder title = new TagBuilder ("title");
				TagBuilder description = new TagBuilder ("description");
				TagBuilder lastBuildDate = new TagBuilder ("lastBuildDate");
				TagBuilder link = new TagBuilder ("link");

				title.InnerHtml = MvcHtmlString.Create (feeds.Title).ToHtmlString ();
				description.InnerHtml = MvcHtmlString.Create (feeds.Description).ToHtmlString ();
				lastBuildDate.InnerHtml = MvcHtmlString.Create (feeds.LastBuildDate.ToString (dateformat)).ToHtmlString ();
				link.InnerHtml = MvcHtmlString.Create (feeds.Link).ToHtmlString ();
				StringBuilder sb = new StringBuilder ();
				foreach (RssFeedsEntry e in feeds.Entries) {
					TagBuilder item = new TagBuilder ("item");
					TagBuilder ititle = new TagBuilder ("title");
					ititle.InnerHtml = e.Title;

					TagBuilder idescription = new TagBuilder ("description");
					idescription.InnerHtml = MvcHtmlString.Create (e.Description).ToHtmlString ();
					TagBuilder ipubDate = new TagBuilder ("pubDate");
					ipubDate.InnerHtml = MvcHtmlString.Create (
						e.PubDate.ToString (dateformat)).ToHtmlString ();

					TagBuilder ilink = new TagBuilder ("link");
					ilink.InnerHtml = MvcHtmlString.Create (e.Link).ToHtmlString ();

					item.InnerHtml = ititle.ToString () + "\n" +
					idescription.ToString () + "\n" +
					ipubDate.ToString () + "\n" +
					ilink.ToString () + "\n";

					sb.Append (item.ToString () + "\n");
				}
				channel.InnerHtml = title.ToString () + "\n" +
				description.ToString () + "\n" +
				lastBuildDate.ToString () + "\n" +
				link.ToString () + "\n" +
				sb.ToString () + "\n";
				rss.InnerHtml = channel.ToString ();
				writer.WriteLine (doctype);
				writer.Write (rss.ToString ());
			}
		}
	}
	
}
