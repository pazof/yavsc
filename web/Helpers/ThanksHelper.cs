using System;
using System.Configuration;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Yavsc.Helpers
{
	/// <summary>
	/// Link.
	/// </summary>
	public class Link {
		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>The text.</value>
		public string Text { get; set; }
		/// <summary>
		/// Gets or sets the URL.
		/// </summary>
		/// <value>The URL.</value>
		public string Url { get; set; }
		/// <summary>
		/// Gets or sets the image.
		/// </summary>
		/// <value>The image.</value>
		public string Image { get; set; }
	}

	/// <summary>
	/// Thanks helper.
	/// </summary>
	public static class ThanksHelper {
        
		static private ThanksConfigurationSection configurationSection=null;   
		/// <summary>
		/// Gets the configuration section.
		/// </summary>
		/// <value>The configuration section.</value>
		static public ThanksConfigurationSection ConfigurationSection {
			get {
				if (configurationSection==null)
					configurationSection = (ThanksConfigurationSection) ConfigurationManager.GetSection ("system.web/thanks");
				return configurationSection;
			}
		}
		/// <summary>
		/// Html code for each entry
		/// </summary>
		public static Link[] Thanks (this HtmlHelper helper)
		{
			List<Link> result = new List<Link>() ;
			if (ConfigurationSection == null) return result.ToArray();
			if (ConfigurationSection.To == null) return result.ToArray();
			foreach (ThanksConfigurationElement e in ConfigurationSection.To)
				result.Add( new Link { Url = e.Url, Image=e.Image, Text = e.Name });
			return result.ToArray();
		}
	}
	
}
