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
using Java.Lang;
using Java.IO;
using Android.Content.Res;

namespace BookAStar.Droid
{

    class MarkdownWebViewClient : WebViewClient
    {
        Action<string> update;
        public MarkdownWebViewClient(Action<string> update) : base()
        {
            this.update = update;
        }
        private static Activity getActivity ()
        {
            return (Activity)App.PlatformSpecificInstance;
        }
        public string Markdown { get; private set; }
        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {
            if (request.Url.Scheme=="file")
            {
                if (request.Url.Path=="/android_asset/validate")
                {
                    // TODO Better,
                    // by inspecting the form entries from the view
                    Markdown = request.Url.GetQueryParameter("md");
                    update(Markdown);
                    return new WebResourceResponse("application/json", "utf-8" ,200, "Ok", null, null);
                }
            }
            return base.ShouldInterceptRequest(view, request);
        }
        
    }
}