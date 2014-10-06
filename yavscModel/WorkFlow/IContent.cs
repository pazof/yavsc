using System;
using System.Collections.Generic;

namespace Yavsc.Model.WorkFlow
{
	public interface IContent
	{
		object Data { get; set; }
		string MimeType { get; set; }

	}
}

