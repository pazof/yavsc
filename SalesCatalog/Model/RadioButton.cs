using System;

namespace SalesCatalog.Model
{
	public class RadioButton:FormInput
	{
		public RadioButton ()
		{
		}
		public string Choice { get; set; }
		public override string ToHtml ()
		{
			return string.Format ("<input type=\"radio\" id=\"{0}\" name=\"{1}\" value=\"{2}\"/><label for=\"{0}\">{2}</label>", Id,Name,Choice);
		}
	}
}

