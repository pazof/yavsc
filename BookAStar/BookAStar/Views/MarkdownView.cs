using BookAStar.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;

namespace BookAStar.Views
{
    public class MarkdownView : View
    {
        public static readonly BindableProperty MarkdownProperty = BindableProperty.Create(
            "Markdown", typeof(string), typeof(MarkdownView), null, BindingMode.TwoWay
            );

        public string Markdown
        {
            get
            {
                return GetValue(MarkdownProperty) as string;
            }
            set
            {
                if (Markdown != value)
                {
                    SetValue(MarkdownProperty, value);
                    
                    if (Modified != null)
                    {
                        Modified.Invoke(this, new EventArgs());
                        return;
                    }
                }
            }
        }
        
        public event EventHandler<EventArgs> Modified;
        
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            double width = widthConstraint;
            double height = heightConstraint;

            if (MinimumWidthRequest>0)
                width = widthConstraint > MinimumWidthRequest ? widthConstraint < double.MaxValue ? widthConstraint : MinimumWidthRequest : MinimumWidthRequest;

            if (MinimumHeightRequest > 0)
                height = heightConstraint > MinimumHeightRequest ? heightConstraint < double.MaxValue ? heightConstraint : MinimumHeightRequest : MinimumHeightRequest;

            return base.OnMeasure(width, height);
        }

    }

}
