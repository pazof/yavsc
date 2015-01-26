using System;
using System.ComponentModel;

namespace Yavsc.Admin
{
	/// <summary>
	/// Export.
	/// </summary>
	public class Export: TaskOutput
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Admin.Export"/> class.
		/// </summary>
		public Export ()
		{
		}
		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		public string FileName { get; set; }
	}
}

