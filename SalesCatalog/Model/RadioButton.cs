using System;

namespace SalesCatalog.Model
{
	public class RadioButton:FormInput
	{
		#region implemented abstract members of FormInput

		public override string Type {
			get {
				return "radio";
			}
		}

		#endregion

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

