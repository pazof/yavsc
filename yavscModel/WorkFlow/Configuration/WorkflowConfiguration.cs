using System;
using System.Configuration;

namespace Yavsc.Model.WorkFlow.Configuration
{
	public class WorkflowConfiguration : ConfigurationSection
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
		[ConfigurationCollection(typeof(WFProvider),
			AddItemName = "add",
			ClearItemsName = "clear",
			RemoveItemName = "remove")]
		public WFProviderCollection Providers {
			get {
				return this["providers"] as WFProviderCollection;
			}

			set {
				this["providers"]=value;
			}
		}
	}
}

