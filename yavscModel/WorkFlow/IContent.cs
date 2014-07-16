using System;
using System.Collections.Generic;

namespace yavscModel.WorkFlow
{
	public interface IContent
	{
		object Data { get; set; }
		string MimeType { get; set; }

	}
}

