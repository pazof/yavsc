using System;

namespace Yavsc.Model.FrontOffice.Catalog.Billing
{
	/// <summary>
	/// Price.
	/// </summary>
	public class Price: Scalar
	{
		decimal quantity;

		#region implemented abstract members of SalesCatalog.Value
		/// <summary>
		/// Gets or sets the quantity.
		/// </summary>
		/// <value>The quantity.</value>
		public override object Quantity {
			get {
				return quantity;
			}
			set {
				quantity = (decimal) value;
			}
		}

		Currency curr;
		/// <summary>
		/// Gets or sets the unit.
		/// </summary>
		/// <value>The unit.</value>
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

