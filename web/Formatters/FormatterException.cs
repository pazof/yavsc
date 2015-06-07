//
//  TexToPdfFormatter.cs
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
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Diagnostics;
using System.Net.Http;
using Yavsc.Helpers;

namespace Yavsc.Formatters
{
	/// <summary>
	/// Formatter exception.
	/// </summary>
	public class FormatterException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Formatters.FormatterException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		public FormatterException(string message):base(message)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Formatters.FormatterException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="innerException">Inner exception.</param>
		public FormatterException(string message,Exception innerException):base(message,innerException)
		{
		}
	}
}
