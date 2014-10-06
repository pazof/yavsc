using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Admin
{
	public class DataAccess {
		private  string host = "localhost";

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

		public int Port {
			get {
				return port;
			}
			set {
				port = value;
			}
		}

		private  string dbname = "yavsc";

		public string Dbname {
			get {
				return dbname;
			}
			set {
				dbname = value;
			}
		}

		private  string dbuser = "postgres";

		public string Dbuser {
			get {
				return dbuser;
			}
			set {
				dbuser = value;
			}
		}

		private  string dbpassword ;
		private  string backupPrefix= "backup/global.backup";
		 
		public string BackupPrefix {
			get {
				return backupPrefix;
			}
			set {
				backupPrefix = value;
			}
		}

		[Required(ErrorMessage ="Please, specify a password")]
		public  string Password {
			get { return dbpassword; }
			set { dbpassword = value; }
		}

		public string [] GetBackupDirs()
		{
			List<string> res = new List<string> ();
			string bkpdir = new FileInfo (backupPrefix).DirectoryName;
			DirectoryInfo bkpdiri = new DirectoryInfo(bkpdir);
			foreach (DirectoryInfo di in bkpdiri.EnumerateDirectories())
				res.Add (Path.Combine(bkpdir,di.Name));
			return res.ToArray ();
		}
	}
	
}
