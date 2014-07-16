using System;

namespace SalesCatalog.Model
{
	public class Text: FormElement
	{
		public string Val {
			get;
			set;
		}

		public override string ToHtml ()
		{
			return Val;
		}
	}
}

