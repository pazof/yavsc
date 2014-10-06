using System;
using System.Configuration;
using System.Collections.Specialized;

namespace Yavsc.Model
{
	public interface IModule
	{
		void Install();
		void Uninstall();
		ConfigurationSection DefaultConfig (string appName, string cnxStr);
		bool Active { get; set; }
		string ApplicationName { get; set; }
		void Initialize (string name, NameValueCollection config);
	}
}

