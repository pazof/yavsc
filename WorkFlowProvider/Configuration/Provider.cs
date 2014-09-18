using System;
using System.Configuration;

namespace WorkFlowProvider.Configuration
{
	public class WFProvider:ConfigurationElement
	{
		[ConfigurationProperty("name", IsKey=true, IsRequired=true)]
		public string Name {
			get {
				return (string) base ["name"];
			}
			set { base ["name"] = value; }
		}

		[ConfigurationProperty("type")]
		public string Type {
			get { return (string) this ["type"]; }
			set {
				this ["type"] = value;
			}
		}
		[ConfigurationProperty("applicationName")]
		public string ApplicationName {
			get {
				return (string)this ["applicationName"];
			}
			set {
				this ["applicationName"] = value;
			}
		}

		[ConfigurationProperty("connectionStringName")]
		public string ConnectionStringName {
			get { return (string)this ["connectionStringName"]; }
			set { this ["connectionStringName"] = value; }
		}
	}
}

