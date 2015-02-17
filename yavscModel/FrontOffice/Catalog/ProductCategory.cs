using System;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Product category.
	/// </summary>
	public class ProductCategory
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.ProductCategory"/> class.
		/// </summary>
		public ProductCategory ()
		{
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }


		/// <summary>
		/// Gets or sets the reference.
		/// </summary>
		/// <value>The reference.</value>
		public string Reference { get; set; }

		/// <summary>
		/// Gets or sets the products.
		/// </summary>
		/// <value>The products.</value>
		public Product[] Products { get; set; }

		/// <summary>
		/// Gets the name of the product by.
		/// </summary>
		/// <returns>The product by name.</returns>
		/// <param name="productName">Product name.</param>
		public Product GetProductByName (string productName)
		{
			return Array.Find<Product> (Products, p => p.Name == productName);
		}

		/// <summary>
		/// Gets the product.
		/// </summary>
		/// <returns>The product.</returns>
		/// <param name="reference">Reference.</param>
		public Product GetProduct (string reference)
		{
			return Array.Find<Product> (Products, p => p.Reference == reference);
		}
	}
}

