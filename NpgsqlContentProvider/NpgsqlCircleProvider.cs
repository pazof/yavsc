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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Npgsql;
using NpgsqlTypes;
using Yavsc.Model;
using Yavsc.Model.Circles;

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
		/// Returns circles from owner.
		/// </summary>
		/// <param name="circle_ids">Circle identifiers.</param>
		/// <param name="member">Member name.</param>
		public override bool Matches (long [] circle_ids, string member)
		{
			bool result=false;
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "select count(*)>0 from circle_members where circle_id = :cid and member = :mbr";
				cmd.Parameters.Add("cid",NpgsqlDbType.Bigint);
				cmd.Parameters.AddWithValue("mbr",member);
				cnx.Open ();
				cmd.Prepare ();
				foreach (long cid in circle_ids) {
					result = (bool) cmd.ExecuteScalar();
					if (result)
						break;
				}
				cnx.Close ();
			}
			return result;
		}

		/// <summary>
		/// Add the specified user.
		/// </summary>
		/// <param name="id">circle Identifier.</param>
		/// <param name="username">User name.</param>
		public override void Add (long id, string username)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "insert into circle_members (circle_id, member) values (:cid,:uname)";
				cmd.Parameters.AddWithValue("cid",id);
				cmd.Parameters.AddWithValue("uname",username);
				cnx.Open ();
				cmd.ExecuteNonQuery ();	
				cnx.Close ();
			}
		}

		/// <summary>
		/// Remove the specified user.
		/// </summary>
		/// <param name="id">circle Identifier.</param>
		/// <param name="username">User name.</param>
		public override void Remove (long id, string username)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "delete from circle_members where circle_id = :cid and username = :uname";
				cmd.Parameters.AddWithValue("cid",id);
				cmd.Parameters.AddWithValue("uname",username);
				cnx.Open ();
				cmd.ExecuteNonQuery ();	
				cnx.Close ();
			}
		}

		/// <summary>
		/// Get the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public override Circle Get (long id)
		{
			Circle circ=null;

			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "select title, owner, public from circle where _id = :cid";
					cmd.Parameters.AddWithValue ("cid", id);
					using (NpgsqlDataReader dr = cmd.ExecuteReader ()) {
						if (dr.Read ()) {
							circ = new Circle ();
							circ.Id = id;
							circ.Title = dr.GetString (
								dr.GetOrdinal ("title"));
							circ.Owner = dr.GetString (
								dr.GetOrdinal ("owner"));
							circ.IsPrivate = !dr.GetBoolean(dr.GetOrdinal("public"));

						}
						dr.Close ();
					}
				}

				if (circ != null) {

					using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
						cmd.CommandText = "select member from circle_members where circle_id = :cid";
						cmd.Parameters.AddWithValue ("cid", id);
						cmd.Prepare ();
						List<string> members = new List<string> ();
						using (NpgsqlDataReader dr = cmd.ExecuteReader ()) {
							while (dr.Read ()) 
								members.Add (dr.GetString (0));
							dr.Close ();
							circ.Members = members.ToArray ();
						}
					}
				}
				cnx.Close ();
			}
			return circ;
		}

		/// <summary>
		/// Add the specified owner, title and users.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="title">Title.</param>
		/// <param name="users">Users.</param>
		public override long Create (string owner, string title, string[] users)
		{
			long id = 0;
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "insert into circle (owner,title,applicationname,public) values (:wnr,:tit,:app,FALSE) returning _id";
					cmd.Parameters.AddWithValue ("wnr", owner);
					cmd.Parameters.AddWithValue ("tit", title);
					cmd.Parameters.AddWithValue ("app", applicationName);
					id = (long) cmd.ExecuteScalar ();
				}
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "insert into circle_members (circle_id,member) values (@cid,@mbr)";
					cmd.Parameters.AddWithValue ("cid", id);
					cmd.Parameters.Add ("mbr", NpgsqlDbType.Varchar);
					cmd.Prepare ();
					if (users!=null)
						foreach (string user in users) {
							cmd.Parameters[1].Value = user;
							cmd.ExecuteNonQuery ();
						}
				}
				cnx.Close ();
			}
			return id;
		}

		/// <summary>
		/// Delete the specified title.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public override void Delete (long id)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "delete from circle where _id = @cid";
				cmd.Parameters.AddWithValue("cid",id);
				cnx.Open ();
				cmd.ExecuteNonQuery ();	
				cnx.Close ();
			}
		}

		/// <summary>
		/// List user's circles.
		/// </summary>
		/// <param name="user">User.</param>
		public override IEnumerable<Circle> List (string user)
		{
			List<Circle> cc = new List<Circle> ();
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "select _id, title from circle where owner = :wnr";
					cmd.Parameters.AddWithValue ("wnr", user);
					cnx.Open ();
					cmd.Prepare ();
					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						if (rdr.HasRows) {
						
							while (rdr.Read ()) {
								string title = null;
								int ottl = rdr.GetOrdinal ("title");
								if (!rdr.IsDBNull (ottl))
									title = rdr.GetString (ottl);
								long id = (long)rdr.GetInt64 (
									         rdr.GetOrdinal ("_id"));
								cc.Add (new Circle { Id = id, Title = title });
							}
						}
						rdr.Close ();
					}
				}
				// select members
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "select member from circle_members where circle_id = :cid";
					cmd.Parameters.Add("cid",NpgsqlDbType.Bigint);
					cmd.Prepare ();
					foreach (Circle c in cc) {
						cmd.Parameters ["cid"].Value = c.Id;
						using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
							if (rdr.HasRows) {
								var res = new List<string> ();
								while (rdr.Read ()) {
									res.Add (rdr.GetString (0));
								}
								c.Members = res.ToArray ();
							}
							rdr.Close ();
						}
					}
				}
				cnx.Close ();
			}
			return cc;
		}

		#endregion

		string connectionString = null;
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

			connectionString = ConfigurationManager.ConnectionStrings [config ["connectionStringName"]].ConnectionString;
			applicationName = config["applicationName"] ?? "/";
		}

	}
}

