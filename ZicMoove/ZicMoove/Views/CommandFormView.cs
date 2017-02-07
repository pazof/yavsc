using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;
using ZicMoove.Model.Workflow;

namespace ZicMoove.Views
{
    public class CommandFormView : ContentView
    {
        public static readonly BindableProperty CommandType = BindableProperty.Create(
            "Type",
            typeof(string),
            typeof(CommandFormView)
            );

        public string Type
        {
            get
            {
                return GetValue(CommandType) as string;
            }
            set {
                SetValue(CommandType, value);
            }
        }

        CommandForm Context { get { return BindingContext as CommandForm; } }

        public CommandFormView()
        {
            Content = new Label { Text = Context?.Title };
            
        }
    }
}
