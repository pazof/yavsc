using System;
using System.Configuration;
using System.ComponentModel;

namespace Npgsql.Web.Blog.Configuration
{
	public class BlogProvidersConfigurationCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement ()
		{
			return new BlogProviderConfigurationElement();
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((BlogProviderConfigurationElement) element).Name;
		}

		public BlogProviderConfigurationElement GetElement (string name)
		{
			return this.BaseGet(name) as BlogProviderConfigurationElement;
		}
	}

}

