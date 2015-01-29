//
//  TexFormatter.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2014 Paul Schneider
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

namespace Yavsc.Formatters
{
	/// <summary>
	/// Simple formatter.
	/// </summary>
	public class SimpleFormatter : BufferedMediaTypeFormatter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Formatters.SimpleFormatter"/> class.
		/// </summary>
		/// <param name="mimetype">Mimetype.</param>
		public SimpleFormatter (string mimetype)
		{
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(mimetype));
		}
		/// <summary>
		/// Determines whether this instance can write type the specified type.
		/// </summary>
		/// <returns><c>true</c> if this instance can write type the specified type; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type.</param>
		public override bool CanWriteType(System.Type type)
		{
			if (type == typeof(string))
			{
				return true;
			}
			else
			{
				Type enumerableType = typeof(IEnumerable<string>);
				return enumerableType.IsAssignableFrom(type);
			}
		}
		/// <summary>
		/// Determines whether this instance can read type the specified type.
		/// </summary>
		/// <returns><c>true</c> if this instance can read type the specified type; otherwise, <c>false</c>.</returns>
		/// <param name="type">Type.</param>
		public override bool CanReadType(Type type)
		{
			return false;
		}
		/// <summary>
		/// Writes to stream.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="value">Value.</param>
		/// <param name="stream">Stream.</param>
		/// <param name="contentHeaders">Content headers.</param>
		public override void WriteToStream (Type type, object value, Stream stream, HttpContentHeaders contentHeaders)
		{
			// TODO create a type containing T4 parameters, and generate from them
			using (var writer = new StreamWriter(stream))
			{
				string doc = value as string;
				writer.Write (doc);
			}
		}
	}
}

