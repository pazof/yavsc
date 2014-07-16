using System;
using System.Web.Security;
using System.Configuration.Provider;
using System.Configuration;
using Npgsql;
using System.Collections.Generic;

/*
 * 
CREATE TABLE roles
(
  rolename character varying(255) NOT NULL,
  applicationname character varying(255) NOT NULL,
  comment character varying(255) NOT NULL,
  CONSTRAINT roles_pkey PRIMARY KEY (rolename , applicationname )
)
WITH (
  OIDS=FALSE
);

CREATE TABLE usersroles
(
  applicationname character varying(255) NOT NULL,
  rolename character varying(255) NOT NULL,
  username character varying(255) NOT NULL,
  CONSTRAINT attrroles_pkey PRIMARY KEY (applicationname , rolename , username ),
  CONSTRAINT usersroles_fk_role FOREIGN KEY (applicationname, rolename)
      REFERENCES roles (applicationname, rolename) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT usersroles_fk_user FOREIGN KEY (applicationname, username)
      REFERENCES users (applicationname, username) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

 */
using System.Linq;


namespace Npgsql.Web
{
	public class NpgsqlRoleProvider: RoleProvider
	{
		protected string name = "NpgsqlRoleProvider";
		protected string connectionStringName = "pgProvider";
		protected string applicationName = "/";
		protected string connectionString = string.Empty;

		public override void Initialize (string iname, System.Collections.Specialized.NameValueCollection config)
		{
			try {

				name = iname ?? config ["name"];

				connectionStringName = config ["connectionStringName"] ?? connectionStringName;

				applicationName = config ["applicationName"] ?? applicationName;
		
				if (applicationName.Length > 250)
					throw new ProviderException ("The maximum length for an application name is 250 characters.");

				var cs = ConfigurationManager.ConnectionStrings [connectionStringName];
				if (cs == null || string.IsNullOrEmpty (cs.ConnectionString)) {
					throw new ProviderException (
						string.Format ("The role provider connection string, '{0}', is not defined.", connectionStringName));
				}

				connectionString = ConfigurationManager.ConnectionStrings [connectionStringName].ConnectionString;
				if (string.IsNullOrEmpty (connectionString))
					throw new ConfigurationErrorsException (
						string.Format (
						"The connection string for the given name ({0})" +
						"must be specified in the <connectionStrings>" +
						"configuration bloc. Aborting.", connectionStringName)
					);

			} catch (Exception ex) {
				var message = "Error initializing the role configuration settings";
				throw new ProviderException (message, ex);
			}
		}

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
					comm.Parameters.Add ("appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					NpgsqlParameter pu = comm.Parameters.Add ("user", NpgsqlTypes.NpgsqlDbType.Varchar, 250);
					NpgsqlParameter pr = comm.Parameters.Add ("role", NpgsqlTypes.NpgsqlDbType.Varchar, 250);
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

		public override string ApplicationName {
			get {
				return applicationName;
			}
			set {
				applicationName = value;
			}
		}

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
					comm.Parameters.Add ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.Parameters.Add ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					comm.Parameters.Add ("@comment", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.ExecuteNonQuery ();
				}
			}

		}

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
					comm.Parameters.Add ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.Parameters.Add ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					comm.Parameters.Add ("@comment", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.ExecuteNonQuery ();
				}
			}
			return true;
		}

		public override string[] FindUsersInRole (string roleName, string usernameToMatch)
		{
			return GetUsersInRole (roleName, usernameToMatch);
		}

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
					comm.Parameters.Add ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = rolename;
					comm.Parameters.Add ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					comm.Parameters.Add ("@username", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = usernameToMatch;
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

		public override string[] GetAllRoles ()
		{
			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = conn.CreateCommand()) {

					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "select rolename from roles where applicationname = @appname";
					comm.Parameters.Add ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
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
					comm.Parameters.Add ("@username", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
					comm.Parameters.Add ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
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
					comm.Parameters.Add ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 255).Value = roleName;
					comm.Parameters.Add ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 255).Value = applicationName;
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
					comm.Parameters.Add ("@username", username);
					comm.Parameters.Add ("@rolename", roleName);
					comm.Parameters.Add ("@appname", applicationName);
					var retval = (bool)comm.ExecuteScalar ();
					return retval;
				}
			}

		}

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
					NpgsqlParameter pu = comm.Parameters.Add ("@username", NpgsqlTypes.NpgsqlDbType.Varchar, 250);
					NpgsqlParameter pr = comm.Parameters.Add ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250);
					comm.Parameters.Add ("@appname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
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

		public override bool RoleExists (string roleName)
		{
			using (var conn = new NpgsqlConnection(connectionString)) {
				conn.Open ();
				using (var comm = new NpgsqlCommand("role_exists", conn)) {
					comm.CommandType = System.Data.CommandType.Text;
					comm.CommandText = "select Count(*)>0 from roles where applicationname = @applicationname and rolename = @rolename";
					comm.Parameters.Add ("@rolename", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.Parameters.Add ("@applicationname", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = applicationName;
					var retval = (bool)comm.ExecuteScalar ();
					return retval;
				}
			}
		}

		public override string Name {
			get {
				return name;
			}
		}

		public override string Description {
			get {
				return "PostgreSQL ASP.Net Role Provider class";
			}
		}
	}
}

