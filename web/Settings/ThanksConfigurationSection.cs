using System;
using System.Configuration;

namespace Yavsc
{
	/// <summary>
	/// Thanks configuration section.
	/// </summary>
	public class ThanksConfigurationSection : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets to.
		/// </summary>
		/// <value>To.</value>
		[ConfigurationProperty("to")]
		public ThanksConfigurationCollection To {
			get {
				return  (ThanksConfigurationCollection) this["to"];
			}
			set {
				this ["to"] = value;
			}
		}
		/// <summary>
		/// Gets or sets the html class.
		/// </summary>
		/// <value>The html class.</value>
		[ConfigurationProperty("html_class")]
		public string HtmlClass {
			get {
				return (string)this ["html_class"];
			}
			set {
				this ["html_class"] = value;
			}
		}
		/// <summary>
		/// Gets or sets the title format.
		/// </summary>
		/// <value>The title format.</value>
		[ConfigurationProperty("title_format")]
		public string TitleFormat {
			get {
				return (string)this ["title_format"];
			}
			set {
				this ["title_format"] = value;
			}
		}
	}
}

