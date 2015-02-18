using System;
using WorkFlowProvider;
using Npgsql;

namespace ITContentProvider
{
	/// <summary>
	/// ITCP npgsql provider.
	/// </summary>
	public class ITCPNpgsqlProvider : NpgsqlContentProvider
	{
		/*	TODO
		
		int NewProject(string name, string desc, string ownedId);
		void AddDevRessource (int prjId, string userName);
		int NewTask(int projectId, string name, string desc);
		void SetProjectName(int projectId, string name);
		void SetProjectDesc(int projectId, string desc);
		void SetTaskName(int taskId, string name);
		void SetStartDate(int taskId, DateTime d);
		void SetEndDate(int taskId, DateTime d);
		void SetTaskDesc(int taskId, string desc);
		void NewRelease(int projectId, string Version);
		*/

		/// <summary>
		/// Initializes a new instance of the <see cref="ITContentProvider.ITCPNpgsqlProvider"/> class.
		/// </summary>
		public ITCPNpgsqlProvider ()
		{
		}
		/// <summary>
		/// Gets the project info.
		/// </summary>
		/// <returns>The project info.</returns>
		/// <param name="projectid">Projectid.</param>
		public ProjectInfo GetProjectInfo(int projectid)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Searchs the project.
		/// </summary>
		/// <returns>The project.</returns>
		/// <param name="pi">Pi.</param>
		public ProjectInfo[] SearchProject(ProjectInfo pi)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// News the task.
		/// </summary>
		/// <returns>The task.</returns>
		/// <param name="projectId">Project identifier.</param>
		/// <param name="name">Name.</param>
		/// <param name="desc">Desc.</param>
		public int NewTask (int projectId, string name, string desc)
		{
			throw new System.NotImplementedException ();
		}

		/// <summary>
		/// Sets the name of the task.
		/// </summary>
		/// <param name="taskId">Task identifier.</param>
		/// <param name="name">Name.</param>
		public void SetTaskName (int taskId, string name)
		{
			throw new System.NotImplementedException ();
		}

		/// <summary>
		/// Sets the start date.
		/// </summary>
		/// <param name="taskId">Task identifier.</param>
		/// <param name="d">D.</param>
		public void SetStartDate (int taskId, DateTime d)
		{
			throw new System.NotImplementedException ();
		}

		/// <summary>
		/// Sets the end date.
		/// </summary>
		/// <param name="taskId">Task identifier.</param>
		/// <param name="d">D.</param>
		public void SetEndDate (int taskId, DateTime d)
		{
			throw new System.NotImplementedException ();
		}

		/// <summary>
		/// Removes the project.
		/// </summary>
		/// <param name="prjId">Prj identifier.</param>
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

		/// <summary>
		/// News the project.
		/// </summary>
		/// <returns>The project.</returns>
		/// <param name="name">Name.</param>
		/// <param name="desc">Desc.</param>
		/// <param name="ownerId">Owner identifier.</param>
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

