using System;
using System.Diagnostics;
using System.IO;
using Yavsc.Model.Admin;
using Npgsql.Web.Blog;
using System.Resources;
using System.Reflection;

namespace Yavsc.Admin
{
	/// <summary>
	/// Data manager.
	/// </summary>
	public class DataManager
	{
		DataAccess da; 
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Admin.DataManager"/> class.
		/// </summary>
		/// <param name="datac">Datac.</param>
		public DataManager (DataAccess datac)
		{
			da = datac;
		}
		/// <summary>
		/// Creates the backup.
		/// </summary>
		/// <returns>The backup.</returns>
		public Export CreateBackup ()
		{
			Environment.SetEnvironmentVariable("PGPASSWORD", da.Password);
			string fileName = da.BackupPrefix + "-" + DateTime.Now.ToString ("yyyyMMddhhmmss")+".tar";
			FileInfo ofi = new FileInfo (fileName);
			Export e = new Export ();
			e.FileName = ofi.FullName;
			/*
			Exec ("pg_dump", string.Format (
				"-wb -Z3 -f {0} -Ft -h {1} -U {2} -p {3} {4}",
				fileName, da.Host, da.Dbuser, da.Port, da.Dbname ),e);
				*/
			Exec ("pg_dump", string.Format (
				"-f {0} -Ft -h {1} -U {2} -p {3} {4}",
				fileName, da.Host, da.Dbuser, da.Port, da.Dbname ),e);
			return e;
		} 

		private void Exec(string name, string args, TaskOutput output)
		{
			ProcessStartInfo Pinfo = 
				new ProcessStartInfo (name,args);
			Pinfo.RedirectStandardError = true;
			Pinfo.RedirectStandardOutput = true;
			Pinfo.CreateNoWindow = true;
			Pinfo.UseShellExecute = false;
			using (Process p = new Process ()) {
				p.EnableRaisingEvents = true;
				p.StartInfo = Pinfo;
				p.Start ();
				p.WaitForExit ();
				output.Error = p.StandardError.ReadToEnd ();
				output.Message = p.StandardOutput.ReadToEnd ();
				output.ExitCode = p.ExitCode;
				p.Close ();
			}
		}
		/// <summary>
		/// Restore the specified fileName and dataOnly.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="dataOnly">If set to <c>true</c> data only.</param>
		public TaskOutput Restore (string fileName, bool dataOnly)
		{
			Environment.SetEnvironmentVariable("PGPASSWORD", da.Password);
			var t = new TaskOutput ();
			Exec ("pg_restore", (dataOnly?"-a ":"")+string.Format ( 
				"-1 -Ft -O -h {0} -U {1} -p {2} -d {3} {4}",
				da.Host, da.Dbuser, da.Port, da.Dbname, fileName ),t);
			/*
			Exec ("pg_restore", (dataOnly?"-a ":"")+string.Format ( 
				"-1 -w -Fd -O -h {0} -U {1} -p {2} -d {3} {4}",
				da.Host, da.Dbuser, da.Port, da.Dbname, fileName ),t);
			*/
			return t;
		}
		/// <summary>
		/// Creates the db.
		/// </summary>
		/// <returns>The db.</returns>
		public TaskOutput CreateDb ()
		{
			TaskOutput res = new TaskOutput ();

			string sql;
			try {
				Assembly a =  System.Reflection.Assembly.GetExecutingAssembly();
				using (Stream sqlStream = a.GetManifestResourceStream("Yavsc.instdbws.sql"))
				{
					try { using (StreamReader srdr = new StreamReader (sqlStream)) {
						sql  = srdr.ReadToEnd ();
						using (var cnx = new Npgsql.NpgsqlConnection (da.ConnectionString)) {
							using (var cmd = cnx.CreateCommand ()) {
								cmd.CommandText = sql;
								cnx.Open();
								cmd.ExecuteNonQuery();
								cnx.Close();
								}
						}
						} } catch (Exception exg) {
						res.ExitCode = 1;
						res.Error = 
							string.Format ("Exception of type {0} occred retrieving the script",
								exg.GetType ().Name);
						res.Message = exg.Message;
					}
				}
			}
			catch (Exception ex) {
				res.ExitCode = 1;
				res.Error = 
					string.Format ("Exception of type {0} occured during the script execution",
					ex.GetType ().Name);
				res.Message = ex.Message;
			}

			return res;
		}
		/// <summary>
		/// Tags the backup.
		/// </summary>
		/// <returns>The backup.</returns>
		/// <param name="filename">Filename.</param>
		/// <param name="tags">Tags.</param>
		public Export TagBackup (string filename, string [] tags)
		{
			/* FileInfo fi = new FileInfo (filename);
			using (FileStream s = fi.OpenWrite ()) {

			} */
				throw new NotImplementedException ();
		}
		/// <summary>
		/// Tags the restore.
		/// </summary>
		/// <returns>The restore.</returns>
		/// <param name="fileName">File name.</param>
		public TaskOutput TagRestore (string fileName)
		{
			Environment.SetEnvironmentVariable ("PGPASSWORD", da.Password);
			var t = new TaskOutput ();
			Exec ("pg_restore", string.Format ( 
				"-a -w -Fd -O -h {0} -U {1} -p {2} -d {3} {4}",
				da.Host, da.Dbuser, da.Port, da.Dbname, fileName ),t);
			return t;
		}
	}
}

