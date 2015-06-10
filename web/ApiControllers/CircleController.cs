﻿//
//  CircleController.cs
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
using System.Web.Http;
using Yavsc.Model.RolesAndMembers;
using System.Collections.Generic;
using Yavsc.Model.Circles;
using System.Web.Security;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// Circle controller.
	/// </summary>
	public class CircleController : ApiController
	{
		/// <summary>
		/// Add the specified id and users.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="title">Title.</param>
		/// <param name="users">Users.</param>
		[Authorize]
		public void Add(string id, string [] users) 
		{
			string user = Membership.GetUser ().UserName;
			CircleManager.DefaultProvider.Add (user, id, users);
		}
		/// <summary>
		/// Delete the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[Authorize] public void Delete(string id) 
		{
			string user = Membership.GetUser ().UserName;
			CircleManager.DefaultProvider.Delete (user, id);
		}

		/// <summary>
		/// Get the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[Authorize]
		public Circle Get(string id)
		{
			string user = Membership.GetUser ().UserName;
			return CircleManager.DefaultProvider.Get (user, id);
		}


		/// <summary>
		/// List this instance.
		/// </summary>
		[Authorize]
		public CircleInfoCollection List()
		{
			string user = Membership.GetUser ().UserName;
			return CircleManager.DefaultProvider.List ();
		}
	}
}

