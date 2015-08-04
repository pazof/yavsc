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
using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Npgsql.Web.RolesAndMembers
{
	using Yavsc.Model.RolesAndMembers;

	/// <summary>
	/// Npgsql user name provider.
	/// </summary>
	public class NpgsqlUserNameProvider: ChangeUserNameProvider {
		private string applicationName;
		private string connectionString;
		/// <summary>
		/// Initialize the specified iname and config.
		/// </summary>
		/// <param name="iname">Iname.</param>
		/// <param name="config">Config.</param>
		public override void Initialize (string iname, NameValueCollection config)
		{
			// get the 
			// - application name
			// - connection string name
			// - the connection string from its name
			string cnxName = config ["connectionStringName"];
			connectionString = ConfigurationManager.ConnectionStrings [cnxName].ConnectionString;
			config.Remove ("connectionStringName");
			applicationName = config ["applicationName"];
			config.Remove ("applicationName");
			base.Initialize (iname, config);
			
		}
		private string GetConfigValue (string configValue, string defaultValue)
		{
			if (String.IsNullOrEmpty (configValue))
				return defaultValue;

			return configValue;
		}

		#region implemented abstract members of ChangeUserNameProvider
		/// <summary>
		/// Changes the name.
		/// </summary>
		/// <param name="oldName">Old name.</param>
		/// <param name="newName">New name.</param>
		public override void ChangeName (string oldName, string newName)
		{
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				conn.Open ();
				using (NpgsqlCommand cmd = new NpgsqlCommand (
					"UPDATE users set " +
					"username = :uname where username = :oname" +
				    " AND ApplicationName = :appname ", conn)) {
					cmd.Parameters.AddWithValue ("uname", newName);
					cmd.Parameters.AddWithValue ("oname", oldName);
					cmd.Parameters.AddWithValue ("appname", this.applicationName);
					cmd.ExecuteNonQuery ();
				}
			}
		}
		/// <summary>
		/// Determines whether this instance is name available the specified name.
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="name">Name.</param>
		public override bool IsNameAvailable (string name)
		{
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				conn.Open ();
				using (NpgsqlCommand cmd = new NpgsqlCommand (
					"SELECT count(*)>0 FROM users " +
					"WHERE username = :uname AND ApplicationName = :appname", conn)) {
					cmd.Parameters.AddWithValue ("uname", name);
					cmd.Parameters.AddWithValue ("appname", this.applicationName);
					return (bool) cmd.ExecuteScalar ();
				}
			}
		}
		#endregion
	}
	
}
