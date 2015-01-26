using System;
using System.Collections.Generic;
using System.IO;

namespace Yavsc
{
	/// <summary>
	/// File info collection.
	/// </summary>
	public class FileInfoCollection : List<FileInfo>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.FileInfoCollection"/> class.
		/// </summary>
		/// <param name="collection">Collection.</param>
		public FileInfoCollection (System.IO.FileInfo[] collection)
		{
			this.AddRange (collection);
		}
	}
}

