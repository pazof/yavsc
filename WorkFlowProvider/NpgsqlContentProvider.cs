using System;
using Npgsql;
using NpgsqlTypes;
using System.Configuration;
using System.Collections.Specialized;
using yavscModel.WorkFlow;

namespace WorkFlowProvider
{
	public class NpgsqlContentProvider:  IContentProvider
	{
		public string Order (IWFCommand c)
		{
			throw new NotImplementedException ();
		}

		public IContent Get (string orderId)
		{
			throw new NotImplementedException ();
		}

		public void AddDevRessource (int prjId, string userName)
		{
			throw new NotImplementedException ();
		}

		public void AddPrjRessource(int prjId, string owner)
		{
		}

		public void NewRelease (int projectId, string Version)
		{
			throw new NotImplementedException ();
		}

		string applicationName=null;
		string cnxstr = null;

		public NpgsqlContentProvider ()
		{
			Initialize("NpgsqlYavscContentProvider",ConfigurationManager.AppSettings);
		}

		public void Initialize (string name, NameValueCollection config)
		{
			cnxstr = ConfigurationManager.ConnectionStrings [config ["connectionStringName"]].ConnectionString;
			applicationName = config["applicationName"] ?? "/";
		}

		NpgsqlConnection CreateConnection ()
		{
			return new NpgsqlConnection (cnxstr);
		}

		#region IDisposable implementation
		public void Dispose ()
		{

		}
		#endregion

		#region IContentProvider implementation

		public int NewTask (int projectId, string name, string desc)
		{
			throw new System.NotImplementedException ();
		}

		public void SetProjectName (int projectId, string name)
		{
			throw new System.NotImplementedException ();
		}

		public void SetProjectDesc (int projectId, string desc)
		{
			throw new System.NotImplementedException ();
		}

		public void SetTaskName (int taskId, string name)
		{
			throw new System.NotImplementedException ();
		}

		public void SetStartDate (int taskId, DateTime d)
		{
			throw new System.NotImplementedException ();
		}

		public void SetEndDate (int taskId, DateTime d)
		{
			throw new System.NotImplementedException ();
		}

		public void SetTaskDesc (int taskId, string desc)
		{
			throw new System.NotImplementedException ();
		}

		public void RemoveProject (int prjId)
		{
			using (var cnx = CreateConnection()) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand()) {
					cmd.CommandText = "delete from projets where id = @id";
					cmd.Parameters.Add ("@id", prjId);
					cmd.ExecuteNonQuery();
				}
				cnx.Close ();
			}
		}

		public void RemoveTask (int taskId)
		{
			throw new System.NotImplementedException ();
		}

		public void SetManager (int projectId, string user)
		{
			throw new System.NotImplementedException ();
		}

		public void RemoveUser (string user)
		{
			throw new System.NotImplementedException ();
		}

		public int NewProject (string name, string desc, string ownerId)
		{
			int id = 0;
			using (var cnx = CreateConnection()) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand()) {
					cmd.CommandText = "insert into projets (name,managerid,ApplicatonName,prdesc) values (@name,@mid,@appname,@pdesc)";
					cmd.Parameters.Add ("@name", name);
					cmd.Parameters.Add ("@mid", ownerId);
					cmd.Parameters.Add ("@appname", applicationName);
					cmd.Parameters.Add ("@desc", desc);
					id = (int)cmd.ExecuteScalar ();
				}
				cnx.Close ();
			}
			return id;
		}
		#endregion

	}
}

