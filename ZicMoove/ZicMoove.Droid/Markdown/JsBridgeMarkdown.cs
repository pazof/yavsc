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
using Java.Interop;

namespace BookAStar.Droid.Markdown
{
    public class JsBridgeMarkdown : Java.Lang.Object
    {
        readonly WeakReference<MarkdownViewRenderer> hybridWebViewRenderer;

        public JsBridgeMarkdown(MarkdownViewRenderer hybridRenderer)
        {
            hybridWebViewRenderer = new WeakReference<MarkdownViewRenderer>(hybridRenderer);
        }

        [JavascriptInterface]
        [Export("contentEdited")]
        public void ContentEdited(string data)
        {
            MarkdownViewRenderer hybridRenderer;

            if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget(out hybridRenderer))
            {
                hybridRenderer.Element.Markdown = data;
            }
        }

        [JavascriptInterface]
        [Export("jsLoaded")]
        public void JSLoaded()
        {

        }
    }
}