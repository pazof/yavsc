using System;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Radio button.
	/// </summary>
	public class RadioButton:FormInput
	{
		#region implemented abstract members of FormInput
		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public override string Type {
			get {
				return "radio";
			}
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.RadioButton"/> class.
		/// </summary>
		public RadioButton ()
		{
		}

		/// <summary>
		/// Gets or sets the choice.
		/// </summary>
		/// <value>The choice.</value>
		public string Choice { get; set; }

		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{
			return string.Format ("<input type=\"radio\" id=\"{0}\" name=\"{1}\" value=\"{2}\"/><label for=\"{0}\">{2}</label>", Id,Name,Choice);
		}
	}
}

