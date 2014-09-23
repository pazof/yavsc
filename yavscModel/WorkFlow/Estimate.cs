using System;

namespace yavscModel.WorkFlow
{
	public class Estimate
	{
		public Estimate ()
		{
		}
		public string Title { get; set; }
		public string Owner { get; set; }

		public decimal Ciffer {
			get {
				decimal total = 0;
				if (Lines == null)
					return total;
				foreach (Writting l in Lines)
					total += l.UnitaryCost * l.Count;
				return total;
			}
		}
		public Writting[] Lines { get; set; }
	}
}

