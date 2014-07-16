using System;

namespace yavscModel.WorkFlow
{
	public class Estimate
	{
		public Estimate ()
		{
		}
		public string Description { get; set; }
		public decimal Ciffer {
			get {
				decimal total = 0;
				foreach (Writting l in Lines)
					total += l.UnitaryCost * l.Count;
				return total;
			}
		}
		public Writting[] Lines { get; set; }
	}
}

