using System;
using System.Configuration;
using System.Web.Profile;
using Npgsql;

namespace Npgsql.Web
{
	public class NpgsqlProfileProvider: ProfileProvider
	{
		private string connectionString;
		private string applicationName;

		public NpgsqlProfileProvider ()
		{
		}

		public override void Initialize (string iname, System.Collections.Specialized.NameValueCollection config)
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

		#region implemented abstract members of System.Web.Profile.ProfileProvider

		public override int DeleteInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new System.NotImplementedException ();
		}

		public override int DeleteProfiles (string[] usernames)
		{
			throw new System.NotImplementedException ();
		}

		public override int DeleteProfiles (ProfileInfoCollection profiles)
		{
			throw new System.NotImplementedException ();
		}

		public override ProfileInfoCollection FindInactiveProfilesByUserName (ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new System.NotImplementedException ();
		}

		public override ProfileInfoCollection FindProfilesByUserName (ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex");
			if (pageSize < 1)
				throw new ArgumentException ("pageSize");

			long lowerBound = (long)pageIndex * pageSize;
			long upperBound = lowerBound + pageSize - 1;
			if (upperBound > Int32.MaxValue)
				throw new ArgumentException ("lowerBound + pageSize*pageIndex -1 > Int32.MaxValue");
			ProfileInfoCollection c = new ProfileInfoCollection ();
			totalRecords = 0;

			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "select username, uniqueid, lastactivitydate, lastupdateddate, isanonymous from profiles where username like @username and applicationname = @appname";
					cmd.Parameters.Add ("@username", usernameToMatch);
					cmd.Parameters.Add ("@appname", applicationName);
					cnx.Open ();
					using (NpgsqlDataReader r = cmd.ExecuteReader ()) {
						if (r.HasRows) {
							while (r.Read ()) {
								if (totalRecords >= lowerBound && totalRecords <= upperBound) {

									object o = r.GetValue (r.GetOrdinal ("isanonymous"));
									bool isanon = o is DBNull ? true : (bool) o;
									o = r.GetValue (r.GetOrdinal ("lastactivitydate"));
									DateTime lact = o is DBNull ? new DateTime() : (DateTime) o;
									o = r.GetValue (r.GetOrdinal ("lastupdateddate"));
									DateTime lupd = o is DBNull ? new DateTime() : (DateTime) o;
									ProfileInfo pi =
										new ProfileInfo (
											r.GetString (r.GetOrdinal ("username")),
											isanon,
											lact,
											lupd,
											0);
									c.Add (pi);
									totalRecords++;
								}
							}
						}
					}
				}

			}
			return c;
		}

		public override ProfileInfoCollection GetAllInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new System.NotImplementedException ();
		}

		public override ProfileInfoCollection GetAllProfiles (ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new System.NotImplementedException ();
		}

		public override int GetNumberOfInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new System.NotImplementedException ();
		}

		#endregion

		#region implemented abstract members of System.Configuration.SettingsProvider

		public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context, SettingsPropertyCollection collection)
		{
			SettingsPropertyValueCollection c = new SettingsPropertyValueCollection ();
			if (collection == null || collection.Count < 1 || context == null)
				return c;
			string username = (string)context ["UserName"];
			if (String.IsNullOrEmpty (username))
				return c;
			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString))
			using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
				cmd.CommandText = "SELECT * from profiledata,profiles where " +
				"profiledata.uniqueid = profiles.uniqueid " +
				"and profiles.username = @username " +
				"and profiles.applicationname = @appname";
				cmd.Parameters.Add ("@username", username);
				cmd.Parameters.Add ("@appname", applicationName);
				cnx.Open ();
				using (NpgsqlDataReader r = cmd.ExecuteReader (
					                            System.Data.CommandBehavior.CloseConnection | System.Data.CommandBehavior.SingleRow)) {
					if (r.Read ()) {
						foreach (SettingsProperty p in collection) {
							SettingsPropertyValue v = new SettingsPropertyValue (p);
							int o = r.GetOrdinal (p.Name.ToLower ());
							v.PropertyValue = r.GetValue (o);
							c.Add (v);
						}
					} else {
						foreach (SettingsProperty p in collection) {
							SettingsPropertyValue v = new SettingsPropertyValue (p);
							v.PropertyValue = null;
							c.Add (v);
						}
					}
				}
			}
			return c;

		}

		public override void SetPropertyValues (SettingsContext context, SettingsPropertyValueCollection collection)
		{
			// get the unique id of the profile
			if (collection == null)
				return;
			long puid = 0;
			string username = (string)context ["UserName"];

			using (NpgsqlConnection cnx = new NpgsqlConnection (connectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmdpi = cnx.CreateCommand ()) {
					cmdpi.CommandText = "select count(uniqueid) " +
					"from profiles where username = @username " +
					"and applicationname = @appname";
					cmdpi.Parameters.Add ("@username", username);
					cmdpi.Parameters.Add ("@appname", applicationName);
				
					long c = (long)cmdpi.ExecuteScalar ();
					if (c == 0) {
						cmdpi.CommandText = "insert into profiles (username,applicationname) " +
						"values ( @username, @appname ) " +
						"returning uniqueid";
						puid = (long)cmdpi.ExecuteScalar ();
						// TODO spec: profiledata insertion <=> profile insertion
						// => BAD DESIGN
						// 
						using (NpgsqlCommand cmdpdins = cnx.CreateCommand ()) {
							cmdpdins.CommandText = "insert into profiledata (uniqueid) values (@puid)";
							cmdpdins.Parameters.Add ("@puid", puid);
							cmdpdins.ExecuteNonQuery ();
						}
					} else {
						cmdpi.CommandText = "select uniqueid from profiles where username = @username " +
						"and applicationname = @appname";
						puid = (long)cmdpi.ExecuteScalar ();
					}
				}


				foreach (SettingsPropertyValue s in collection) {
					if (s.UsingDefaultValue) {
						//TODO Drop the property in the profile
					} else {
						// update the property value
						// TODO update to null values (included to avoid Not Implemented columns in profiledata
						if (s.PropertyValue != null) {
							using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
								cmd.CommandText = string.Format (
									"update profiledata " +
									"set {0} = @val " +
									"where uniqueid = @puid ",
									s.Name
								);
								cmd.Parameters.Add ("@puid", puid);
								cmd.Parameters.Add ("@val", s.PropertyValue);
								cmd.ExecuteNonQuery ();
							}
						}
					}
				}
			}
		}

		public override string ApplicationName {
			get {
				return applicationName;
			}
			set {
				applicationName = value;
			}
		}

		#endregion

	}
}

