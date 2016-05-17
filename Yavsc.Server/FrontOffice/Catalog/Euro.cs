using System;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Euro.
	/// </summary>
	public class Euro : Currency
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Euro"/> class.
		/// </summary>
		public Euro ()
		{
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public override string Name {
			get {
				return "Euro";
			}
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>The description.</value>
		public override string Description {
			get {
				return "European currency";
			}
		}

		/// <summary>
		/// Maies the convert to.
		/// </summary>
		/// <returns><c>true</c>, if convert to was mayed, <c>false</c> otherwise.</returns>
		/// <param name="other">Other.</param>
		public override bool MayConvertTo (Unit other)
		{
			return other.GetType().IsSubclassOf(typeof (Currency));
		}

		/// <summary>
		/// Converts to.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="dest">Destination.</param>
		/// <param name="value">Value.</param>
		public override object ConvertTo (Unit dest, object value)
		{
			throw new NotImplementedException();
		}
	}
}

