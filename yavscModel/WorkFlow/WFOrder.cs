using System;
using SalesCatalog.Model;

namespace Yavsc.Model.WorkFlow
{
	public class WFOrder : IWFOrder
	{
		private Product p;
		private DateTime date;
		private string catref;
		private string id = null;
		public WFOrder(Product prod,string catalogReference){
			date = DateTime.Now;
			catref=catalogReference;
			p = prod;
			id = Guid.NewGuid ().ToString();
		}  
		public override string ToString ()
		{
			return string.Format ("[Commande date={0} prodref={1}, cat={2}]",date,p.Reference,catref);
		}

		public event EventHandler<OrderStatusChangedEventArgs> StatusChanged;

		#region IWFCommand implementation
		/// <summary>
		/// Gets the catalog reference, a unique id for the catalog (not a product id).
		/// </summary>
		/// <value>The catalog reference.</value>
		public string UniqueID {
			get {
				return id;
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

