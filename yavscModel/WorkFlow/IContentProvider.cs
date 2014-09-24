using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace yavscModel.WorkFlow
{
	public interface IContentProvider : IProvider, IDisposable
	{
		void DropWritting (long wrid);
		void DropEstimate (long estid);
		void TagWritting (long wrid,string tag);
		int GetStatus (string estimId);
		/// <summary>
		/// Gets the status labels.
		/// 0 is the starting status 
		/// </summary>
		/// <value>The status labels.</value>
		string [] StatusLabels {get;}
		bool [] FinalStatuses { get; }
		long CreateEstimate (string client, string title);
		void SetTitle (long estid,  string newTitle);
		long Write (long estid, string desc, decimal ucost, int count, long productid);
		void SetDesc (long writid, string newDesc);
		Estimate GetEstimate (long estimid);
	}
}

