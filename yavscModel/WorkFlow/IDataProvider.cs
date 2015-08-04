using System;
using System.Collections.Generic;
using Yavsc.Model.FrontOffice;

namespace Yavsc.Model.WorkFlow
{
	public interface IDataProvider<T>
	{
		T Get (long id);
		void Update (T data);
	}

}

