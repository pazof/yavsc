//
//  AuthorizationDenied.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Profile;
using System.Web.Security;
using Yavsc.Formatters;
using Yavsc.Helpers;
using Yavsc.Model;
using Yavsc.Model.FrontOffice;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Model.WorkFlow;
using System.IO;

namespace Yavsc.ApiControllers
{

	/// <summary>
	/// Authorization denied.
	/// </summary>
	public class AuthorizationDenied : HttpRequestException {

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.ApiControllers.FrontOfficeController+AuthorizationDenied"/> class.
		/// </summary>
		/// <param name="msg">Message.</param>
		public AuthorizationDenied(string msg) : base(msg)
		{
		}
	}
	
}
