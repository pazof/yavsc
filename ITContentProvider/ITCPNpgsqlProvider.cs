using System;
using WorkFlowProvider;
using Npgsql;

namespace ITContentProvider
{
	public class ITCPNpgsqlProvider :NpgsqlContentProvider
	{
		public ITCPNpgsqlProvider ()
		{
		}

		public ProjectInfo GetProjectInfo(int projectid)
		{
			throw new NotImplementedException ();
		}

		public ProjectInfo[] SearchProject(ProjectInfo pi)
		{
			throw new NotImplementedException ();
		}

		public int NewTask (int projectId, string name, string desc)
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


		public int NewProject (string name, string desc, string ownerId)
		{
			int id = 0;
			using (var cnx = CreateConnection()) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand()) {
					cmd.CommandText = "insert into projets (name,managerid,ApplicatonName,prdesc) values (@name,@mid,@appname,@pdesc)";
					cmd.Parameters.Add ("@name", name);
					cmd.Parameters.Add ("@mid", ownerId);
					cmd.Parameters.Add ("@appname", ApplicationName);
					cmd.Parameters.Add ("@desc", desc);
					id = (int)cmd.ExecuteScalar ();
				}
				cnx.Close ();
			}
			return id;
		}

	}
}

