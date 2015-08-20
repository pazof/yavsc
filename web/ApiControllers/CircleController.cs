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
using System.Collections.Specialized;
using Yavsc.Model;

namespace Yavsc.ApiControllers
{
	/// <summary>
	/// New circle.
	/// </summary>
	public class NewCircle {
		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string title { get ; set; } 
		/// <summary>
		/// Gets or sets the users.
		/// </summary>
		/// <value>The users.</value>
		public string [] users { get ; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.ApiControllers.NewCircle"/> is private.
		/// </summary>
		/// <value><c>true</c> if is private; otherwise, <c>false</c>.</value>
		public bool isPrivate { get; set; }
	}

	/// <summary>
	/// Circle controller.
	/// </summary>
	public class CircleController : ApiController
	{

		/// <summary>
		/// Create the specified circle.
		/// </summary>
		/// <param name="model">Model.</param>
		[Authorize,
			AcceptVerbs ("POST")]
		public long Create(NewCircle model) 
		{
			string user = Membership.GetUser ().UserName;
			return CircleManager.DefaultProvider.Create (user, model.title, model.users);
		}

		/// <summary>
		/// Add the specified users to the circle.
		/// </summary>
		/// <param name="id">Circle Identifier.</param>
		/// <param name="username">username.</param>
		[Authorize,
			AcceptVerbs ("POST")]
		public void Add(long id, string username)
		{
			checkIsOwner (CircleManager.DefaultProvider.Get (id));
			CircleManager.DefaultProvider.Add (id, username);
		}


		/// <summary>
		/// Delete the circle specified by id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[Authorize,
			AcceptVerbs ("GET")] 
		public void Delete(long id) 
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
		[Authorize,
		AcceptVerbs ("GET")]
		public Circle Get(long id)
		{
			var c = CircleManager.DefaultProvider.Get (id);
			checkIsOwner (c);
			return c;
		}

		/// <summary>
		/// List the circles
		/// </summary>
		[Authorize,
			AcceptVerbs ("GET")]
		public IEnumerable<Circle> List()
		{
			string user = Membership.GetUser ().UserName;
			return CircleManager.DefaultProvider.List (user);
		}
	}
}

