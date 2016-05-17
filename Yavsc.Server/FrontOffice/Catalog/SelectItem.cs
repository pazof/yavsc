using System;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Select item.
	/// </summary>
	public class SelectItem
	{ 
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.SelectItem"/> class.
		/// </summary>
		/// <param name="t">T.</param>
		public SelectItem(string t)
		{
			Value = t;
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Value { get; set; }

		/// <param name="t">a SelectItem.</param>
		public static implicit operator string(SelectItem t)
		{
			return t.Value;
		}

		/// <param name="t">a string.</param>
		public static implicit operator SelectItem(string t)
		{
			return new SelectItem(t);
		}

	}
}

