using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.FrontOffice.Catalog
{
	/// <summary>
	/// Link.
	/// </summary>
	public class Link:Label
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Link"/> class.
		/// </summary>
		public Link ()
		{
		}

		/// <summary>
		/// Gets or sets the reference.
		/// </summary>
		/// <value>The reference.</value>
		[Required]
		public string Ref { get; set; }
	}
}

