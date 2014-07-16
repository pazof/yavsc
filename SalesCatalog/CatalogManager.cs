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
		public static Catalog GetCatalog ()
		{
			CatalogProvider p = CatalogHelper.GetProvider ();
			return p.GetCatalog ();
		}
	}
}

