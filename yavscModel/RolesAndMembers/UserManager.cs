using System.Configuration;
using System.Reflection;
using System;

namespace Yavsc.Model.RolesAndMembers
{
	/// <summary>
	/// User manager.
	/// </summary>
	public static class UserManager
	{
		/// <summary>
		/// Changes the name.
		/// </summary>
		/// <param name="oldName">Old name.</param>
		/// <param name="newName">New name.</param>
		public static void ChangeName (string oldName, string newName) {
			Provider.ChangeName (oldName, newName);
		}

		/// <summary>
		/// Determines if is available the specified name.
		/// </summary>
		/// <returns><c>true</c> if is available the specified name; otherwise, <c>false</c>.</returns>
		/// <param name="name">Name.</param>
		public static bool IsAvailable(string name)
		{
			return Provider.IsNameAvailable (name);
		}
		/// <summary>
		/// The provider.
		/// </summary>
		private static ChangeUserNameProvider provider;

		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <value>The provider.</value>
		public static ChangeUserNameProvider Provider {
			get {
				if (provider == null)
					provider = GetProvider ();
				if (provider == null)
					throw new ConfigurationErrorsException ("No username section defined");
				return provider;
			}
		}

		private static ChangeUserNameProvider GetProvider ()
		{
			DataProviderConfigurationSection config = ConfigurationManager.GetSection ("system.web/userNameManager") as DataProviderConfigurationSection;
			if (config == null)
				throw new ConfigurationErrorsException("The configuration bloc for the username provider was not found");
			ProviderSettings celt=null;
			if (config.DefaultProvider!=null)
				celt = config.Providers[config.DefaultProvider];
			if ((celt == null) && config.Providers!=null)
			if (config.Providers.Count>0)
				celt = config.Providers [0];
			if (celt == null)
				throw new ConfigurationErrorsException("The default username provider was not found");
			ConstructorInfo ci = Type.GetType (celt.Type).GetConstructor (Type.EmptyTypes);
			provider = ci.Invoke (Type.EmptyTypes) as ChangeUserNameProvider;
			provider.Initialize (celt.Name, celt.Parameters);
			return provider;
		}
	}

}
