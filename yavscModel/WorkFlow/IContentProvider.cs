using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace yavscModel.WorkFlow
{
	public interface IContentProvider: IDisposable
	{
		IWFOrder CreateOrder ();
		IWFOrder ImapctOrder (string orderid, FormCollection col);
		IContent GetBlob (string orderId);
		int GetStatus (string orderId);
		/// <summary>
		/// Gets the status labels.
		/// 0 is the starting status 
		/// </summary>
		/// <value>The status labels.</value>
		bool [] IsFinalStatus { get; }
		string [] StatusLabels {get;}
	}
}

