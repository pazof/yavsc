using System;
using System.Configuration;
using System.Collections.Generic;

namespace Yavsc
{
	public static class ThanksHelper {
                static private ThanksConfigurationSection configurationSection=null;   
		static public ThanksConfigurationSection ConfigurationSection {
			get {
				if (configurationSection==null)
					configurationSection = (ThanksConfigurationSection) ConfigurationManager.GetSection ("system.web/thanks");
				return configurationSection;
			}
		}
		
		public static string[] Links ()
		{
			List<string> result = new List<string>() ;
			if (ConfigurationSection == null) return result.ToArray();
			if (ConfigurationSection.To == null) return result.ToArray();
			foreach (ThanksConfigurationElement e in ConfigurationSection.To) {
				string link = "";
				if (!string.IsNullOrEmpty(e.Url))
					link = string.Format("<a href=\"{0}\">",e.Url);
				string dsp = (string.IsNullOrEmpty(e.Display))?e.Name:e.Display;
				if (!string.IsNullOrEmpty(e.Image)) {
					string ttl = (string.IsNullOrEmpty(ConfigurationSection.TitleFormat))?"Go and see the website ({0})":ConfigurationSection.TitleFormat;
					ttl = string.Format(ttl,dsp);
					link += string.Format(
						"<img src=\"{1}\" alt=\"{0}\" title=\"{2}\"/>",
						dsp,e.Image,ttl);
				}
				else link += dsp;
				if (e.Url!=null)
					link += "</a> ";
				result.Add (link);
			}
			return result.ToArray();
		}
	}
	
}
