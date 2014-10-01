using System;

namespace SalesCatalog.Model
{
	public class FilesInput : FormInput
	{
		#region implemented abstract members of FormInput
		public override string Type {
			get {
				return "file";
			}
		}
		#endregion

		public FilesInput ()
		{
		}

		public override string ToHtml ()
		{
			return string.Format ("<input type=\"file\" id=\"{0}\" name=\"{0}\"/>", Id);
		}
	}
}

