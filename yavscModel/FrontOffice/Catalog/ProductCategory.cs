using System;

namespace Yavsc.Model.FrontOffice
{
	public class ProductCategory
	{
		public ProductCategory ()
		{
		}
		public string Name { get; set; }
		public string Reference { get; set; }
		public Product[] Products { get; set; }
		public Product GetProductByName (string productName)
		{
			return Array.Find<Product> (Products, p => p.Name == productName);
		}
		public Product GetProduct (string reference)
		{
			return Array.Find<Product> (Products, p => p.Reference == reference);
		}
	}
}

