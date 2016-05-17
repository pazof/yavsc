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
using Yavsc.Model.Identity;
using System.Collections.Generic;

namespace Npgsql.Web.RolesAndMembers
{
	using Yavsc.Model.RolesAndMembers;

	/// <summary>
	/// Npgsql user name provider.
	/// </summary>
	public class NpgsqlUserNameProvider: ChangeUserNameProvider
	{
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

		#region implemented abstract members of ChangeUserNameProvider

		/// <summary>
		/// Saves the token.
		/// </summary>
		/// <returns>The token.</returns>
		/// <param name="username">Username.</param>
		/// <param name="token">Token.</param>
		// TODO: in case of updating an existing token, 
		// an Id should be given and indicate an update instead of
		// an insertion.
		public override long SaveToken (string username, AuthToken token)
		{
			long savedtokenid = 0;
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				conn.Open ();
				using (var transact = conn.BeginTransaction ()) {
					using (NpgsqlCommand cmd = conn.CreateCommand ()) {
						cmd.CommandText = 
							@"select t._id from oauthtoken t, profile p 
where p.username = :uname 
AND p.applicationname = :app
AND t.authtype = :auth
";
						cmd.Parameters.AddWithValue ("uname", username);
						cmd.Parameters.AddWithValue ("appname", this.applicationName);
						cmd.Parameters.AddWithValue ("auth", token.token_type);
						savedtokenid = (long)cmd.ExecuteScalar ();
					}
					if (savedtokenid > 0) {
						using (NpgsqlCommand cmd = conn.CreateCommand ()) {
							// do not override an existing 
							// refresh token with null
							if (token.refresh_token==null)
								cmd.CommandText = @"update oauthtoken t 
								set access = :token, externaltokenid = :etid , expiration = :exp
where _id = :tid
";
							else 
							cmd.CommandText = @"update oauthtoken t 
								set access = :token, 
refresh = :reft , externaltokenid = :etid , expiration = :exp
where _id = :tid
";
							cmd.Parameters.AddWithValue("token",token.access_token);
							cmd.Parameters.AddWithValue("etid",token.id_token);
							cmd.Parameters.AddWithValue("exp",
								DateTime.Now.AddSeconds(token.expires_in));
							cmd.Parameters.AddWithValue("tid",savedtokenid);
							if (token.refresh_token!=null)
								cmd.Parameters.AddWithValue("reft",token.refresh_token);
							cmd.ExecuteNonQuery ();
						}
					} else {
						using (NpgsqlCommand cmd = conn.CreateCommand ()) {
							cmd.CommandText = 
								@"insert into oauthtoken 
								(profileid,access,refresh,externaltokenid,expiration,authtype) 
								values (
					(select uniqueid from profile where username = :user and applicationname = :app)
		, :token , :reft, :etid, :exp, :auth )
								returning _id ";
							savedtokenid = (long) cmd.ExecuteScalar ();
						}
							
					}
					transact.Commit ();
				}
			}
			return savedtokenid;
		}


		public override SavedToken GetOAuthToken (string username, string authType)
		{
			SavedToken result = null;
			using (NpgsqlConnection conn = new NpgsqlConnection (connectionString)) {
				conn.Open ();
				using (NpgsqlCommand cmd = new NpgsqlCommand (
					                           "SELECT t._id, " +
					                           " t.expiration, " +
					                           " t.refresh, t.access, t.externaltokenid " +
					                           "FROM oauthtoken t, profile p " +
					                           "WHERE t.profileid = p.uniqueid " +
					                           "AND p.username = :uname " +
					                           "AND p.applicationname = :app " +
					                           "AND t.authtype = :auth ", conn)) {
					cmd.Parameters.AddWithValue ("uname", username);
					cmd.Parameters.AddWithValue ("appname", this.applicationName);
					cmd.Parameters.AddWithValue ("auth", authType);
					using (var rdr = cmd.ExecuteReader ()) {
						if (rdr.Read ()) {
							result = new SavedToken ();
							result.Id = rdr.GetInt64 (0);
							result.token_type = authType;
							result.expires = rdr.GetDateTime (1);
							result.refresh_token = rdr.GetString (2);
							result.access_token = rdr.GetString (3);
							result.id_token = rdr.GetString (4);
						}
					}
				}
				conn.Close ();
			}
			return result;
		}

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
					                           "SELECT count(*)=0 FROM users " +
					                           "WHERE username = :uname AND ApplicationName = :appname", conn)) {
					cmd.Parameters.AddWithValue ("uname", name);
					cmd.Parameters.AddWithValue ("appname", this.applicationName);
					return (bool)cmd.ExecuteScalar ();
				}
			}
		}

		#endregion
	}
	
}
