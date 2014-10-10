using System;
using SalesCatalog.Model;

namespace SalesCatalog
{
	/// <summary>
	/// Catalog manager.
	/// Use this class to retreive the catalog or its elements
	/// </summary>
	public static class CatalogManager
	{
		private static CatalogProvider defaultProvider = null;
		public static Catalog GetCatalog ()
		{
			if (defaultProvider == null) {
				if (CatalogHelper.Config == null)
					CatalogHelper.LoadConfig ();
				defaultProvider = CatalogHelper.GetDefaultProvider ();

			}
			return defaultProvider.GetCatalog ();
		}
	}
}

