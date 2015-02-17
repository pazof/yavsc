using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Admin
{
	/// <summary>
	/// Data access.
	/// </summary>
	public class DataAccess {
		private  string host = "localhost";

		/// <summary>
		/// Gets or sets the host.
		/// </summary>
		/// <value>The host.</value>
		[StringLength(2056)]
		public string Host {
			get {
				return host;
			}
			set {
				host = value;
			}
		}

		private  int port = 5432;

		/// <summary>
		/// Gets or sets the port.
		/// </summary>
		/// <value>The port.</value>
		public int Port {
			get {
				return port;
			}
			set {
				port = value;
			}
		}

		private  string dbname = "yavsc";

		/// <summary>
		/// Gets or sets the dbname.
		/// </summary>
		/// <value>The dbname.</value>
		public string Dbname {
			get {
				return dbname;
			}
			set {
				dbname = value;
			}
		}

		private  string dbuser = "postgres";

		/// <summary>
		/// Gets or sets the dbuser.
		/// </summary>
		/// <value>The dbuser.</value>
		public string Dbuser {
			get {
				return dbuser;
			}
			set {
				dbuser = value;
			}
		}

		private  string dbpassword ;
		private  string backupPrefix= "~/backup/global.backup";

		/// <summary>
		/// Gets or sets the backup prefix.
		/// </summary>
		/// <value>The backup prefix.</value>
		public string BackupPrefix {
			get {
				return backupPrefix;
			}
			set {
				backupPrefix = value;
			}
		}

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>The password.</value>
		[Required(ErrorMessage ="Please, specify a password")]
		public  string Password {
			get { return dbpassword; }
			set { dbpassword = value; }
		}
		/// <summary>
		/// Connections the string.
		/// </summary>
		/// <returns>The string.</returns>
		public string ConnectionString() {
			return string.Format ("Server={0};Port={1};Database={2};User Id={3};Password={4};Encoding=Unicode;",
				Host,Port,Dbuser,Password);
		}
	}
	
}
