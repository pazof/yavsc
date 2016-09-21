
using BookAStar.Views;
using Android.Webkit;
using Xamarin.Forms.Platform.Android;
using BookAStar.Droid;
using System;
using Java.Interop;

[assembly: Xamarin.Forms.ExportRenderer(typeof(MarkdownView), typeof(MarkdownViewRenderer))]
namespace BookAStar.Droid
{
    public class JSBridge : Java.Lang.Object
    {
        readonly WeakReference<MarkdownViewRenderer> hybridWebViewRenderer;

        public JSBridge(MarkdownViewRenderer hybridRenderer)
        {
            hybridWebViewRenderer = new WeakReference<MarkdownViewRenderer>(hybridRenderer);
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data)
        {
            MarkdownViewRenderer hybridRenderer;

            if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget(out hybridRenderer))
            {
                hybridRenderer.Element.Markdown=data;
            }
        }
    }
    public class MarkdownViewRenderer : ViewRenderer<MarkdownView, WebView>
    {
        private WebView editorView;
        private MarkdownEditor editorTemplate = new MarkdownEditor();
        private MarkdownDeep.Markdown markdown = new MarkdownDeep.Markdown();
        const string JavaScriptFunction = "function invokeCSharpAction(data){jsBridge.invokeAction(data);}";

        private void SetMDEditorText(string text)
        {
            editorTemplate.Model = (text == null) ? null : markdown.Transform(text);
            var html = editorTemplate.GenerateString();
            editorView.LoadDataWithBaseURL("file:///android_asset/",
            html, "text/html", "utf-8", null);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<MarkdownView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                editorView = new WebView(Context);
                editorView.Settings.BuiltInZoomControls = true;
                editorView.Settings.JavaScriptEnabled = true;
                editorView.Settings.LoadsImagesAutomatically = true;
                editorView.Settings.SetAppCacheEnabled(true);
                editorView.Settings.AllowContentAccess = true;
                editorView.Settings.AllowFileAccess = true;
                editorView.Settings.AllowFileAccessFromFileURLs = true;
                editorView.Settings.AllowUniversalAccessFromFileURLs = true;
                editorView.Settings.BlockNetworkImage = false;
                editorView.Settings.BlockNetworkLoads = false;
                editorView.Settings.DomStorageEnabled = true;
              //  editorView.SetMinimumHeight(300);
                SetNativeControl(editorView);
            }
            if (e.OldElement != null)
            {
                // Unsubscribe
            }
            if (e.NewElement != null)
            {
                // Subscribe
                var viewclient = new MarkdownWebViewClient(
                md => { e.NewElement.Markdown = md; });
                editorView.SetWebViewClient(viewclient);
                Control.AddJavascriptInterface(new JSBridge(this), "jsBridge");
                SetMDEditorText(e.NewElement.Markdown);
                InjectJS(JavaScriptFunction);
            }
        }
        

        void InjectJS(string script)
        {
            if (Control != null)
            {
                Control.LoadUrl(string.Format("javascript: {0}", script));
            }
        }

    }
}