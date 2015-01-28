using System;

namespace Yavsc.Model.FrontOffice
{
	public class SelectItem
	{ 
		public SelectItem(string t)
		{
			Value = t;
		}
		public string Value { get; set; }
		public static implicit operator string(SelectItem t)
		{
			return t.Value;
		}
		public static implicit operator SelectItem(string t)
		{
			return new SelectItem(t);
		}

	}
}

