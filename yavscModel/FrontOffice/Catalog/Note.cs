using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Note.
	/// </summary>
	public class Note:Text
	{
		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{
			return string.Format("<quote>{0}</quote>",Val);
		}
	}
}

