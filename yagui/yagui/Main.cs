using System;

namespace yagui
{
	public partial class Main : Gtk.Window
	{
		public Main () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
		}
	}
}

