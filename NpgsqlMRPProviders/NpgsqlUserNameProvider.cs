//
//  NpgsqlUserNameProvider.cs
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

using System.Web.Security;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;

namespace Npgsql.Web
{
	using Yavsc.Model.RolesAndMembers;
	public class NpgsqlUserNameProvider: ChangeUserNameProvider {
		#region implemented abstract members of ChangeUserNameProvider
		public override void ChangeName (string oldName, string newName)
		{
			throw new NotImplementedException ();
		}
		public override bool IsNameAvailable (string name)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
	
}
