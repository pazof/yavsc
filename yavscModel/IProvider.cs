using System;
using System.Collections.Specialized;

namespace yavscModel
{
	public interface IProvider 
	{
		string ApplicationName { get; set; }
		void Initialize (string name, NameValueCollection config);
	}
}

