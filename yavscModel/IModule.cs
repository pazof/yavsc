using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Data;
using System.Web.Mvc;

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
		void Initialize (string name, NameValueCollection config);
	}
}

