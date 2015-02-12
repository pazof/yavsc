using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Brand.
	/// </summary>
	public class Brand
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Brand"/> class.
		/// </summary>
		public Brand ()
		{
		}
		
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[Required]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the slogan.
		/// </summary>
		/// <value>The slogan.</value>
		public string Slogan { get; set; }

		/// <summary>
		/// Gets or sets the logo.
		/// </summary>
		/// <value>The logo.</value>
		public ProductImage Logo { get; set; }

		/// <summary>
		/// Gets or sets the categories.
		/// </summary>
		/// <value>The categories.</value>
		public ProductCategory[] Categories { get; set; }
		/// <summary>
		/// Gets or sets the default form.
		/// </summary>
		/// <value>The default form.</value>
		public SaleForm DefaultForm { get; set; }

		/// <summary>
		/// Gets the product category.
		/// </summary>
		/// <returns>The product category.</returns>
		/// <param name="reference">Reference.</param>
		public ProductCategory GetProductCategory(string reference)
		{
			return Array.Find<ProductCategory>(Categories, c => c.Reference == reference);
		}

		/// <summary>
		/// Gets the name of the product category by.
		/// </summary>
		/// <returns>The product category by name.</returns>
		/// <param name="catName">Cat name.</param>
		public ProductCategory GetProductCategoryByName(string catName)
		{
			return Array.Find<ProductCategory>(Categories, c => c.Name == catName);
		}
	}
}

