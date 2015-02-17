using System;
using System.Configuration;

namespace Yavsc.Model.WorkFlow.Configuration
{
	/// <summary>
	/// WF provider.
	/// </summary>
	public class WFProvider:ConfigurationElement
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[ConfigurationProperty("name", IsKey=true, IsRequired=true)]
		public string Name {
			get {
				return (string) base ["name"];
			}
			set { base ["name"] = value; }
		}

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		[ConfigurationProperty("type")]
		public string Type {
			get { return (string) this ["type"]; }
			set {
				this ["type"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the application.
		/// </summary>
		/// <value>The name of the application.</value>
		[ConfigurationProperty("applicationName")]
		public string ApplicationName {
			get {
				return (string)this ["applicationName"];
			}
			set {
				this ["applicationName"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the connection string.
		/// </summary>
		/// <value>The name of the connection string.</value>
		[ConfigurationProperty("connectionStringName")]
		public string ConnectionStringName {
			get { return (string)this ["connectionStringName"]; }
			set { this ["connectionStringName"] = value; }
		}
	}
}

