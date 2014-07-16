using System;
using System.ComponentModel.DataAnnotations;

namespace SalesCatalog.Model
{
	/// <summary>
	/// Product.
	/// Crucial object in the catalog,
	/// being at each origin of form display
	/// its properties may be used to fill some form input values or other form element.
	/// <c>in text values, within {} ex: {Name} : {Price} ({stockStatus}) ($description) </c>.
	/// </summary>
	public abstract class Product
	{
		/// <summary>
		/// Gets or sets the product name.
		/// </summary>
		/// <value>The name.</value>
		[Required]
		[StringLength(1024)]
		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the product description.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }
		public ProductImage[] Images { get; set; }
		public SaleForm CommandForm { get; set; }
		[Required]
		[StringLength(255)]
		public string Reference { get; set; }
		public Period CommandValidityDates { get; set; }
		public abstract string[] GetSalesConditions();

	}
}

