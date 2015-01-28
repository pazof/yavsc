using System;

namespace Yavsc.Model.FrontOffice
{
	public class Service : Product
	{
		public Service ()
		{
		}

		public Price HourPrice { get; set; }

		#region implemented abstract members of SalesCatalog.Model.Product
		public override string [] GetSalesConditions ()
		{
			return new string [] { string.Format(
				"Prix horaire de la prestation : {0} {1}",
			     HourPrice.Quantity.ToString(), 
					HourPrice.Unit.Name) } ;
		}
		#endregion
		public override string ToString ()
		{
			return string.Format ("[Service: HourPrice={0}]", HourPrice);
		}
	}
}

