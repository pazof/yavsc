using System;
using System.Configuration;
using System.ComponentModel;

namespace Yavsc.Model.FrontOffice.Configuration
{
	/// <summary>
	/// Catalog providers configuration collection.
	/// </summary>
	public class CatalogProvidersConfigurationCollection : ConfigurationElementCollection
	{
		/// <summary>
		/// Creates the new element.
		/// </summary>
		/// <returns>The new element.</returns>
		protected override ConfigurationElement CreateNewElement ()
		{
			return new CatalogProviderConfigurationElement();
		}

		/// <summary>
		/// Gets the element key.
		/// </summary>
		/// <returns>The element key.</returns>
		/// <param name="element">Element.</param>
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((CatalogProviderConfigurationElement) element).Name;
		}

		/// <summary>
		/// Gets the element.
		/// </summary>
		/// <returns>The element.</returns>
		/// <param name="name">Name.</param>
		public CatalogProviderConfigurationElement GetElement (string name)
		{
			return this.BaseGet(name) as CatalogProviderConfigurationElement;
		}
	}

}

