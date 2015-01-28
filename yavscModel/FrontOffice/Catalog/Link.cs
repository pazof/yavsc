using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Model.FrontOffice
{
	public class Link:Label
	{
		public Link ()
		{
		}
		[Required]
		public string Ref { get; set; }
	}
}

