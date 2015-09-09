//
//  CircleManager.cs
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
using System.Security.Permissions;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections;
using System.Reflection;
using System.Configuration.Provider;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Yavsc.Model.Circles
{
	/// <summary>
	/// Circle provider.
	/// </summary>
	public abstract class CircleProvider: ProviderBase
	{
		/// <summary>
		/// Add the specified owner, title and users.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="title">Title.</param>
		/// <param name="users">Users.</param>
		public abstract long Create(string owner, string title, string [] users);

		/// <summary>
		/// Add the specified user to the specified circle.
		/// </summary>
		/// <param name="id">circle Identifier.</param>
		/// <param name="username">User name.</param>
		public abstract void AddMember(long id, string username);

		/// <summary>
		/// Delete the specified circle by its id.
		/// </summary>
		/// <param name="id">Circle Identifier.</param>
		public abstract void Delete(long id) ;

		/// <summary>
		/// Get the specified circle by id, including all of its members.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public abstract Circle GetMembers(long id);
		/// <summary>
		/// Get the specified circle by id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public abstract CircleBase Get (long id);

		public abstract long GetId (string circle, string username);
		/// <summary>
		/// List circle's user.
		/// </summary>
		public abstract IEnumerable<CircleBase> List(string user);

		/// <summary>
		/// True when the specified user is listed in one of 
		/// the specified circles
		/// </summary>
		/// <param name="circle_ids">circles to look at</param>
		/// <param name="member">Username to look for in the circles</param>
		public abstract bool Matches(long [] circle_ids, string member);

		/// <summary>
		/// Removes the membership.
		/// </summary>
		/// <param name="circle_id">Circle identifier.</param>
		/// <param name="member">Member.</param>
		public abstract void RemoveMembership(long circle_id, string member);

		/// <summary>
		/// Removes the member from all current user circles.
		/// </summary>
		/// <param name="member">Member.</param>
		public abstract void RemoveMember(string member);

		/// <summary>
		/// Updates the circle.
		/// </summary>
		/// <param name="c">C.</param>
		public abstract void UpdateCircle(CircleBase c);

	}

}

