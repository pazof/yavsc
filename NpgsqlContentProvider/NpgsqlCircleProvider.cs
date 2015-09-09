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
		#region implemented abstract members of CircleProvider
		/// <summary>
		/// Updates the circle.
		/// </summary>
		/// <param name="c">C.</param>
		public override void UpdateCircle (CircleBase c)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "update circle " +
						"set title = :title " +
						"where _id = :cid ";
					cmd.Parameters.AddWithValue ("title", c.Title);
					cmd.Parameters.AddWithValue ("cid", c.Id);
					cmd.ExecuteNonQuery ();
				}
				cnx.Close ();
			}
		}
		/// <summary>
		/// Get the specified circle by id, including all of its members.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <returns>The members.</returns>
		public override Circle GetMembers (long id)
		{
			Circle circ = null;

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
		/// Gets the identifier.
		/// </summary>
		/// <returns>The identifier.</returns>
		/// <param name="circle">Circle.</param>
		/// <param name="username">Username.</param>
		public override long GetId (string circle, string username)
		{
			long cid = 0;
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "select _id from circle where " +
					"owner = :uname " +
					"and title = :title " +
					"and applicationname = :appname";

					cmd.Parameters.AddWithValue ("uname", username);
					cmd.Parameters.AddWithValue ("title", circle);
					cmd.Parameters.AddWithValue ("appname", applicationName);
					cid = (long)cmd.ExecuteScalar ();
				}
				cnx.Close ();
			}
			return cid;
		}
		/// <summary>
		/// Removes the membership.
		/// </summary>
		/// <param name="circle_id">Circle identifier.</param>
		/// <param name="member">Member.</param>
		public override void RemoveMembership (long circle_id, string member)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "delete from circle_members where circle_id = :cid and username = :uname";
				cmd.Parameters.AddWithValue ("cid", circle_id);
				cmd.Parameters.AddWithValue ("uname", member);
				cnx.Open ();
				cmd.ExecuteNonQuery ();	
				cnx.Close ();
			}
		}
		/// <summary>
		/// Removes the member from all current user circles.
		/// </summary>
		/// <param name="member">Member.</param>
		public override void RemoveMember (string member)
		{
			throw new NotImplementedException ();
		}

		#endregion

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
		public override bool Matches (long[] circle_ids, string member)
		{
			bool result = false;
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "select count(*)>0 from circle_members where circle_id = :cid and member = :mbr";
				cmd.Parameters.Add ("cid", NpgsqlDbType.Bigint);
				cmd.Parameters.AddWithValue ("mbr", member);
				cnx.Open ();
				cmd.Prepare ();
				foreach (long cid in circle_ids) {
					result = (bool)cmd.ExecuteScalar ();
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
		public override void AddMember (long id, string username)
		{
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "insert into circle_members (circle_id, member) values (:cid,:uname)";
				cmd.Parameters.AddWithValue ("cid", id);
				cmd.Parameters.AddWithValue ("uname", username);
				cnx.Open ();
				cmd.ExecuteNonQuery ();	
				cnx.Close ();
			}
		}

		/// <summary>
		/// Get the specified circle by id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public override CircleBase Get (long id)
		{
			CircleBase circ = null;
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "select title, owner from circle where _id = :cid";
					cmd.Parameters.AddWithValue ("cid", id);
					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
							if (rdr.Read ()) {
								circ = new CircleBase ();
								circ.Id = id;
								circ.Owner = rdr.GetString (1);
								circ.Title = rdr.GetString (0);
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
					id = (long)cmd.ExecuteScalar ();
				}
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "insert into circle_members (circle_id,member) values (@cid,@mbr)";
					cmd.Parameters.AddWithValue ("cid", id);
					cmd.Parameters.Add ("mbr", NpgsqlDbType.Varchar);
					cmd.Prepare ();
					if (users != null)
						foreach (string user in users) {
							cmd.Parameters [1].Value = user;
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
				cmd.Parameters.AddWithValue ("cid", id);
				cnx.Open ();
				cmd.ExecuteNonQuery ();	
				cnx.Close ();
			}
		}

		/// <summary>
		/// List user's circles.
		/// </summary>
		/// <param name="user">User.</param>
		public override IEnumerable<CircleBase> List (string user)
		{
			List<CircleBase> cc = new List<CircleBase> ();
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "select _id, title from circle where owner = :wnr";
					cmd.Parameters.AddWithValue ("wnr", user);
					cnx.Open ();
					cmd.Prepare ();
					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						if (rdr.HasRows) {
							while (rdr.Read ()) {
								CircleBase cb = new CircleBase ();
								cb.Id = rdr.GetInt64 (0);
								cb.Title = rdr.GetString (1);
								cb.Owner = user;
								cc.Add (cb);
							}
						}
						rdr.Close ();
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
			if (string.IsNullOrWhiteSpace (config ["connectionStringName"]))
				throw new ConfigurationErrorsException ("No name for Npgsql connection string found");

			connectionString = ConfigurationManager.ConnectionStrings [config ["connectionStringName"]].ConnectionString;
			applicationName = config ["applicationName"] ?? "/";
		}

	}
}

