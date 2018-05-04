﻿using Xwt;

namespace yaxwtui
{
	public class MainWindow : Window
	{
		public MainWindow ()
		{
			Title = "yaxwtui";
			Width = 400;
			Height = 400;

			Menu menu = new Menu ();

			var file = new MenuItem ("_File");
			file.SubMenu = new Menu ();
			file.SubMenu.Items.Add (new MenuItem ("_Open"));
			file.SubMenu.Items.Add (new MenuItem ("_New"));
			MenuItem mi = new MenuItem ("_Close");
			mi.Clicked += (sender, e) => Close ();
			file.SubMenu.Items.Add (mi);
			menu.Items.Add (file);
			MainMenu = menu;
			
			var sampleLabel = new Label ("yaxwtui");
			Content = sampleLabel;
		}

		protected override bool OnCloseRequested ()
		{
			var allow_close = MessageDialog.Confirm ("yaxwtui will be closed", Command.Ok);
			if (allow_close)
				Application.Exit ();
			return allow_close;
		}
	}
}


