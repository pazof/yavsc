using System;
using System.Collections.Generic;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Sale form.
	/// </summary>
	public class SaleForm
	{
		/// <summary>
		/// Gets or sets the catalog reference.
		/// It must be non null,
		/// it is an Uri, returning the Xml
		/// Catalog at Http GET request
		/// </summary>
		/// <value>The catalog reference.</value>
		public string CatalogReference { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.SaleForm"/> class.
		/// </summary>
		public SaleForm ()
		{
		}

		/// <summary>
		/// Gets or sets the action, that is,
		/// the Method of the FrontOffice controller
		/// called to post this Command form.
		/// It defaults to "Command"
		/// </summary>
		/// <value>The action.</value>
		public string Action {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the input values of this command form.
		/// </summary>
		/// <value>The items.</value>
		public FormElement[] Items { get; set; }
	}
}

