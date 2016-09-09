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
    class MarkdownRazorWebViewClient : WebViewClient
    {
        Context context;
        public MarkdownRazorWebViewClient(Context context)
        {
            this.context = context;
        }

        
        private WebResourceResponse getWebResourceResponseFromAssets(string name)
        {
            var desc =
                getActivity().Assets.OpenFd(name);
            var stream = desc.CreateInputStream();
            string encoding = null;
            string mimet = "text/html";
            if (name.EndsWith(".css"))
            {
                mimet = "text/css";
                encoding = "utf-8";
            }
            else if (name.EndsWith(".js"))
            { mimet = "text/js";
                encoding = "utf-8";
            }
            else if (name.EndsWith(".ico"))
            { mimet = "image/ico";
                encoding = "utf-8";
            }

            return new WebResourceResponse(mimet, encoding, stream );

        }
        private static Activity getActivity ()
        {
            return (Activity)App.PlateformSpecificInstance;
        }
        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {

            if (request.Url.Scheme=="file")
            {
                return getWebResourceResponseFromAssets(request.Url.Path);
            }
            if (request.Url.Scheme=="hybrid")
            {
               getActivity().RunOnUiThread(
              () => testGetContent(view));
            }
            return base.ShouldInterceptRequest(view, request);
        }

        class ContentCallBack : Java.Lang.Object, IValueCallback
        {
            public string Result { get; private set; }
            public void OnReceiveValue(Java.Lang.Object value)
            {
                if (value == null) { Result = null; }
                else { Result = new string(((Java.Lang.String)value).ToCharArray()); }
            }
        }

        void testGetContent(WebView view)
        {
            var cb = new ContentCallBack();
            view.EvaluateJavascript("$('#Content').val()", cb);
        }

    }
}