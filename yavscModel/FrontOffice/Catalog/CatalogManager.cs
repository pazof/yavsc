using System;
namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Catalog manager.
	/// Use this class to retreive the catalog or its elements
	/// </summary>
	public static class CatalogManager
	{
		private static CatalogProvider defaultProvider = null;

		/// <summary>
		/// Gets the catalog.
		/// </summary>
		/// <returns>The catalog.</returns>
		/// <param name="catalogUri">Catalog URI.</param>
		public static Catalog GetCatalog (string catalogUri)
		{

			if (defaultProvider == null) {
				if (CatalogHelper.Config == null)
					CatalogHelper.LoadConfig ();
				defaultProvider = CatalogHelper.GetDefaultProvider ();
			}
			Catalog res = defaultProvider.GetCatalog ();

			// Assert res.Brands.All( x => x.DefaultForm != null );
			// Sanity fixes
			foreach (Brand b in res.Brands) {
				if (b.DefaultForm.CatalogReference==null)
					b.DefaultForm.CatalogReference = catalogUri;
				foreach (ProductCategory pc in b.Categories) {
					foreach (Product p in pc.Products) {
						if (p.CommandForm == null)
							p.CommandForm = b.DefaultForm;
					}
				}
			}
			return res;
		}
	}
}

