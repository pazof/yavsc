using System;
using System.Configuration;
using System.Reflection;
using System.Collections.Specialized;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Blog helper.
	/// </summary>
	public static class BlogHelper
	{
		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <returns>The provider.</returns>
		public static BlogProvider GetProvider ()
		{
			DataProviderConfigurationSection config = ConfigurationManager.GetSection ("system.web/blog") as DataProviderConfigurationSection;
			if (config == null)
				throw new ConfigurationErrorsException("The configuration bloc for the blog provider was not found");
			ProviderSettings celt = 
				config.Providers[config.DefaultProvider];
			if (config == null)
				throw new ConfigurationErrorsException("The default blog provider was not found");
			ConstructorInfo ci = Type.GetType (celt.Type).GetConstructor (Type.EmptyTypes);
			BlogProvider bp = ci.Invoke (Type.EmptyTypes) as BlogProvider;
			bp.Initialize (celt.Name, celt.Parameters);
			return bp;
		}


	}

}
