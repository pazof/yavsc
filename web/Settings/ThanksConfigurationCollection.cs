using System;
using System.Configuration;

namespace Yavsc
{
	public class ThanksConfigurationCollection : ConfigurationElementCollection
	{
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ThanksConfigurationElement) element).Name;
		}
		protected override ConfigurationElement CreateNewElement ()
		{
			return new ThanksConfigurationElement();
		}
		public ThanksConfigurationElement GetElement (string name)
		{
			return this.BaseGet(name) as ThanksConfigurationElement;
		}
	}
}

