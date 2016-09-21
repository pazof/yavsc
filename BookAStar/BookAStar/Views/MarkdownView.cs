﻿using BookAStar.Interfaces;
using System;
using System.Collections.Generic;
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

        private string markdown;

        public string Markdown
        {
            get
            {
                return markdown;
            }
            set {
                if (markdown != value)
                    if (Edited != null)
                    {
                        markdown = value;
                        Edited.Invoke(this, new EventArgs());
                        return;
                    }
                
                markdown = value;
            }
        }
        
        public event EventHandler<EventArgs> Edited;
        
    }

}
