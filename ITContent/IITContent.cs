using System;
using WorkFlowProvider;
using yavscModel.WorkFlow;

namespace ITContent
{
	public interface IITContent: IContentProvider
	{
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
	}
}

