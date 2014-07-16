using System;

namespace SalesCatalog.Model
{
	public class CheckBox : FormInput
	{
		public CheckBox ()
		{
		}
		public bool Value { get; set; }

		public override string ToHtml ()
		{
			return string.Format ("<input type=\"checkbox\" id=\"{0}\" name=\"{1}\" {2}/>", Id,Name,Value?"checked":"");
		}
	}
}

