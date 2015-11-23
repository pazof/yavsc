using System;
using System.Collections.Generic;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// I data provider.
	/// </summary>
	public interface IDataProvider<T,IDT> where T : IIdentified<IDT>
	{
		/// <summary>
		/// Get the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		T Get (IDT id);
		/// <summary>
		/// Update the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		void Update (T data);
	}

}

