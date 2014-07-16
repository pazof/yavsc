using System;
using System.ComponentModel.DataAnnotations;

namespace SalesCatalog.Model
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

