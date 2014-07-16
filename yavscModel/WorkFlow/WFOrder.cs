using System;
using SalesCatalog.Model;
using yavscModel.WorkFlow;

namespace WorkFlow
{
	public class WFOrder : IWFCommand
	{
		private Product p;
		private DateTime date;
		private string catref;
		public WFOrder(Product prod,string catalogReference){
			date = DateTime.Now;
			catref=catalogReference;
			p = prod;
		}  
		public override string ToString ()
		{
			return string.Format ("[Commande date={0} prodref={1}, cat={2}]",date,p.Reference,catref);
		}

		#region IWFCommand implementation

		public string CatalogReference {
			get {
				return catref;
			}
		}

		public DateTime OrderDate {
			get {
				return date;
			}
		}

		#endregion
	}
}

