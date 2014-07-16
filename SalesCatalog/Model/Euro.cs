using System;

namespace SalesCatalog.Model
{
	public class Euro : Currency
	{
		public Euro ()
		{
		}

		public override string Name {
			get {
				return "Euro";
			}
		}

		public override string Description {
			get {
				return "European currency";
			}
		}

		public override bool MayConvertTo (Unit other)
		{
			return other.GetType().IsSubclassOf(typeof (Currency));
		}

		public override object ConvertTo (Unit dest, object value)
		{
			throw new NotImplementedException();
		}
	}
}

