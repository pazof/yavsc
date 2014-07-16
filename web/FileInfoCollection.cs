using System;
using System.Collections.Generic;
using System.IO;

namespace Yavsc
{
	public class FileInfoCollection : List<FileInfo>
	{
		public FileInfoCollection (System.IO.FileInfo[] collection)
		{
			this.AddRange (collection);
		}
	}
}

