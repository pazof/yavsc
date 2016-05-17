using System;
using System.ComponentModel.DataAnnotations;
using Yavsc.Model.Calendar;

namespace Yavsc.Model.FrontOffice.Catalog
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
		/// <summary>
		/// Gets or sets the images.
		/// </summary>
		/// <value>The images.</value>
		public ProductImage[] Images { get; set; }
		/// <summary>
		/// Gets or sets the command form.
		/// </summary>
		/// <value>The command form.</value>
		public SaleForm CommandForm { get; set; }

		/// <summary>
		/// Gets or sets the reference.
		/// </summary>
		/// <value>The reference.</value>
		[Required]
		[StringLength(255)]
		public string Reference { get; set; }
		/// <summary>
		/// Gets or sets the command validity dates.
		/// </summary>
		/// <value>The command validity dates.</value>
		public Period CommandValidityDates { get; set; }
		/// <summary>
		/// Gets the sales conditions.
		/// </summary>
		/// <returns>The sales conditions.</returns>
		public abstract string[] GetSalesConditions();
		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public virtual string Type { get { return GetType().Name; }
		}
	}
}

