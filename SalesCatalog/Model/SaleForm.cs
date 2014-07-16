using System;
using System.Collections.Generic;

namespace SalesCatalog.Model
{
	public class SaleForm
	{
		public SaleForm ()
		{
		}

		public string Action {
			get;
			set;
		}

		public FormElement[] Items { get; set; }
	}
}

