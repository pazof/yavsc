using System;

namespace SalesCatalog.Model
{
	public class Price: Scalar
	{
		public Price ()
		{
		}

		decimal quantity;

		#region implemented abstract members of SalesCatalog.Value
		public override object Quantity {
			get {
				return quantity;
			}
			set {
				quantity = (decimal) value;
			}
		}

		Currency curr;
		public override SalesCatalog.Model.Unit Unit {
			get {
				return curr;
			}
			set {
				curr = (Currency)value;
			}
		}
		#endregion

	}
}

