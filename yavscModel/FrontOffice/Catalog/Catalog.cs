using System;
using System.Collections.Generic;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Catalog.
	/// </summary>
	public class Catalog {

		/// <summary>
		/// Gets or sets the catalog unique identifier in the system.
		/// </summary>
		/// <value>The unique identifier.</value>
		string UID { get; set; }

		/// <summary>
		/// Gets or sets the brands.
		/// </summary>
		/// <value>The brands.</value>
		public Brand[] Brands { get; set; }
		/// <summary>
		/// Gets the brand.
		/// </summary>
		/// <returns>The brand.</returns>
		/// <param name="brandName">Brand name.</param>
		public Brand GetBrand(string brandName)
		{
			return Array.Find<Brand>(Brands, b => b.Name == brandName);
		}

		/// <summary>
		/// Adds the brand.
		/// </summary>
		/// <returns>The brand.</returns>
		/// <param name="brandName">Brand name.</param>
		/// <param name="slogan">Slogan.</param>
		/// <param name="logo">Logo.</param>
		public Brand AddBrand(string brandName,string slogan=null, ProductImage logo=null)
		{
			Brand[] oldbrs = (Brand[]) Brands.Clone ();
			int oldl = Brands.Length;
			Array.Resize<Brand>(ref oldbrs,oldl+1);
			Brand b = new Brand ();
			b.Name=brandName;
			b.Slogan = slogan;
			b.Logo = logo;
			oldbrs [oldl] = b;
			Brands=oldbrs;
			return b;
		}

		/// <summary>
		/// Removes the brand.
		/// </summary>
		/// <returns><c>true</c>, if brand was removed, <c>false</c> otherwise.</returns>
		/// <param name="brandName">Brand name.</param>
		public bool RemoveBrand(string brandName)
		{
			Brand b = this.GetBrand (brandName);
			if (b == null)
				return false;			
			//ASSERT Brands.Length>0;
			List<Brand> nb = new List<Brand> (Brands);
			nb.Remove (b);
			Brands = nb.ToArray ();
			return true;
		}

		/// <summary>
		/// Gets or sets the start date.
		/// </summary>
		/// <value>The start date.</value>
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Gets or sets the end date.
		/// </summary>
		/// <value>The end date.</value>
		public DateTime EndDate { get; set; }

		/// <summary>
		/// Finds the product.
		/// </summary>
		/// <returns>The product.</returns>
		/// <param name="reference">Reference.</param>
		public Product FindProduct (string reference)
		{
			Product p = null;
			foreach (Brand b in Brands)
				foreach (ProductCategory c in b.Categories)
					if ((p = c.GetProduct(reference))!=null)
						return p;
			return null;
		}
	}
	
}
