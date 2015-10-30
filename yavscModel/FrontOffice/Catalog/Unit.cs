using System;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Unit.
	/// </summary>
	public abstract class Unit
	{
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public abstract string Name { get; }
		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>The description.</value>
		public abstract string Description { get; }
		/// <summary>
		/// Converts to.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="dest">Destination.</param>
		/// <param name="value">Value.</param>
		public abstract object ConvertTo (Unit dest, object value);
		/// <summary>
		/// Maies the convert to.
		/// </summary>
		/// <returns><c>true</c>, if convert to was mayed, <c>false</c> otherwise.</returns>
		/// <param name="other">Other.</param>
		public abstract bool MayConvertTo (Unit other);
	}
}

