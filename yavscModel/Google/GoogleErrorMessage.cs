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

namespace Yavsc.Model.Google
{
	public class GoogleErrorException : Exception 
	{
		public string Title { get; set; }
		public string Content { get; set; }

		public GoogleErrorException (WebException ex) {
			// ASSERT ex != null;
			Title = ex.Message;

			using (var stream = ex.Response.GetResponseStream())
			using (var reader = new StreamReader(stream))
			{
				Content = reader.ReadToEnd();
			}
		}
		public GoogleErrorException(JsonReaderException ex, string message) {
			Content = message;
			Title = ex.Message;
		}
	}

}

