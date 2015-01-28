using System;
using System.Configuration;
using System.ComponentModel;

namespace Yavsc.Model.FrontOffice.Configuration
{
	public class CatalogProvidersConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("defaultProvider")]
		public string DefaultProvider {
			get { return (string)base ["defaultProvider"]; }
			set { base ["defaultProvider"] = value; }
		}

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
