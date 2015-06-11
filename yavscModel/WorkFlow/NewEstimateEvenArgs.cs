using System;
using Yavsc.Model.WorkFlow;
using System.Configuration;
using System.Collections.Specialized;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// New estimate even arguments.
	/// </summary>
	public class NewEstimateEvenArgs: EventArgs
	{
		private Estimate data=null;

		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <value>The data.</value>
		public Estimate Data{ get { return data; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.WorkFlow.NewEstimateEvenArgs"/> class.
		/// </summary>
		/// <param name="created">Created.</param>
		public NewEstimateEvenArgs(Estimate created)
		{
			data = created;
		}

	}

}

