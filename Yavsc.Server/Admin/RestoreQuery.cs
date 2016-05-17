using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Admin
{
	/// <summary>
	/// Restore query.
	/// </summary>
	public class RestoreQuery: DataAccess
	{
		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		[Required]
		[StringLength(2056)]
		public string FileName { get; set ; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.Admin.RestoreQuery"/> class.
		/// </summary>
		public RestoreQuery ()
		{
		}
	}
}

