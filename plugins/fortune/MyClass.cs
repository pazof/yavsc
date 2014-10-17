using System;
using Yavsc.Model;
using System.Configuration;

namespace fortune
{
	public class MyClass : IModule
	{
		public MyClass ()
		{
		}

		#region IModule implementation

		public void Install (System.Data.IDbConnection cnx)
		{
			throw new NotImplementedException ();
		}

		public void Uninstall (System.Data.IDbConnection cnx, bool removeConfig)
		{
			throw new NotImplementedException ();
		}

		public ConfigurationSection DefaultConfig (string appName, string cnxStr)
		{
			throw new NotImplementedException ();
		}

		public void Initialize (string name, System.Collections.Specialized.NameValueCollection config)
		{
			throw new NotImplementedException ();
		}

		public bool Active {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public string ApplicationName {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}

