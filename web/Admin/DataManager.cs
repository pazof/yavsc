using System;
using System.Diagnostics;
using System.IO;
using Yavsc.Model.Admin;
using Npgsql.Web.Blog;

namespace Yavsc.Admin
{
	public class DataManager
	{
		DataAccess da; 
		public DataManager (DataAccess datac)
		{
			da = datac;
		}

		public Export CreateBackup ()
		{
			Environment.SetEnvironmentVariable("PGPASSWORD", da.Password);
			Export e = new Export ();
			string fileName = da.BackupPrefix + "-" + DateTime.Now.ToString ("yyyyMMddhhmmss")+".tar";
			FileInfo ofi = new FileInfo (fileName);
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
		public TaskOutput CreateDb ()
		{
			return Restore ("freshinstall", false);
		}
		public Export TagBackup (string filename, string [] tags)
		{
			/* FileInfo fi = new FileInfo (filename);
			using (FileStream s = fi.OpenWrite ()) {

			} */
				throw new NotImplementedException ();
		}
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

