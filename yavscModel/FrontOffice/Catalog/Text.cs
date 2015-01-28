using System;

namespace Yavsc.Model.FrontOffice
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

