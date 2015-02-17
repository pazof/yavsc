using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Product image.
	/// </summary>
	public class ProductImage: FormElement
	{
		#region implemented abstract members of FormElement
		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{
			return string.Format ("<img src=\"\" alt=\"\"/>", Src, Alt);
		}

		#endregion
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.ProductImage"/> class.
		/// </summary>
		public ProductImage ()
		{
		}

		/// <summary>
		/// Gets or sets the source.
		/// </summary>
		/// <value>The source.</value>
		public string Src { get; set; }

		/// <summary>
		/// Gets or sets the alternate.
		/// </summary>
		/// <value>The alternate.</value>
		public string Alt { get; set; }
	}
}

