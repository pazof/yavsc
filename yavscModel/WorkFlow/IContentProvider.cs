using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace yavscModel.WorkFlow
{

	public interface IContentProvider : IModule, IDisposable
	{

		/// <summary>
		/// Gets the status labels.
		/// 0 is the starting status 
		/// </summary>
		/// <value>The status labels.</value>
		string [] StatusLabels {get;}
		bool [] FinalStatuses { get; }
		StatusChange[] GetWrittingStatuses (long wrid);
		StatusChange[] GetEstimateStatuses (long estid);
		long CreateEstimate (string client, string title);
		Estimate [] GetEstimates(string client);
		Estimate GetEstimate (long estimid);
		long Write (long estid, string desc, decimal ucost, int count, long productid);
		void DropWritting (long wrid);
		void DropEstimate (long estid);
		void TagWritting (long wrid,string tag);
		void DropTagWritting (long wrid,string tag);
		void UpdateWritting (Writting wr);
		void SetTitle (long estid,  string newTitle);
		void SetDesc (long writid, string newDesc);
		void SetWrittingStatus (long wrtid,int status,string username);
		void SetEstimateStatus (long estid,int status,string username);
	}
}

