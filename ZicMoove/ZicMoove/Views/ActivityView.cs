using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;
using ZicMoove.Model.Workflow;

namespace ZicMoove.Views
{
    public class ActivityView : ContentView
    {
        public static readonly BindableProperty ContextProperty = BindableProperty.Create(
            "Context",
            typeof(Activity),
            typeof(ActivityView)
            );

        public Activity Context
        {
            get
            {
                return BindingContext as Activity;
            }
            set
            {
                BindingContext = value;
            }
        }

        public ActivityView()
        {
        }

        public ActivityView(Activity context)
        {
            BindingContext = context;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            var cnt = new StackLayout
            {
            };
            cnt.Children.Add(new Label
            {
                Text = Context.Name,
                Style = Resources["BigLabelStyle"] as Style
            });

            cnt.Children.Add(new Image
            {
                Source = Context.Photo,
                HeightRequest = Device.OnPlatform<double>(40,40,40)
            });

            cnt.Children.Add(new Label
            {
                Text = Context.Description
            });
        }
    }
}
