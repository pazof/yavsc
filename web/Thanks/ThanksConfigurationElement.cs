using System;
using System.Configuration;

namespace Yavsc
{
	public class ThanksConfigurationElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsKey=true, IsRequired=true)]
		public string Name {
			get {
				return (string) base ["name"];
			}
			set { base ["name"] = value; }
		}

		[ConfigurationProperty("url")]
		public string Url {
			get {
				return (string) base ["url"];
			}
			set { base ["url"] = value; }
		}

		[ConfigurationProperty("image")]
		public string Image {
			get {
				return (string) base ["image"];
			}
			set { base ["image"] = value; }
		}

		/// <summary>
		/// Gets or sets the display.
		/// </summary>
		/// <value>
		/// The displaied text when no image is provided and we 
		/// don't want use the name attribute.
		/// </value>
		[ConfigurationProperty("display")]
		public string Display {
			get {
				return (string) base ["display"];
			}
			set { base ["display"] = value; }
		}
	}
}

