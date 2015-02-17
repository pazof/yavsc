using System;
using System.Configuration;

namespace Yavsc.Model.FrontOffice.Configuration
{
	/// <summary>
	/// Catalog provider configuration element.
	/// </summary>
	public class CatalogProviderConfigurationElement : ConfigurationElement
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[ConfigurationProperty("name", IsRequired = true, IsKey=true)]
		public string Name {
			get { return (string)this ["name"]; }
			set { this ["name"] = value; }
		}

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		[ConfigurationProperty("type", IsRequired = true)]
		public string Type {
			get { return (string)this ["type"]; }
			set { this ["type"] = value; }
		}

		/// <summary>
		/// Gets or sets the connection.
		/// </summary>
		/// <value>The connection.</value>
		[ConfigurationProperty("connection")]
		public string Connection {
			get { return (string)this ["connection"]; }
			set { this ["connection"] = value; }
		}

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[ConfigurationProperty("description")]
		public string Description {
			get { return (string)this ["description"]; }
			set { this ["description"] = value; }
		}

		/// <summary>
		/// Gets or sets the name of the application.
		/// </summary>
		/// <value>The name of the application.</value>
		[ConfigurationProperty("applicationName")]
		public string ApplicationName {
			get { return (string)this ["applicationName"]; }
			set { this ["applicationName"] = value; }
		}
	}
}
