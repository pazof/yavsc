using System;
using System.Web.Security;
using System.Configuration.Provider;
using System.Configuration;
using Npgsql;
using System.Collections.Generic;


using System.Linq;
using System.Collections.Specialized;

namespace Npgsql.Web
{
	/// <summary>
	/// Npgsql role provider.
	/// </summary>
	public class NpgsqlRoleProvider: RoleProvider
	{
		/// <summary>
		/// The name.
		/// </summary>
		protected string name = "NpgsqlRoleProvider";
		/// <summary>
		/// The name of the connection string.
		/// </summary>
		protected string connectionStringName = "pgProvider";
		/// <summary>
		/// The name of the application.
		/// </summary>
		protected string applicationName = "/";
		/// <summary>
		/// The connection string.
		/// </summary>
		protected string connectionString = string.Empty;

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
		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Adds the users to roles.
		/// </summary>
		/// <param name="usernames">Usernames.</param>
		/// <param name="roleNames">Role names.</param>
		public override void AddUsersToRoles (string[] usernames, string[] roleNames)
		{
			if (usernames.Any (x => x == null) || roleNames.Any (x => x == null)) {
				throw new ArgumentNullException ();
			}
			if (usernames.Any (x => x.Trim () == string.Empty) || (roleNames.Any (x => x.Trim () == string.Empty))) {
				throw new ArgumentException ("One or more of the supplied usernames or role names are empty.");
			}


			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "insert into usersroles (applicationname, username, rolename) values (@appname,@user,@role)";
					comm.Parameters.AddWithValue ("appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					NpgsqlParameter pu = comm.Parameters.AddWithValue ("user", NpgsqlTypes.NpgsqlDbType.Varchar, 250);
					NpgsqlParameter pr = comm.Parameters.AddWithValue ("role", NpgsqlTypes.NpgsqlDbType.Varchar, 250);
					foreach (string u in usernames) {
						pu.Value = u;
						foreach (string r in roleNames) {
							pr.Value = r;
							comm.ExecuteNonQuery ();
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

		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Creates the role.
		/// </summary>
		/// <param name="roleName">Role name.</param>
		public override void CreateRole (string roleName)
		{
			if (roleName == null)
				throw new ArgumentNullException ();
			if (roleName.Trim () == string.Empty)
				throw new ArgumentException ("A role name cannot be empty.");
			if (roleName.Contains (","))
				throw new ArgumentException ("A role name cannot contain commas.  Blame Microsoft for that rule!");
			if (roleName.Length > 250)
				throw new ArgumentException ("The maximum length for a Role name is 250 characters.");


			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "insert into roles (rolename, applicationname, comment) values (@rolename, @appname, @comment)";
					comm.Parameters.AddWithValue ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.Parameters.AddWithValue ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					comm.Parameters.AddWithValue ("@comment", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.ExecuteNonQuery ();
				}
			}

		}

		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Deletes the role.
		/// </summary>
		/// <returns><c>true</c>, if role was deleted, <c>false</c> otherwise.</returns>
		/// <param name="roleName">Role name.</param>
		/// <param name="throwOnPopulatedRole">If set to <c>true</c> throw on populated role.</param>
		public override bool DeleteRole (string roleName, bool throwOnPopulatedRole)
		{
			if (roleName == null)
				throw new ArgumentNullException ();
			if (roleName.Trim () == string.Empty)
				throw new ArgumentException ("The specified role name cannot be empty.");
			if (throwOnPopulatedRole)
			if (FindUsersInRole (roleName, "").Count () > 0)
				throw new ProviderException (
						string.Format ("The role {0} is populated, we cannot delete it.", roleName));

			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "delete from roles where rolename = @rolename and applicationname = @appname";
					comm.Parameters.AddWithValue ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.Parameters.AddWithValue ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					comm.Parameters.AddWithValue ("@comment", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.ExecuteNonQuery ();
				}
			}
			return true;
		}

		/// <summary>
		/// Finds the users in role.
		/// </summary>
		/// <returns>The users in role.</returns>
		/// <param name="roleName">Role name.</param>
		/// <param name="usernameToMatch">Username to match.</param>
		public override string[] FindUsersInRole (string roleName, string usernameToMatch)
		{
			return GetUsersInRole (roleName, usernameToMatch);
		}

		/// <summary>
		/// Gets the users in role.
		/// </summary>
		/// <returns>The users in role.</returns>
		/// <param name="rolename">Rolename.</param>
		/// <param name="usernameToMatch">Username to match.</param>
		protected string[] GetUsersInRole (string rolename, string usernameToMatch)
		{
			if (rolename == null)
				throw new ArgumentNullException ();
			if (rolename == string.Empty)
				throw new ProviderException ("Cannot look for blank role names.");
			usernameToMatch = usernameToMatch ?? string.Empty;
			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "select username from usersroles where applicationname = @appname " +
						"and rolename = @rolename and username like @username";
					comm.Parameters.AddWithValue ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = rolename;
					comm.Parameters.AddWithValue ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					comm.Parameters.AddWithValue ("@username", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = usernameToMatch;
					using (var reader = comm.ExecuteReader()) {
						var r = new List<string> ();
						var usernameColumn = reader.GetOrdinal ("username");
						while (reader.Read()) {
							r.Add (reader.GetString (usernameColumn));
						}
						return r.ToArray ();
					}
				}
			}
		}

		/// <summary>
		/// Gets all roles.
		/// </summary>
		/// <returns>The all roles.</returns>
		public override string[] GetAllRoles ()
		{
			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {

					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "select rolename from roles where applicationname = @appname";
					comm.Parameters.AddWithValue ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					using (var reader = comm.ExecuteReader()) {
						var r = new List<string> ();
						var rolenameColumn = reader.GetOrdinal ("rolename");
						while (reader.Read()) {
							r.Add (reader.GetString (rolenameColumn));
						}
						return r.ToArray ();
					}
				}
			}
		}

		/// <summary>
		/// Gets the roles for user.
		/// </summary>
		/// <returns>The roles for user.</returns>
		/// <param name="username">Username.</param>
		public override string[] GetRolesForUser (string username)
		{
			if (username == null)
				throw new ArgumentNullException ();
			if (username.Trim () == string.Empty)
				throw new ArgumentException ("The specified username cannot be blank.");
			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "select rolename from usersroles where applicationname = @appname and username = @username";
					comm.Parameters.AddWithValue ("@username", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
					comm.Parameters.AddWithValue ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					using (var reader = comm.ExecuteReader()) {
						var r = new List<string> ();
						var rolenameColumn = reader.GetOrdinal ("rolename");
						while (reader.Read()) {
							r.Add (reader.GetString (rolenameColumn));
						}
						return r.ToArray ();
					}
				}
			}
		}


		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Gets the users in role.
		/// </summary>
		/// <returns>The users in role.</returns>
		/// <param name="roleName">Role name.</param>
		public override string[] GetUsersInRole (string roleName)
		{
			if (string.IsNullOrEmpty (roleName))
				throw new ArgumentException ("The specified role name cannot be blank or null");
			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {
					//
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "select username from usersroles where applicationname = @appname " +
						"and rolename = @rolename";
					comm.Parameters.AddWithValue ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 255).Value = roleName;
					comm.Parameters.AddWithValue ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 255).Value = applicationName;
					using (var reader = comm.ExecuteReader()) {
						var r = new List<string> ();
						var usernameColumn = reader.GetOrdinal ("username");
						while (reader.Read()) {
							r.Add (reader.GetString (usernameColumn));
						}
						return r.ToArray ();
					}
				}
			}
		}

		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Determines whether this instance is user in role the specified username roleName.
		/// </summary>
		/// <returns><c>true</c> if this instance is user in role the specified username roleName; otherwise, <c>false</c>.</returns>
		/// <param name="username">Username.</param>
		/// <param name="roleName">Role name.</param>
		public override bool IsUserInRole (string username, string roleName)
		{
			if (username == null || roleName == null)
				throw new ArgumentNullException ();
			if (username.Trim () == string.Empty)
				throw new ArgumentException ("The specified username cannot be blank.");
			if (roleName.Trim () == string.Empty)
				throw new ArgumentException ("The specified role name cannot be blank.");

			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {
					//
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "select count(*)>0 from usersroles where applicationname = @appname " +
						"and username = @username and rolename = @rolename";
					comm.Parameters.AddWithValue ("@username", username);
					comm.Parameters.AddWithValue ("@rolename", roleName);
					comm.Parameters.AddWithValue ("@appname", applicationName);
					var retval = (bool)comm.ExecuteScalar ();
					return retval;
				}
			}

		}

		/// <Docs>To be added.</Docs>
		/// <summary>
		/// Removes the users from roles.
		/// </summary>
		/// <param name="usernames">Usernames.</param>
		/// <param name="roleNames">Role names.</param>
		public override void RemoveUsersFromRoles (string[] usernames, string[] roleNames)
		{
			if (usernames.Any (x => x == null) || roleNames.Any (x => x == null)) {
				throw new ArgumentNullException ();
			}
			if (usernames.Any (x => x.Trim () == string.Empty) || (roleNames.Any (x => x.Trim () == string.Empty))) {
				throw new ArgumentException ("One or more of the supplied usernames or role names are empty.");
			}

			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "delete from usersroles where applicationname = @appname and " +
						"username = @username and rolename = @rolename";
					NpgsqlParameter pu = comm.Parameters.AddWithValue ("@username", NpgsqlTypes.NpgsqlDbType.Varchar, 250);
					NpgsqlParameter pr = comm.Parameters.AddWithValue ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250);
					comm.Parameters.AddWithValue ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					foreach (string rolename in roleNames) {
						pr.Value = rolename;
						foreach (string username in usernames) {
							pu.Value = username;
							comm.ExecuteNonQuery ();
						}
					}
				}
			}
			
		}

		/// <Docs>Tests if a given role name exists.</Docs>
		/// <summary>
		/// Tests if a given role name exists.
		/// </summary>
		/// <returns><c>true</c>, if exists was roled, <c>false</c> otherwise.</returns>
		/// <param name="roleName">Role name.</param>
		public override bool RoleExists (string roleName)
		{
			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = new NpgsqlCommand("role_exists", conn)) {
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "select Count(*)>0 from roles where applicationname = @applicationname and rolename = @rolename";
					comm.Parameters.AddWithValue ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.Parameters.AddWithValue ("@applicationname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					var retval = (bool)comm.ExecuteScalar ();
					return retval;
				}
			}
		}
		/// <summary>
		/// Gets the name of this provider, 
		/// should correspond to the item key
		/// in the configuration collection of providers.
		/// </summary>
		/// <value>The name.</value>
		public override string Name {
			get {
				return name;
			}
		}
		/// <summary>
		/// Gets the description for this provider.
		/// </summary>
		/// <value>The description.</value>
		public override string Description {
			get {
				return "PostgreSQL ASP.Net Role Provider class";
			}
		}
	}
}

