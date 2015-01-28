using System;

namespace Yavsc.Model.FrontOffice
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

