using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Scalar.
	/// </summary>
	public abstract class Scalar
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Scalar"/> class.
		/// </summary>
		public Scalar ()
		{
		}
		/// <summary>
		/// Gets or sets the quantity.
		/// </summary>
		/// <value>The quantity.</value>
		public abstract object Quantity { get; set; }
		/// <summary>
		/// Gets or sets the unit.
		/// </summary>
		/// <value>The unit.</value>
		public abstract Unit Unit{ get; set; }
	}
}

