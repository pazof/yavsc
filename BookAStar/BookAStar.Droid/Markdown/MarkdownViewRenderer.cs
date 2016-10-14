
using BookAStar.Views;
using Android.Webkit;
using Xamarin.Forms.Platform.Android;
using BookAStar.Droid;
using System;
using Java.Interop;
using System.ComponentModel;
using Android.Views;

[assembly: Xamarin.Forms.ExportRenderer(typeof(MarkdownView), typeof(MarkdownViewRenderer))]
namespace BookAStar.Droid
{
    using Markdown;
    public class MarkdownViewRenderer : ViewRenderer<MarkdownView, WebView>
    {
        private WebView editorView;
        private MarkdownEditor editorTemplate = new MarkdownEditor();
        const string jsLoadedJavaScriptFunction = "function jsLoaded(){jsBridge.jsLoaded()}";
        const string contentEditedJavaScriptFunction = "function contentEdited(data){jsBridge.contentEdited(data)}";
        public WebView EditorView
        {
            get
            {
                return editorView;
            }
        }

        /// <summary>
        /// To be called once document finished loading
        /// </summary>
        /// <param name="xview"></param>
        /// <param name="view"></param>
        public static async void AdjustHeightRequest(MarkdownView xview, WebView view)
        {
            if (view == null || xview == null) return;
            var vch = view.ContentHeight;
            xview.HeightRequest = vch > xview.MinimumHeightRequest ? vch : xview.MinimumHeightRequest;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<MarkdownView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                SetNativeControl(CreateNativeControl());
                InjectJS(jsLoadedJavaScriptFunction);
                InjectJS(contentEditedJavaScriptFunction);
            }
            if (e.OldElement != null)
            {
                // Unsubscribe
            }
            if (e.NewElement != null)
            {
                // Subscribe
                editorTemplate.Model = new Markdown.MarkdownViewModel
                { Content = e.NewElement.Markdown, Editable = e.NewElement.Editable };
                var html = editorTemplate.GenerateString();
                EditorView.LoadDataWithBaseURL("file:///android_asset/",
                html, "text/html", "utf-8", null);
            }
        }

        void InjectJS(string script)
        {
            if (Control != null)
            {
                Control.LoadUrl(string.Format("javascript: {0}", script));
            }
        }

        private WebView CreateNativeControl()
        {
            editorView = new WebView(Context);
            EditorView.SetWebChromeClient(
                new MarkdownWebChromeClient()
                );
            EditorView.Settings.BuiltInZoomControls = false;
            EditorView.Settings.JavaScriptEnabled = true;
            EditorView.Settings.LoadsImagesAutomatically = true;
            EditorView.Settings.SetAppCacheEnabled(true);
            EditorView.Settings.AllowContentAccess = true;
            EditorView.Settings.AllowFileAccess = true;
            EditorView.Settings.AllowFileAccessFromFileURLs = true;
            EditorView.Settings.AllowUniversalAccessFromFileURLs = true;
            EditorView.Settings.BlockNetworkImage = false;
            EditorView.Settings.BlockNetworkLoads = false;
            EditorView.Settings.DomStorageEnabled = true;
            EditorView.AddJavascriptInterface(new JsBridgeMarkdown(this), "jsBridge");
            EditorView.ViewTreeObserver.PreDraw += ViewTreeObserver_PreDraw;
            return EditorView;
        }

        private void ViewTreeObserver_PreDraw(object sender, ViewTreeObserver.PreDrawEventArgs e)
        {
            AdjustHeightRequest(Element, Control);
        }
    }
}