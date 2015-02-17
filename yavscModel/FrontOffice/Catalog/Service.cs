using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Service.
	/// </summary>
	public class Service : Product
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Service"/> class.
		/// </summary>
		public Service ()
		{
		}

		/// <summary>
		/// Gets or sets the hour price.
		/// </summary>
		/// <value>The hour price.</value>
		public Price HourPrice { get; set; }

		#region implemented abstract members of Product
		/// <summary>
		/// Gets the sales conditions.
		/// </summary>
		/// <returns>The sales conditions.</returns>
		public override string [] GetSalesConditions ()
		{
			return new string [] { string.Format(
				"Prix horaire de la prestation : {0} {1}",
			     HourPrice.Quantity.ToString(), 
					HourPrice.Unit.Name) } ;
		}
		#endregion

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.FrontOffice.Service"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Yavsc.Model.FrontOffice.Service"/>.</returns>
		public override string ToString ()
		{
			return string.Format ("[Service: HourPrice={0}]", HourPrice);
		}
	}
}

