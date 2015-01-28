using System;

namespace Yavsc.Model.FrontOffice
{
	public abstract class Unit
	{
		public abstract string Name { get; }
		public abstract string Description { get; }
		public abstract object ConvertTo (Unit dest, object value);
		public abstract bool MayConvertTo (Unit other);
	}
}

