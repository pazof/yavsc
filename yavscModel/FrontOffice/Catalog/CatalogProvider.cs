using System;
using System.Configuration.Provider;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Catalog provider.<br/>
	/// Abstract class, inherited to implement a catalog provider.
	/// </summary>
	public abstract class CatalogProvider: ProviderBase
	{
		/// <summary>
		/// Gets the catalog.
		/// </summary>
		/// <returns>The catalog.</returns>
		public abstract Catalog GetCatalog ();
	}
}

