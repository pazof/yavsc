using System;
using System.Configuration;
using System.ComponentModel;

namespace SalesCatalog.Configuration
{
	public class CatalogProvidersConfigurationCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement ()
		{
			return new CatalogProviderConfigurationElement();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((CatalogProviderConfigurationElement) element).Name;
		}

		public CatalogProviderConfigurationElement GetElement (string name)
		{
			return this.BaseGet(name) as CatalogProviderConfigurationElement;
		}
	}

}

