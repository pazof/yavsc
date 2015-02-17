using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Price.
	/// </summary>
	public class Price: Scalar
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Price"/> class.
		/// </summary>
		public Price ()
		{
		}

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

