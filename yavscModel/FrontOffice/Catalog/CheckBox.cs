using System;

namespace Yavsc.Model.FrontOffice
{
	public class CheckBox : FormInput
	{
		public CheckBox ()
		{
		}

		#region implemented abstract members of FormInput


		public override string Type {
			get {
				return "checkbox";
			}
		}

		public bool Value { get; set; }

		public override string ToHtml ()
		{
			return string.Format ("<input type=\"checkbox\" id=\"{0}\" name=\"{1}\" {2}/>", Id,Name,Value?"checked":"");
		}
		#endregion
	}
}

