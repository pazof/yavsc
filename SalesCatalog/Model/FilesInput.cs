using System;

namespace SalesCatalog.Model
{
	public class FilesInput : FormInput
	{

		public FilesInput ()
		{
		}

		public override string ToHtml ()
		{
			return string.Format ("<input type=\"file\" id=\"{0}\" name=\"{0}\"/>", Id);
		}
	}
}

