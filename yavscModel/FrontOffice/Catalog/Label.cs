using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Label.
	/// </summary>
	public class Label:FormElement
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Label"/> class.
		/// </summary>
		public Label ()
		{
		}
		string Text { get; set; }
		string For { get; set ; }
		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{
			return string.Format ("<label for=\"{0}\">{1}</label>", For, Text);
		}
	}
}

