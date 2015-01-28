using System;
using System.Configuration;

namespace Yavsc.Model.FrontOffice.Configuration
{

	public class CatalogProviderConfigurationElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true, IsKey=true)]
		public string Name {
			get { return (string)this ["name"]; }
			set { this ["name"] = value; }
		}

		[ConfigurationProperty("type", IsRequired = true)]
		public string Type {
			get { return (string)this ["type"]; }
			set { this ["type"] = value; }
		}

		[ConfigurationProperty("connection")]
		public string Connection {
			get { return (string)this ["connection"]; }
			set { this ["connection"] = value; }
		}

		[ConfigurationProperty("description")]
		public string Description {
			get { return (string)this ["description"]; }
			set { this ["description"] = value; }
		}

		[ConfigurationProperty("applicationName")]
		public string ApplicationName {
			get { return (string)this ["applicationName"]; }
			set { this ["applicationName"] = value; }
		}
	}
}
