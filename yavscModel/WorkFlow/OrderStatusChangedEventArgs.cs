using System;

namespace Yavsc.Model.WorkFlow
{
	public class OrderStatusChangedEventArgs: EventArgs
	{
		public OrderStatusChangedEventArgs (int oldstatus, int newstatus, string reason)
		{
			oldstat = oldstatus;
			newstat = newstatus;
		}
		private int oldstat, newstat;
		public int OldStatus { get { return oldstat; } }
		public int NewStatus { get { return newstat; } }
	}
}

