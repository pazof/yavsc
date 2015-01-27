using System;
using System.Configuration;

namespace Yavsc
{
	/// <summary>
	/// Thanks configuration collection.
	/// Imlements the configuration, 
	/// providing the thanks collection
	/// </summary>
	public class ThanksConfigurationCollection : ConfigurationElementCollection
	{
		/// <summary>
		/// Gets the element key.
		/// </summary>
		/// <returns>The element key.</returns>
		/// <param name="element">Element.</param>
		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((ThanksConfigurationElement) element).Name;
		}
		/// <summary>
		/// Creates the new element.
		/// </summary>
		/// <returns>The new element.</returns>
		protected override ConfigurationElement CreateNewElement ()
		{
			return new ThanksConfigurationElement();
		}
		/// <summary>
		/// Gets the element.
		/// </summary>
		/// <returns>The element.</returns>
		/// <param name="name">Name.</param>
		public ThanksConfigurationElement GetElement (string name)
		{
			return this.BaseGet(name) as ThanksConfigurationElement;
		}
	}
}

