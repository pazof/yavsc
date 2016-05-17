using System;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// Order status changed event arguments.
	/// </summary>
	public class OrderStatusChangedEventArgs: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.WorkFlow.OrderStatusChangedEventArgs"/> class.
		/// </summary>
		/// <param name="oldstatus">Oldstatus.</param>
		/// <param name="newstatus">Newstatus.</param>
		/// <param name="reason">Reason.</param>
		public OrderStatusChangedEventArgs (int oldstatus, int newstatus, string reason)
		{
			oldstat = oldstatus;
			newstat = newstatus;
		}
		private int oldstat, newstat;
		/// <summary>
		/// Gets the old status.
		/// </summary>
		/// <value>The old status.</value>
		public int OldStatus { get { return oldstat; } }
		/// <summary>
		/// Gets the new status.
		/// </summary>
		/// <value>The new status.</value>
		public int NewStatus { get { return newstat; } }
	}
}

