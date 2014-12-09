using System;
using System.Collections.Generic;

namespace Yavsc.Model.WorkFlow
{
	public interface IWFOrder
	{
		/// <summary>
		/// Gets the unique Identifier for this order, in this application.
		/// </summary>
		/// <value>The unique I.</value>
		long UniqueID {
			get;
		}
		/// <summary>
		/// Gets the actual status for this order.
		/// </summary>
		/// <returns>The status.</returns>
		string GetStatus();
	}
}

