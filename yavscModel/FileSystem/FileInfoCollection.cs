using System;
using System.IO;
using System.Collections.Generic;

namespace Yavsc.Model.FileSystem
{
	/// <summary>
	/// File info collection.
	/// </summary>
	public class FileInfoCollection: List<FileInfo>
	{
		/// <summary>
		/// Gets or sets the owner.
		/// </summary>
		/// <value>The owner.</value>
		public string Owner { get; set; }
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FileSystem.FileInfoCollection"/> class.
		/// </summary>
		/// <param name="files">Files.</param>
		public FileInfoCollection (FileInfo [] files)
		{
			AddRange (files);
		}
	}
}

