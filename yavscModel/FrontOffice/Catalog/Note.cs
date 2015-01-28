using System;

namespace Yavsc.Model.FrontOffice
{
	public class Note:Text
	{
		public override string ToHtml ()
		{
			return string.Format("<quote>{0}</quote>",Val);
		}
	}
}

