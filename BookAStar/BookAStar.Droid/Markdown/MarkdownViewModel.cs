using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BookAStar.Droid.Markdown
{
    public class MarkdownViewModel
    {
        protected static MarkdownDeep.Markdown markdown = new MarkdownDeep.Markdown();
        public string Content { get; set; }
        public bool Editable { get; set; }
        public string GetHtml()
        {
            return markdown.Transform(Content);
        }
        public override string ToString()
        {
            return Content;
        }
    }
}