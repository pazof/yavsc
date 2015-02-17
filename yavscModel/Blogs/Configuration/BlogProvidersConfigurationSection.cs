using System;
using System.Configuration;
using System.ComponentModel;

namespace Yavsc.Model.Blogs.Configuration
{
	/// <summary>
	/// Blog providers configuration section.
	/// </summary>
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

		/// <summary>
		/// Gets or sets the providers.
		/// </summary>
		/// <value>The providers.</value>
		[ConfigurationProperty("providers")]
		[ConfigurationCollection(typeof(BlogProvidersConfigurationCollection),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]
		public BlogProvidersConfigurationCollection	Providers{
			get { return (BlogProvidersConfigurationCollection) this["providers"]; }
			set { this["providers"] = value; }
		}
	}
	
}
