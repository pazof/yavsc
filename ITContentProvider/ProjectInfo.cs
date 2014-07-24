using System;
using WorkFlowProvider;

namespace ITContentProvider
{
	public class ProjectInfo
	{
		string Name { get; set; }
		string Licence { get; set; }
		string BBDescription { get; set; }
		DateTime StartDate { get; set; }
		string ProdVersion { get; set; }
		string StableVersion { get; set; }
		string TestingVersion { get; set; }
		string WebSite { get; set; }
	}
}

