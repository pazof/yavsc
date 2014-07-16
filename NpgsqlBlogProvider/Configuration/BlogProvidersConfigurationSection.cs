using System;
using System.Configuration;
using System.ComponentModel;

namespace Npgsql.Web.Blog.Configuration
{
	public class BlogProvidersConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("defaultProvider")]
		public string DefaultProvider {
			get { return (string)base ["defaultProvider"]; }
			set { base ["defaultProvider"] = value; }
		}

		[ConfigurationProperty("providers")]
		[ConfigurationCollection(typeof(BlogProvidersConfigurationCollection),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]
		public BlogProvidersConfigurationCollection	Providers{
			get { return (BlogProvidersConfigurationCollection) base ["providers"]; }
			set { base ["providers"] = value; }
		}
	}
	
}
