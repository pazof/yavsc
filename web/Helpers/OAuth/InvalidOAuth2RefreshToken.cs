//
//  InvalidOAuth2RefreshToken.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
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
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Yavsc.Model.Google;
using System.Web.Profile;
using System.Web;
using Yavsc.Model;
using System.Runtime.Serialization.Json;
using Yavsc.Client;
using System.Collections.Generic;

namespace Yavsc.Helpers.OAuth
{

	/// <summary>
	/// Invalid O auth2 refresh token.
	/// </summary>
	public class InvalidOAuth2RefreshToken: Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Helpers.OAuth.Api.InvalidOAuth2RefreshToken"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		public InvalidOAuth2RefreshToken(string message):base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Helpers.OAuth.Api.InvalidOAuth2RefreshToken"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="innerException">Inner exception.</param>
		public InvalidOAuth2RefreshToken(string message,Exception innerException):base(message,innerException)
		{
		}
	}

}
