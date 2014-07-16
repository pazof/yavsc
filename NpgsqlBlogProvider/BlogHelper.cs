using System;
using System.Configuration;
using System.Reflection;
using System.Collections.Specialized;
using Npgsql.Web.Blog.Configuration;

namespace Npgsql.Web.Blog
{
	public static class BlogHelper
	{
		public static BlogProvider GetProvider ()
		{
			BlogProvidersConfigurationSection config = ConfigurationManager.GetSection ("system.web/blog") as BlogProvidersConfigurationSection;
			if (config == null)
				throw new ConfigurationErrorsException("The configuration bloc for the blog provider was not found");
			BlogProviderConfigurationElement celt = 
				config.Providers.GetElement (config.DefaultProvider);
			if (config == null)
				throw new ConfigurationErrorsException("The default blog provider was not found");
			ConstructorInfo ci = Type.GetType (celt.Type).GetConstructor (Type.EmptyTypes);
			BlogProvider bp = ci.Invoke (Type.EmptyTypes) as BlogProvider;
			NameValueCollection c = new NameValueCollection ();
			c.Add ("name", celt.Name);
			c.Add ("type", celt.Type);
			c.Add ("connectionStringName", celt.ConnectionStringName);
			c.Add ("description", celt.Description);
			c.Add ("applicationName", celt.ApplicationName);
			bp.Initialize (celt.Name, c);
			return bp;
		}


	}

}
