using System;
using System.Collections.Generic;

namespace yavscModel.WorkFlow
{
	public interface IWFOrder
	{
		/// <summary>
		/// Gets the unique Identifier for this order, in this application.
		/// </summary>
		/// <value>The unique I.</value>
		string UniqueID {
			get;
		}
		event EventHandler<OrderStatusChangedEventArgs> StatusChanged;
	}
}

