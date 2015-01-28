using System;

namespace Yavsc.Model.FrontOffice
{
	public class ProductImage: FormElement
	{
		#region implemented abstract members of FormElement

		public override string ToHtml ()
		{
			return string.Format ("<img src=\"\" alt=\"\"/>", Src, Alt);
		}

		#endregion

		public ProductImage ()
		{
		}
		public string Src { get; set; }
		public string Alt { get; set; }
	}
}

