using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Profile;
using System.Web.Security;
using Yavsc;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Helpers;
using System.Web.Mvc;
using Yavsc.Model.Circles;
using System.Collections.Specialized;
using Yavsc.Model;
using System.Configuration;
using System.Reflection;

namespace Yavsc.Controllers
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
				return provider;
			}
		}

		private static ChangeUserNameProvider GetProvider ()
		{
			DataProviderConfigurationSection config = ConfigurationManager.GetSection ("system.web/blog") as DataProviderConfigurationSection;
			if (config == null)
				throw new ConfigurationErrorsException("The configuration bloc for the blog provider was not found");
			ProviderSettings celt = 
				config.Providers[config.DefaultProvider];
			if (config == null)
				throw new ConfigurationErrorsException("The default blog provider was not found");
			ConstructorInfo ci = Type.GetType (celt.Type).GetConstructor (Type.EmptyTypes);
			provider = ci.Invoke (Type.EmptyTypes) as ChangeUserNameProvider;
			provider.Initialize (celt.Name, celt.Parameters);
			return provider;
		}
	}

}
