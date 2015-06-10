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

namespace Yavsc.Model.Circles
{
	/// <summary>
	/// Circle provider.
	/// </summary>
	public class CircleProvider: ProviderBase
	{
		/// <summary>
		/// Add the specified title and users.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="users">Users.</param>
		public void Add(string owner, string title, string [] users)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Delete the specified id.
		/// </summary>
		/// <param name="title">Title.</param>
		public void Delete(string owner, string title) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Get the specified id.
		/// </summary>
		/// <param name="title">Title.</param>
		public Circle Get(string owner, string title)
		{
			throw new NotImplementedException ();
		}


		/// <summary>
		/// List this instance.
		/// </summary>
		public CircleInfoCollection List()
		{
			throw new NotImplementedException ();
		}

	}

}

