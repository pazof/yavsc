using System;
using System.Configuration;

namespace WorkFlowProvider.Configuration
{
	public class WFProviderCollection : ConfigurationElementCollection
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((WFProvider) element).Name;
		}
		protected override ConfigurationElement CreateNewElement ()
		{
			return new WFProvider();
		}
		public WFProvider GetElement (string name)
		{
			return this.BaseGet (name) as WFProvider;
		}
	}
}

