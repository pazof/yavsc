using System;
using System.Configuration;
using System.Web.Profile;
using Npgsql;

namespace Npgsql.Web
{
	/// <summary>
	/// Npgsql profile provider.
	/// </summary>
	public class NpgsqlProfileProvider: ProfileProvider
	{
		private string connectionString;
		private string applicationName;
		/// <summary>
		/// Initializes a new instance of the <see cref="Npgsql.Web.NpgsqlProfileProvider"/> class.
		/// </summary>
		public NpgsqlProfileProvider ()
		{
		}

		/// <summary>
		/// Initialize the specified iname and config.
		/// </summary>
		/// <param name="iname">Iname.</param>
		/// <param name="config">Config.</param>
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
		/// <summary>
		/// Deletes the inactive profiles.
		/// </summary>
		/// <returns>The inactive profiles.</returns>
		/// <param name="authenticationOption">Authentication option.</param>
		/// <param name="userInactiveSinceDate">User inactive since date.</param>
		public override int DeleteInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new System.NotImplementedException ();
		}
		/// <summary>
		/// Deletes the profiles.
		/// </summary>
		/// <returns>The profiles.</returns>
		/// <param name="usernames">Usernames.</param>
		public override int DeleteProfiles (string[] usernames)
		{
			throw new System.NotImplementedException ();
		}
		/// <summary>
		/// Deletes the profiles.
		/// </summary>
		/// <returns>The profiles.</returns>
		/// <param name="profiles">Profiles.</param>
		public override int DeleteProfiles (ProfileInfoCollection profiles)
		{
			throw new System.NotImplementedException ();
		}
		/// <Docs>To be added.</Docs>
		/// <param name="userInactiveSinceDate">To be added.</param>
		/// <param name="pageSize">To be added.</param>
		/// <summary>
		/// To be added.
		/// </summary>
		/// <remarks>To be added.</remarks>
		/// <returns>The inactive profiles by user name.</returns>
		/// <param name="authenticationOption">Authentication option.</param>
		/// <param name="usernameToMatch">Username to match.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="totalRecords">Total records.</param>
		public override ProfileInfoCollection FindInactiveProfilesByUserName (ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new System.NotImplementedException ();
		}
		/// <Docs>To be added.</Docs>
		/// <param name="pageIndex">To be added.</param>
		/// <param name="totalRecords">To be added.</param>
		/// <returns>To be added.</returns>
		/// <since version=".NET 2.0"></since>
		/// <summary>
		/// Finds the name of the profiles by user.
		/// </summary>
		/// <param name="authenticationOption">Authentication option.</param>
		/// <param name="usernameToMatch">Username to match.</param>
		/// <param name="pageSize">Page size.</param>
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
					cmd.Parameters.AddWithValue ("@username", usernameToMatch);
					cmd.Parameters.AddWithValue ("@appname", applicationName);
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
		/// <Docs>To be added.</Docs>
		/// <param name="pageIndex">To be added.</param>
		/// <param name="totalRecords">To be added.</param>
		/// <returns>To be added.</returns>
		/// <since version=".NET 2.0"></since>
		/// <summary>
		/// Gets all inactive profiles.
		/// </summary>
		/// <param name="authenticationOption">Authentication option.</param>
		/// <param name="userInactiveSinceDate">User inactive since date.</param>
		/// <param name="pageSize">Page size.</param>
		public override ProfileInfoCollection GetAllInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new System.NotImplementedException ();
		}
		/// <Docs>To be added.</Docs>
		/// <param name="pageSize">To be added.</param>
		/// <summary>
		/// To be added.
		/// </summary>
		/// <remarks>To be added.</remarks>
		/// <returns>The all profiles.</returns>
		/// <param name="authenticationOption">Authentication option.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="totalRecords">Total records.</param>
		public override ProfileInfoCollection GetAllProfiles (ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new System.NotImplementedException ();
		}
		/// <summary>
		/// Gets the number of inactive profiles.
		/// </summary>
		/// <returns>The number of inactive profiles.</returns>
		/// <param name="authenticationOption">Authentication option.</param>
		/// <param name="userInactiveSinceDate">User inactive since date.</param>
		public override int GetNumberOfInactiveProfiles (ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new System.NotImplementedException ();
		}

		#endregion

		#region implemented abstract members of System.Configuration.SettingsProvider
		/// <summary>
		/// Gets the property values.
		/// </summary>
		/// <returns>The property values.</returns>
		/// <param name="context">Context.</param>
		/// <param name="collection">Collection.</param>
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
				cmd.Parameters.AddWithValue ("@username", username);
				cmd.Parameters.AddWithValue ("@appname", applicationName);
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
		/// <summary>
		/// Sets the property values.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="collection">Collection.</param>
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
					cmdpi.Parameters.AddWithValue ("@username", username);
					cmdpi.Parameters.AddWithValue ("@appname", applicationName);
				
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
							cmdpdins.Parameters.AddWithValue ("@puid", puid);
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
								cmd.Parameters.AddWithValue ("@puid", puid);
								cmd.Parameters.AddWithValue ("@val", s.PropertyValue);
								cmd.ExecuteNonQuery ();
							}
						}
					}
				}
			}
		}
		/// <summary>
		/// Gets or sets the name of the application.
		/// </summary>
		/// <value>The name of the application.</value>
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

