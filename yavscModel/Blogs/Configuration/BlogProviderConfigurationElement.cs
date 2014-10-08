using System;
using System.Configuration;
using System.ComponentModel;

namespace Yavsc.Model.Blogs.Configuration
{

	public class BlogProviderConfigurationElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true, IsKey=true)]
		public string Name {
			get { return (string)this ["name"]; }
			set { this ["name"] = value; }
		}

		[ConfigurationProperty("type", IsRequired = true, IsKey=false)]
		public string Type {
			get { return (string)this ["type"]; }
			set { this ["type"] = value; }
		}

		[ConfigurationProperty("connectionStringName")]
		public string ConnectionStringName {
			get { return (string)this ["connectionStringName"]; }
			set { this ["connectionStringName"] = value; }
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
