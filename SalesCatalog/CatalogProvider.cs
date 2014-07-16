using System;
using System.Configuration.Provider;
using SalesCatalog.Model;

namespace SalesCatalog
{
	/// <summary>
	/// Catalog provider.<br/>
	/// Abstract class, inherited to implement a catalog provider.
	/// </summary>
	public abstract class CatalogProvider: ProviderBase
	{
		public abstract Catalog GetCatalog ();
	}
}

