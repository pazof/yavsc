using System;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Form element.
	/// </summary>
	public abstract class FormElement
	{
		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public abstract string ToHtml ();
	}
}

