using System;
using System.Collections.Generic;

using Xamarin.Forms;
using BookAStar;
using System.Collections.ObjectModel;
using BookAStar.Model.Workflow.Messaging;

namespace BookAStar
{
	public partial class EventDetail : ContentPage
	{
		public EventDetail (YaEvent ev)
		{
            InitializeComponent();
            BindingContext = ev;
			btn_webpage.Clicked += (object sender, EventArgs e) => {
				App.PlateformSpecificInstance.OpenWeb(ev.EventWebPage);
			};
		}

	}
}

