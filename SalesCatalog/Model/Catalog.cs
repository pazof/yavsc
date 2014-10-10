using System;
using System.Collections.Generic;

namespace SalesCatalog.Model
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

		public Brand GetBrand(string brandName)
		{
			return Array.Find<Brand>(Brands, b => b.Name == brandName);
		}

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

		public bool RemoveBrand(string brandName)
		{
			Brand b = this.GetBrand (brandName);
			if (b == null)
				return false;			
			//assert(Brands.Length>0);
			List<Brand> nb = new List<Brand> (Brands);
			nb.Remove (b);
			Brands = nb.ToArray ();
			return true;
		}

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

	}
	
}
