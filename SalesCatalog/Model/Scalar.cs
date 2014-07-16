using System;

namespace SalesCatalog.Model
{
	public abstract class Scalar
	{
		public Scalar ()
		{
		}
		public abstract object Quantity { get; set; }
		public abstract Unit Unit{ get; set; }
	}
}

