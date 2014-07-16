using System;

namespace SalesCatalog.Model
{
	public class Option : FormElement
	{
		public Option ()
		{
		}
		public string Value { get; set; }
		public string Text { get; set; }

		public override string ToHtml ()
		{
			return string.Format ("<option value=\"{0}\">{1}</option>\n",Value,Text);
		}
	}
}

