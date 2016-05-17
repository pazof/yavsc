using System;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Files input.
	/// </summary>
	public class FilesInput : FormInput
	{
		#region implemented abstract members of FormInput
		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public override string Type {
			get {
				return "file";
			}
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.FilesInput"/> class.
		/// </summary>
		public FilesInput ()
		{
		}

		/// <summary>
		/// Tos the html.
		/// </summary>
		/// <returns>The html.</returns>
		public override string ToHtml ()
		{
			return string.Format ("<input type=\"file\" id=\"{0}\" name=\"{0}\"/>", Id);
		}
	}
}

