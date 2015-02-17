using System;
using System.Configuration;
using System.ComponentModel;

namespace Yavsc.Model.FrontOffice.Configuration
{
	/// <summary>
	/// Catalog providers configuration section.
	/// </summary>
	public class CatalogProvidersConfigurationSection : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets the default provider.
		/// </summary>
		/// <value>The default provider.</value>
		[ConfigurationProperty("defaultProvider")]
		public string DefaultProvider {
			get { return (string)base ["defaultProvider"]; }
			set { base ["defaultProvider"] = value; }
		}

		/// <summary>
		/// Gets or sets the providers.
		/// </summary>
		/// <value>The providers.</value>
		[ConfigurationProperty("providers")]
		[ConfigurationCollection(typeof(CatalogProvidersConfigurationCollection),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]
		public CatalogProvidersConfigurationCollection	Providers{
			get { return (CatalogProvidersConfigurationCollection) base ["providers"]; }
			set { base ["providers"] = value; }
		}
	}
	
}
