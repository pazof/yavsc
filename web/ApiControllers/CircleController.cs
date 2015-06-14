//
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
	public class CircleApiController : ApiController
	{
		/// <summary>
		/// Creates the specified circle using the given title and user list.
		/// </summary>
		/// <param name="title">Identifier.</param>
		/// <param name="users">Users.</param>
		[Authorize]
		public long Create(string title, string [] users) 
		{
			string user = Membership.GetUser ().UserName;
			return CircleManager.DefaultProvider.Create (user, title, users);
		}

		/// <summary>
		/// Add the specified users to the circle.
		/// </summary>
		/// <param name="id">Circle Identifier.</param>
		/// <param name="username">username.</param>
		[Authorize]
		public void Add(long id, string username)
		{
			checkIsOwner (CircleManager.DefaultProvider.Get (id));
			CircleManager.DefaultProvider.Add (id, username);
		}


		/// <summary>
		/// Delete the circle specified by id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[Authorize] public void Delete(long id) 
		{
			checkIsOwner (CircleManager.DefaultProvider.Get(id));
			CircleManager.DefaultProvider.Delete (id);
		}

		private void checkIsOwner(Circle c)
		{
			string user = Membership.GetUser ().UserName;
			if (c.Owner != user)
			throw new AccessViolationException ("You're not owner of this circle");
		}

		/// <summary>
		/// Get the circle specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[Authorize]
		public Circle Get(long id)
		{
			var c = CircleManager.DefaultProvider.Get (id);
			checkIsOwner (c);
			return c;
		}


		/// <summary>
		/// List the circles
		/// </summary>
		[Authorize]
		public CircleInfoCollection List()
		{
			string user = Membership.GetUser ().UserName;
			return CircleManager.DefaultProvider.List (user);
		}
	}
}

