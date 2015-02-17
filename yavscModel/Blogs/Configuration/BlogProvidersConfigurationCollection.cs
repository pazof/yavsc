using System;
using System.Configuration;
using System.ComponentModel;

namespace Yavsc.Model.Blogs.Configuration
{
	/// <summary>
	/// Blog providers configuration collection.
	/// </summary>
	public class BlogProvidersConfigurationCollection : ConfigurationElementCollection
	{
		/// <summary>
		/// Creates the new element.
		/// </summary>
		/// <returns>The new element.</returns>
		protected override ConfigurationElement CreateNewElement ()
		{
			return new BlogProviderConfigurationElement();
		}

		/// <summary>
		/// Gets the element key.
		/// </summary>
		/// <returns>The element key.</returns>
		/// <param name="element">Element.</param>
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((BlogProviderConfigurationElement) element).Name;
		}

		/// <summary>
		/// Gets the element.
		/// </summary>
		/// <returns>The element.</returns>
		/// <param name="name">Name.</param>
		public BlogProviderConfigurationElement GetElement (string name)
		{
			return this.BaseGet(name) as BlogProviderConfigurationElement;
		}
	}

}

