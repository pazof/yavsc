using System;
using Xwt;

namespace yaxwtui.Wpf
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
			Application.Initialize (ToolkitType.Wpf);

			MainWindow w = new MainWindow ();
			w.Show ();

			Application.Run ();

			w.Dispose ();
			Application.Dispose ();
		}
	}
}

