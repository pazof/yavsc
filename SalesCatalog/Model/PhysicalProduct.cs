using System;

namespace SalesCatalog.Model
{
	public class PhysicalProduct : Product
	{
		public PhysicalProduct ()
		{
		}
		public Price UnitaryPrice { get; set; }		
		#region implemented abstract members of SalesCatalog.Model.Product
		public override string[] GetSalesConditions ()
		{
			return new string [] { string.Format(
				"Prix unitaire : {0} {1}",
			     UnitaryPrice.Quantity.ToString(), 
					UnitaryPrice.Unit.Name) };
		}
		#endregion

		public override string ToString ()
		{
			return string.Format ("[PhysicalProduct: UnitaryPrice={0}]", UnitaryPrice);
		}
	}
}

