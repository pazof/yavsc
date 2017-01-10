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
using Android.Webkit;

namespace BookAStar.Droid.Markdown
{
    class MDWebView : WebView
    {
        public MDWebView (Context context) : base (context)
        {
        }

        public override ActionMode StartActionMode(ActionMode.ICallback callback)
        {
            return base.StartActionMode(callback);
        }

    }
}