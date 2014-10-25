using System;
using Yavsc.Model.WorkFlow;
using System.Configuration;
using Yavsc.Model.WorkFlow.Configuration;
using System.Collections.Specialized;

namespace Yavsc.Model.WorkFlow
{
	public class NewEstimateEvenArgs: EventArgs
	{
		private Estimate data=null;
		public Estimate Data{ get { return data; } }
		public NewEstimateEvenArgs(Estimate created)
		{
			data = created;
		}

	}

}

