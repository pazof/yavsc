//
//  JsonReaderError.cs
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
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Yavsc.Model
{
	/// <summary>
	/// Google error exception.
	/// </summary>
	public class OtherWebException : Exception 
	{
		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title { get; set; }
		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		/// <value>The content.</value>
		public string Content { get; set; }
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Google.GoogleErrorException"/> class.
		/// </summary>
		/// <param name="ex">Ex.</param>
		public OtherWebException (WebException ex) {
			// ASSERT ex != null;
			Title = ex.Message;

			using (var stream = ex.Response.GetResponseStream())
			using (var reader = new StreamReader(stream))
			{
				Content = reader.ReadToEnd();
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Google.GoogleErrorException"/> class.
		/// </summary>
		/// <param name="ex">Ex.</param>
		/// <param name="message">Message.</param>
		[Obsolete]
		public OtherWebException(Exception ex, string message) {
			Content = message;
			Title = ex.Message;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Google.GoogleErrorException"/> class.
		/// </summary>
		/// <param name="ex">Ex.</param>
		[Obsolete]
		public OtherWebException(Exception ex) {
			Content = ex.Message;
			Title = ex.GetType().FullName;
		}
	}

}

