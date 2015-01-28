using System;
using System.Text;
using System.Web.Mvc;

namespace Yavsc.Model.FrontOffice
{
	public class SelectInput: FormInput
	{
		#region implemented abstract members of FormInput

		public override string Type {
			get {
				return "select";
			}
		}

		#endregion

		public Option[] Items;
		public int SelectedIndex;
		public override string ToHtml ()
		{
			StringBuilder sb = new StringBuilder ();
			foreach (Option opt in Items)
				sb.Append (opt.ToHtml());
			return string.Format ("<select id=\"{0}\" name=\"{1}\">{2}</select>\n", Id,Name,sb.ToString());
		}
	}
}

