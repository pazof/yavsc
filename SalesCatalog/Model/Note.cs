using System;

namespace SalesCatalog.Model
{
	public class Note:Text
	{
		public override string ToHtml ()
		{
			return string.Format("<quote>{0}</quote>",Val);
		}
	}
}

