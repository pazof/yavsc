using System;
using System.Text;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Select input.
	/// </summary>
	public class SelectInput: FormInput
	{
		#region implemented abstract members of FormInput
		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public override string Type {
			get {
				return "select";
			}
		}

		#endregion
		/// <summary>
		/// The items.
		/// </summary>
		public Option[] Items;

		/// <summary>
		/// The index of the selected.
		/// </summary>
		public int SelectedIndex;

		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{
			StringBuilder sb = new StringBuilder ();
			foreach (Option opt in Items)
				sb.Append (opt.ToHtml());
			return string.Format ("<select id=\"{0}\" name=\"{1}\">{2}</select>\n", Id,Name,sb.ToString());
		}
	}
}

