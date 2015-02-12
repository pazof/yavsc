using System;
using System.Configuration;
using System.ComponentModel;

namespace Yavsc.Model.Blogs.Configuration
{
	public class BlogProvidersConfigurationSection : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets the default provider.
		/// </summary>
		/// <value>The default provider.</value>
		[ConfigurationProperty("defaultProvider")]
		public string DefaultProvider {
			get { return (string) this ["defaultProvider"]; }
			set { this["defaultProvider"] = value; }
		}

		[ConfigurationProperty("providers")]
		[ConfigurationCollection(typeof(BlogProvidersConfigurationCollection),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]

		/// <summary>
		/// Gets or sets the providers.
		/// </summary>
		/// <value>The providers.</value>
		public BlogProvidersConfigurationCollection	Providers{
			get { return (BlogProvidersConfigurationCollection) this["providers"]; }
			set { this["providers"] = value; }
		}
	}
	
}
