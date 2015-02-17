using System;

namespace Yavsc.Model.FrontOffice
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

