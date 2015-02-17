using System;
using System.Configuration;
using System.Reflection;
using System.Collections.Specialized;
using Yavsc.Model.FrontOffice.Configuration;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Catalog helper.
	/// Used by the catalog manager to get the catalog provider from the configuration.
	/// </summary>
	public static class CatalogHelper
	{
		/// <summary>
		/// Gets or sets the config.
		/// </summary>
		/// <value>The config.</value>
		public static CatalogProvidersConfigurationSection Config {get; set; }

		/// <summary>
		/// Loads the config.
		/// </summary>
		public static void LoadConfig () {
			Config = ConfigurationManager.GetSection ("system.web/catalog") as CatalogProvidersConfigurationSection;
			if (Config == null)
				throw new ConfigurationErrorsException("The configuration bloc for the catalog provider was not found");
			/* foreach (CatalogProviderConfigurationElement e in Config.Providers) {
				  Providers.Add(CreateProvider (e));
			} */
		}
		private static CatalogProvider CreateProvider(CatalogProviderConfigurationElement celt) {
			if (celt == null)
				throw new ConfigurationErrorsException("The default catalog provider was not found");
			Type catprtype = Type.GetType (celt.Type);
			if (catprtype == null)
				throw new Exception (
					string.Format("The catalog provider type ({0}) could not be found",celt.Type)); 
			ConstructorInfo ci = catprtype.GetConstructor (Type.EmptyTypes);
			if (ci==null)
				throw new Exception (
					string.Format("The catalog provider type ({0}) doesn't contain public constructor with empty parameter list",celt.Type)); 

			CatalogProvider cp = ci.Invoke (Type.EmptyTypes) as CatalogProvider;
			NameValueCollection c = new NameValueCollection ();
			c.Add ("name", celt.Name);
			c.Add ("type", celt.Type);
			c.Add ("connection", celt.Connection);
			c.Add ("description", celt.Description);
			c.Add ("applicationName", celt.ApplicationName);
			cp.Initialize (celt.Name, c);
			return cp;
		}
		/// <summary>
		/// Gets the default provider.
		/// 
		/// </summary>
		/// <returns>The default provider.</returns>
		public static CatalogProvider GetDefaultProvider ()
		{
			if (Config == null)
				throw new Exception ("Configuration wanted, use a call to \"Load\".");
			CatalogProviderConfigurationElement celt = 
				Config.Providers.GetElement (Config.DefaultProvider);

			return CreateProvider (celt);
		}
	}
}

