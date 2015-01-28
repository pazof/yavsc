using System;

namespace Yavsc.Model.FrontOffice
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
		public override Unit Unit {
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

