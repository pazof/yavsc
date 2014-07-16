using System;

namespace SalesCatalog.Model
{
	public class Label:FormElement
	{
		public Label ()
		{
		}
		string Text { get; set; }
		string For { get; set ; }
		public override string ToHtml ()
		{
			return string.Format ("<label for=\"{0}\">{1}</label>", For, Text);
		}
	}
}

