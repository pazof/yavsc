//
//  NpgsqlCircleProvider.cs
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
using Yavsc.Model.Circles;
using System.Collections.Specialized;
using System.Configuration;

namespace WorkFlowProvider
{
	/// <summary>
	/// Npgsql circle provider.
	/// </summary>
	public class NpgsqlCircleProvider : CircleProvider
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WorkFlowProvider.NpgsqlCircleProvider"/> class.
		/// </summary>
		public NpgsqlCircleProvider ()
		{
		}

		#region implemented abstract members of CircleProvider
		/// <summary>
		/// Add the specified owner, title and users.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="title">Title.</param>
		/// <param name="users">Users.</param>
		public override void Add (string owner, string title, string[] users)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Delete the specified owner and title.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="title">Title.</param>
		public override void Delete (string owner, string title)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Get the specified owner and title.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="title">Title.</param>
		public override Circle Get (string owner, string title)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// List this instance.
		/// </summary>
		public override CircleInfoCollection List ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		string cnxstr = null;
		string applicationName = null;
		/// <summary>
		/// Initialize this object using the specified name and config.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="config">Config.</param>
		public override void Initialize (string name, NameValueCollection config)
		{
			if ( string.IsNullOrWhiteSpace(config ["connectionStringName"]))
				throw new ConfigurationErrorsException ("No name for Npgsql connection string found");

			cnxstr = ConfigurationManager.ConnectionStrings [config ["connectionStringName"]].ConnectionString;
			applicationName = config["applicationName"] ?? "/";
		}

	}
}

