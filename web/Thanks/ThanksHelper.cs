using System;
using System.Configuration;
using System.Collections.Generic;

namespace Yavsc
{
	public static class ThanksHelper {

		public static string[] Links ()
		{
			List<string> result = new List<string>() ;
			ThanksConfigurationSection s = (ThanksConfigurationSection) ConfigurationManager.GetSection ("system.web/thanks");
			if (s == null) return result.ToArray();
			if (s.To == null) return result.ToArray();
			foreach (ThanksConfigurationElement e in s.To) {
				string link = "";
				if (!string.IsNullOrEmpty(e.Url))
					link = string.Format("<a class=\"athanks\" href=\"{0}\">",e.Url);
				link += "<div class=\"thanks\">";
				string dsp = (string.IsNullOrEmpty(e.Display))?e.Name:e.Display;
				if (!string.IsNullOrEmpty(e.Image)) {
					string ttl = (string.IsNullOrEmpty(s.TitleFormat))?"Go and see the website ({0})":s.TitleFormat;
					ttl = string.Format(ttl,dsp);
					link += string.Format(
						"<img src=\"{1}\" alt=\"{0}\" title=\"{2}\"/>",
						dsp,e.Image,ttl);
				}
				else link += dsp;
				link += "</div>";
				if (e.Url!=null)
					link += "</a> ";
				result.Add (link);
			}
			return result.ToArray();
		}
	}
	
}
