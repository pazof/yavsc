using System;
using System.Collections.Generic;

namespace yavscModel.WorkFlow
{
	public interface IContentProvider: IDisposable
	{
		string Order (IWFCommand c);
		IContent Get (string orderId);
	}
}

