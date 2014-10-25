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

namespace Yavsc.Formatters
{
	public class TexFormatter : BufferedMediaTypeFormatter
	{
		public TexFormatter ()
		{
			SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/x-tex"));

		}

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

		public override bool CanReadType(Type type)
		{
			return false;
		}

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

