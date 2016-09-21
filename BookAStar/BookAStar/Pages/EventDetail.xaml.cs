using System;
using Xamarin.Forms;

namespace BookAStar
{
    using Model.Workflow.Messaging;

    namespace Pages
    {
        public partial class EventDetail : ContentPage
        {
            public EventDetail(YaEvent ev)
            {
                InitializeComponent();
                BindingContext = ev;
                btn_webpage.Clicked += (object sender, EventArgs e) =>
                {
                    App.PlatformSpecificInstance.OpenWeb(ev.EventWebPage);
                };
            }

        }
    }
}

