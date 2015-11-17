using System;
using System.Collections.Generic;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// I data provider.
	/// </summary>
	public interface IDataProvider<T>
	{
		/// <summary>
		/// Get the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		T Get (long id);
		/// <summary>
		/// Update the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		void Update (T data);
	}

}

