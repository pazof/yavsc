using System;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Text.
	/// </summary>
	public class Text: FormElement
	{
		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Val {
			get;
			set;
		}

		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{
			return Val;
		}
	}
}

