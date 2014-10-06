using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.Admin
{
	public class RestoreQuery: DataAccess
	{
		[Required]
		[StringLength(2056)]
		public string FileName { get; set ; }

		public RestoreQuery ()
		{
		}
	}
}

