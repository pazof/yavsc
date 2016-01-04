//
//  ErrorHtmlFormatter.cs
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
using MarkdownDeep;
using Yavsc.Helpers;
using Yavsc.Model.Blogs;

namespace Yavsc.Formatters
{
	/// <summary>
	/// Formats a given error message to respond
	/// in case of error, and in an html format
	/// </summary>
	public class ErrorHtmlFormatter:SimpleFormatter {

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title { get ; set; }
		/// <summary>
		/// Gets or sets the error code.
		/// </summary>
		/// <value>The error code.</value>
		public HttpStatusCode ErrorCode { get ; set; }

		string doctype="<!DOCTYPE html>";
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Formatters.ErrorHtmlFormatter"/> class.
		/// </summary>
		/// <param name="errorCode">Error code.</param>
		/// <param name="title">Title.</param>
		public ErrorHtmlFormatter 
		(HttpStatusCode errorCode, string title):base("text/html")
		{
			// FIXME this is not a place for this property
			ErrorCode = errorCode;
			Title = title;

		}
		/// <summary>
		/// Writes to stream.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="value">Value.</param>
		/// <param name="stream">Stream.</param>
		/// <param name="contentHeaders">Content headers.</param>
		public override void WriteToStream (Type type, object value, Stream stream, HttpContent contentHeaders)
		{
			// TODO create a type containing T4 parameters, and generate from them
			using (var writer = new StreamWriter(stream))
			{
				string message = value as string;
				TagBuilder doc = new TagBuilder ("html");
				TagBuilder body = new TagBuilder ("body");
				TagBuilder h1 = new TagBuilder ("h1");
				TagBuilder p = new TagBuilder ("p");
				TagBuilder head = new TagBuilder ("head");
				head.InnerHtml = "<meta http-equiv=\"Content-Type\" " +
					"content=\"text/html; charset=utf-8\"/>" +
					"<link rel=\"stylesheet\" " +
					"href=\"/Theme/style.css\" />" +
					"<link rel=\"icon\" type=\"image/png\"" +
					" href=\"/favicon.png\" />";
				p.InnerHtml = MarkdownHelper.Markdown(message).ToHtmlString();
				h1.InnerHtml = MvcHtmlString.Create (Title).ToHtmlString();
				body.InnerHtml = h1.ToString()+p.ToString ();
				doc.InnerHtml = head.ToString()+"\n"+body.ToString ();
				writer.WriteLine (doctype);
				writer.Write (doc.ToString());
			}

		}

	}
}
