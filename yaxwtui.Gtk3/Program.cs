using System;
using Xwt;

namespace yaxwtui.Gtk3
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
			Application.Initialize (ToolkitType.Gtk3);

			MainWindow w = new MainWindow ();
			w.Show ();

			Application.Run ();

			w.Dispose ();
			Application.Dispose ();
		}
	}
}

