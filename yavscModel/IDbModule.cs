using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Data;
using System.Web.Mvc;

namespace Yavsc.Model
{
	/// <summary>
	/// I module.
	/// </summary>
	public interface IDbModule
	{
		/// <summary>
		/// Install the model in database using the specified cnx.
		/// </summary>
		/// <param name="cnx">Cnx.</param>
		void Install(IDbConnection cnx);
		/// <summary>
		/// Uninstall the module data and data model from 
		/// database, using the specified connection.
		/// </summary>
		/// <param name="cnx">Cnx.</param>
		/// <param name="removeConfig">If set to <c>true</c> remove config.</param>
		void Uninstall(IDbConnection cnx, bool removeConfig);
		/// <summary>
		/// Initialize this object using the specified name and config.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="config">Config.</param>
		void Initialize (string name, NameValueCollection config);
	}
}

