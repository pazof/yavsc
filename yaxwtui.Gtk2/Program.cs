using System;
using Xwt;

namespace yaxwtui.Gtk2
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
			Application.Initialize (ToolkitType.Gtk);

			MainWindow w = new MainWindow ();
			w.Show ();

			Application.Run ();

			w.Dispose ();
			Application.Dispose ();
		}
	}
}

