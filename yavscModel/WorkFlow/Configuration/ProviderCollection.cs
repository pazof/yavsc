using System;
using System.Configuration;

namespace Yavsc.Model.WorkFlow.Configuration
{
	/// <summary>
	/// WF provider collection.
	/// </summary>
	public class WFProviderCollection : ConfigurationElementCollection
	{
		/// <summary>
		/// Gets the element key.
		/// </summary>
		/// <returns>The element key.</returns>
		/// <param name="element">Element.</param>
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((WFProvider) element).Name;
		}
		/// <summary>
		/// Creates the new element.
		/// </summary>
		/// <returns>The new element.</returns>
		protected override ConfigurationElement CreateNewElement ()
		{
			return new WFProvider();
		}
		/// <summary>
		/// Gets the element.
		/// </summary>
		/// <returns>The element.</returns>
		/// <param name="name">Name.</param>
		public WFProvider GetElement (string name)
		{
			return this.BaseGet (name) as WFProvider;
		}
	}
}

