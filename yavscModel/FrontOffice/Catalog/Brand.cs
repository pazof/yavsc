using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.FrontOffice
{
	public class Brand
	{
		public Brand ()
		{
		}

		[Required]
		public string Name { get; set; }

		public string Slogan { get; set; }

		public ProductImage Logo { get; set; }

		public ProductCategory[] Categories { get; set; }
		/// <summary>
		/// Gets or sets the default form.
		/// </summary>
		/// <value>The default form.</value>
		public SaleForm DefaultForm { get; set; }

		public ProductCategory GetProductCategory(string reference)
		{
			return Array.Find<ProductCategory>(Categories, c => c.Reference == reference);
		}
		public ProductCategory GetProductCategoryByName(string catName)
		{
			return Array.Find<ProductCategory>(Categories, c => c.Name == catName);
		}
	}
}

