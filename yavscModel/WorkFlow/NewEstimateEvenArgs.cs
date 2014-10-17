using System;
using Yavsc.Model.WorkFlow;
using System.Configuration;
using Yavsc.Model.WorkFlow.Configuration;
using System.Collections.Specialized;

namespace Yavsc.Model.WorkFlow
{
	public class NewEstimateEvenArgs: EventArgs
	{
		private string clientName;
		private string estimateTitle;
		private long eid;

		public string ClientName{ get { return clientName; } }
		public string EstimateTitle { get { return estimateTitle; } }
		public long EstimateId { get { return eid; } }

		public NewEstimateEvenArgs(long estid, string client, string title)
		{
			clientName = client;
			estimateTitle = title;
			eid = estid;
		}

	}

}

