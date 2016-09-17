using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;
          
namespace BookAStar.Views
{
    public partial class MarkdownView : ContentView
    {
        public static readonly BindableProperty MarkdownProperty = BindableProperty.Create(
            "Markdown", typeof(string), typeof(MarkdownView), null, BindingMode.TwoWay
            );

        private string markdown;

        public string Markdown
        {
            get
            {
                return markdown;
            }
            set {
                markdown = value;
                Content = App.PlateformSpecificInstance.CreateMarkdownView(
                    markdown,
                    e => {
                        Markdown = e;
                        if (Validated != null)
                            Validated.Invoke(this, new EventArgs());
                    }
                    );

            }
        }

        public MarkdownView() : base()
        {
            
        }

        public event EventHandler Validated;

    }
}
