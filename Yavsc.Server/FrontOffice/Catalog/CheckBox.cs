using System;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Check box.
	/// </summary>
	public class CheckBox : FormInput
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.CheckBox"/> class.
		/// </summary>
		public CheckBox ()
		{
		}

		#region implemented abstract members of FormInput

		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public override string Type {
			get {
				return "checkbox";
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.FrontOffice.CheckBox"/> is value.
		/// </summary>
		/// <value><c>true</c> if value; otherwise, <c>false</c>.</value>
		public bool Value { get; set; }

		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{
			return string.Format ("<input type=\"checkbox\" id=\"{0}\" name=\"{1}\" {2}/>", Id,Name,Value?"checked":"");
		}
		#endregion
	}
}

