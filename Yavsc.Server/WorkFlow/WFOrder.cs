using System;
using SalesCatalog.Model;

namespace Yavsc.Model.WorkFlow
{
	public class WFOrder : IWFOrder
	{
		public string GetStatus ()
		{
			// TODO Manager.GetStatus(this.id);
			throw new NotImplementedException ();
		}

		private long prodid;

		public long ProductId {
			get {
				return prodid;
			}
		}

		private long id = 0;
		public long UniqueID {
			get {
				return id;
			}
		}

		private DateTime date;
		public DateTime OrderDate {
			get {
				return date;
			}
		}

		private string catref;

		/// <summary>
		/// Gets the catalog reference, a unique id for the catalog (not a product id).
		/// </summary>
		/// <value>The catalog reference.</value>
		public string CatalogReference {
			get {
				return catref;
			}
		}

		public static WFOrder CreateOrder(long productId,string catalogReference){
			return new WFOrder() {date = DateTime.Now,
			catref=catalogReference,
				prodid = productId};
			//TODO id = Manager.Order(...)
		}  

		public override string ToString ()
		{
			return string.Format ("[Commande date={0} prodref={1}, cat={2}]",date,prodid,catref);
		}



	}
}

