using System;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Option.
	/// </summary>
	public class Option : FormElement
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Option"/> class.
		/// </summary>
		public Option ()
		{
		}
		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Value { get; set; }
		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>The text.</value>
		public string Text { get; set; }
		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{
			return string.Format ("<option value=\"{0}\">{1}</option>\n",Value,Text);
		}
	}
}

