using System;
using System.Configuration;

namespace Yavsc
{
	public class ThanksConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("to")]
		public ThanksConfigurationCollection To {
			get {
				return  (ThanksConfigurationCollection) this["to"];
			}
			set {
				this ["to"] = value;
			}
		}

		[ConfigurationProperty("html_class")]
		public string HtmlClass {
			get {
				return (string)this ["html_class"];
			}
			set {
				this ["html_class"] = value;
			}
		}

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

