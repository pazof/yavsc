using System;
using System.Text;
using System.Web.Mvc;

namespace SalesCatalog.Model
{
	public class SelectInput: FormInput
	{
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
