using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Data;

namespace Yavsc.Model
{
	public interface IModule
	{
		/// <summary>
		/// Install the model in database using the specified cnx.
		/// </summary>
		/// <param name="cnx">Cnx.</param>
		void Install(IDbConnection cnx);
		/// <summary>
		/// Uninstall the module data and data model from 
		/// database, using the specified cnx.
		/// </summary>
		/// <param name="cnx">Cnx.</param>
		/// <param name="removeConfig">If set to <c>true</c> remove config.</param>
		void Uninstall(IDbConnection cnx,bool removeConfig);
		ConfigurationSection DefaultConfig (string appName, string cnxStr);
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.Model.IModule"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		bool Active { get; set; }
		/// <summary>
		/// Gets or sets the name of the application.
		/// </summary>
		/// <value>The name of the application.</value>
		string ApplicationName { get; set; }
		void Initialize (string name, NameValueCollection config);
	}
}

