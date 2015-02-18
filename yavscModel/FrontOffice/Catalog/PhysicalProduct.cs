using System;
using Yavsc.Model.FrontOffice.Billing;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Physical product.
	/// </summary>
	public class PhysicalProduct : Product
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.PhysicalProduct"/> class.
		/// </summary>
		public PhysicalProduct ()
		{
		}
		/// <summary>
		/// Gets or sets the unitary price.
		/// </summary>
		/// <value>The unitary price.</value>
		public Price UnitaryPrice { get; set; }		
		#region implemented abstract members of Product
		/// <summary>
		/// Gets the sales conditions.
		/// </summary>
		/// <returns>The sales conditions.</returns>
		public override string[] GetSalesConditions ()
		{
			return new string [] { string.Format(
				"Prix unitaire : {0} {1}",
			     UnitaryPrice.Quantity.ToString(), 
					UnitaryPrice.Unit.Name) };
		}
		#endregion
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.FrontOffice.PhysicalProduct"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.FrontOffice.PhysicalProduct"/>.</returns>
		public override string ToString ()
		{
			return string.Format ("[PhysicalProduct: Reference:{0} UnitaryPrice={1}]", Reference, UnitaryPrice);
		}
	}
}

