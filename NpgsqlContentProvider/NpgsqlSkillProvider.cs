//
//  NpgsqlSkillProvider.cs
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
using Yavsc.Model.Skill;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;

namespace WorkFlowProvider
{
	/// <summary>
	/// Npgsql skill provider.
	/// </summary>
	public class NpgsqlSkillProvider : SkillProvider
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WorkFlowProvider.NpgsqlSkillProvider"/> class.
		/// </summary>
		public NpgsqlSkillProvider ()
		{
			
		}
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

		#region implemented abstract members of SkillProvider
		/// <summary>
		/// Gets the user skills.
		/// </summary>
		/// <returns>The user skills.</returns>
		/// <param name="username">Username.</param>
		public override PerformerProfile GetUserSkills (string username)
		{
			var skills = new List <UserSkill>();
			var profile = new PerformerProfile (username);
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						" select u._id, u.skillid, s.name, " +
						" u.comment, u.rate from userskills u, " +
						" skill s " +
						" where u.skillid = s._id and " +
						" u.username = :uname " +
						" and applicationname = :app " +
						" order by u.rate desc";
					cmd.Parameters.AddWithValue ("uname", NpgsqlTypes.NpgsqlDbType.Varchar, username);
					cmd.Parameters.AddWithValue ("app", NpgsqlTypes.NpgsqlDbType.Varchar, applicationName);
					cmd.Prepare ();
					using (var rdr = cmd.ExecuteReader ()) { 
						if (rdr.HasRows) while (rdr.Read ()) { 
								skills.Add (new UserSkill () { 
									Id = rdr.GetInt64 (0),
									SkillId = rdr.GetInt64 (1),
									SkillName = rdr.GetString (2),
									Comment = rdr.GetString (3),
									Rate = rdr.GetInt32(4)
								});
							}
						profile.Skills = skills.ToArray ();
					}
				}
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"select uniqueid from profiles where username = :user and applicationname = :app";
					cmd.Parameters.AddWithValue ("user", NpgsqlTypes.NpgsqlDbType.Varchar, username);
					cmd.Parameters.AddWithValue ("app", NpgsqlTypes.NpgsqlDbType.Varchar, applicationName);
					profile.Id = (long) cmd.ExecuteScalar ();
				}
				cnx.Close ();
			}
			return profile;
		}

		/// <summary>
		/// Create the specified skill.
		/// </summary>
		/// <param name="skill">skill.</param>
		public override long Declare (Skill skill)
		{
			long res = 0;
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString)) {
				cnx.Open ();
				if (skill.Id == 0) {
					using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
						cmd.CommandText = "insert into skill (name,rate) values (:name,:rate) returning _id";
						cmd.Parameters.AddWithValue ("name", NpgsqlTypes.NpgsqlDbType.Varchar, skill.Name);
						cmd.Parameters.AddWithValue ("rate", 
							NpgsqlTypes.NpgsqlDbType.Integer, skill.Rate);
						res = (long)cmd.ExecuteScalar ();
					}
				} else {
					using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
						cmd.CommandText = "update skill set name = :name, rate = :rate where _id = :sid";
						cmd.Parameters.AddWithValue ("name", NpgsqlTypes.NpgsqlDbType.Bigint, skill.Id);
						cmd.Parameters.AddWithValue ("rate", 
							NpgsqlTypes.NpgsqlDbType.Integer, skill.Rate);
						cmd.ExecuteNonQuery ();
					}
				}
				cnx.Close ();
			}
			return res;
		}

		/// <summary>
		/// Declares the userskill.
		/// </summary>
		/// <returns>The userskill.</returns>
		/// <param name="userskill">userskill.</param>
		public override long Declare (UserSkillDeclaration userskill)
		{
			long res=0;
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString)) {
				cnx.Open ();
				if (userskill.Id == 0) {
					using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
						cmd.CommandText = "insert into userskills" +
						" (username, applicationname, skillid, rate, comment) " +
						" values (:uname,:app,:sid,:rate,:cmnt) returning _id";
						cmd.Parameters.AddWithValue ("uname", 
							NpgsqlTypes.NpgsqlDbType.Varchar, userskill.UserName);
						cmd.Parameters.AddWithValue ("app", 
							NpgsqlTypes.NpgsqlDbType.Varchar, applicationName);
						cmd.Parameters.AddWithValue ("sid", 
							NpgsqlTypes.NpgsqlDbType.Bigint, userskill.SkillId);
						cmd.Parameters.AddWithValue ("rate", 
							NpgsqlTypes.NpgsqlDbType.Integer, userskill.Rate);
						cmd.Parameters.AddWithValue ("cmnt", 
							NpgsqlTypes.NpgsqlDbType.Varchar, userskill.Comment);
						userskill.Id = res = (long)cmd.ExecuteScalar ();
					}
				} else {
					using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
						cmd.CommandText = "update userskills" +
						" set  rate = :rate," +
						" comment = :cmnt) " +
						" where _id = :usid ";
						cmd.Parameters.AddWithValue ("comment", 
							NpgsqlTypes.NpgsqlDbType.Varchar, userskill.Comment);
						cmd.Parameters.AddWithValue ("rate", 
							NpgsqlTypes.NpgsqlDbType.Integer, userskill.Rate);
						cmd.Parameters.AddWithValue ("usid", 
							NpgsqlTypes.NpgsqlDbType.Bigint, userskill.Id);
						cmd.ExecuteNonQuery ();
					}
				}
				cnx.Close ();
			}
			return res;
		}

		/// <summary>
		/// Rate the specified user's skill.
		/// It creates the record describing the user's skill
		/// if not existent.
		/// </summary>
		/// <param name="userSkill">UserSkillRating.</param>
		public override long Rate (UserSkillRating userSkill)
		{
			// TODO Use the Author value to choose 
			// between a self rating that goes into the `userskills` table
			// and a client rating that goes into the
			// `statisfaction` table.
			long usid = 0;
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					if (userSkill.Id == 0) {
						cmd.CommandText = "insert into userskills " +
							" ( skillid, rate, username, applicationname ) " +
							" values ( :sid, :rate, :uname, :app ) " +
							" returning _id ";
						cmd.Parameters.AddWithValue ("sid", NpgsqlDbType.Bigint, userSkill.Id);
						cmd.Parameters.AddWithValue ("rate", NpgsqlDbType.Integer, userSkill.Rate);
						cmd.Parameters.AddWithValue ("uname", NpgsqlDbType.Varchar, userSkill.Performer);
						cmd.Parameters.AddWithValue ("app", NpgsqlDbType.Varchar, applicationName);
						usid = (long) cmd.ExecuteScalar ();
					} else {
						cmd.CommandText = "update userskills " +
							" set rate = :rate " +
							" where _id = :usid ";
						cmd.Parameters.AddWithValue ("rate", NpgsqlDbType.Integer, userSkill.Rate);
						cmd.Parameters.AddWithValue ("usid", NpgsqlDbType.Bigint, userSkill.Id);
						cmd.ExecuteNonQuery ();
					}

				}
				cnx.Close ();
			}
			return usid;
		}

		/// <summary>
		/// Rate the specified skill.
		/// The access to this method 
		/// should be restricted to an Admin,
		/// or a rating engine
		/// </summary>
		/// <param name="skill">Skill.</param>
		public override void Rate (SkillRating skill)
		{
			// TODO Use the Author value to choose 
			// between a global setting for the application
			// and an user setting on his needs

			// when the `Author` value is not null,
			// it's concerning a rating on a need of the Author
			// if not, it's a need of the site

			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "update skill set rate = :rate where _id = :sid";
					cmd.Parameters.AddWithValue ("sid", NpgsqlTypes.NpgsqlDbType.Bigint, skill.Id);
					cmd.Parameters.AddWithValue ("rate", NpgsqlTypes.NpgsqlDbType.Integer, skill.Rate);
					cmd.ExecuteNonQuery ();
				}
				cnx.Close ();
			}
		}
		/// <summary>
		/// Finds the skill identifier.
		/// </summary>
		/// <returns>The skill identifier.</returns>
		/// <param name="pattern">Pattern.</param>
		public override Skill[] FindSkill (string pattern)
		{
			List<Skill> skills = new List<Skill> ();
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "select _id, name, rate from skill where name like :name order by rate desc";
					cmd.Parameters.AddWithValue ("name", NpgsqlTypes.NpgsqlDbType.Varchar, pattern);
					cmd.Prepare ();
					using (var rdr = cmd.ExecuteReader ()) {
						if (rdr.HasRows) while (rdr.Read ()) {
							skills.Add (new Skill () { 
								Id = (long)rdr.GetInt64 (0),
								Name = (string)rdr.GetString (1),
								Rate = (int) rdr.GetInt32(2)
							});
						}
					}
				}
				cnx.Close ();
			}
			return skills.ToArray ();
		}

		/// <summary>
		/// Finds the performer.
		/// </summary>
		/// <returns>The performer.</returns>
		/// <param name="skillIds">Skill identifiers.</param>
		public override string[] FindPerformer (long[] skillIds)
		{
			var res = new List<string> ();

			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					
					cmd.CommandText = " select username from userskills " +
						" where skillid = :sid " +
						" order by rate desc ";
					cmd.Parameters.AddWithValue ("sid", NpgsqlDbType.Bigint, 0);
					cmd.Prepare ();

					foreach ( long sid in skillIds )
					{
						cmd.Parameters ["sid"].Value = sid;
						using (var rdr = cmd.ExecuteReader ()) {
							string uname = rdr.GetString (0);
							if (!res.Contains(uname))
								res.Add (uname);
						}
					}
				}
				cnx.Close ();
			}
			return res.ToArray ();
		}

		/// <summary>
		/// Deletes the skill.
		/// </summary>
		/// <param name="skillId">Skill identifier.</param>
		public override void DeleteSkill (long skillId)
		{
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = " delete from skill " +
						" where _id = :sid ";
					cmd.Parameters.AddWithValue ("sid", NpgsqlTypes.NpgsqlDbType.Bigint,skillId);
					cmd.ExecuteNonQuery ();
				}
				cnx.Close ();
			}
		}

		/// <summary>
		/// Deletes the user skill.
		/// </summary>
		/// <param name="userSkillId">User skill identifier.</param>
		public override void DeleteUserSkill(long userSkillId) {
			using (NpgsqlConnection cnx=new NpgsqlConnection(connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = " delete from userskills " +
						" where _id = :usid ";
					cmd.Parameters.AddWithValue ("usid", NpgsqlTypes.NpgsqlDbType.Bigint,userSkillId);
					cmd.ExecuteNonQuery ();
				}
				cnx.Close ();
			}
		}
		#endregion
	}
}

