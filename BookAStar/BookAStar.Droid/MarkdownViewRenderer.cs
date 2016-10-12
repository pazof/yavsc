
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
    public class JsBridgeMarkdown : Java.Lang.Object
    {
        readonly WeakReference<MarkdownViewRenderer> hybridWebViewRenderer;

        public JsBridgeMarkdown(MarkdownViewRenderer hybridRenderer)
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
                hybridRenderer.Element.Markdown = data;
                MarkdownViewRenderer.AdjustHeightRequest(hybridRenderer.Element,
                    hybridRenderer.EditorView);
            }
        }


    }

    public class MarkdownViewRenderer : ViewRenderer<MarkdownView, WebView>
    {
        private WebView editorView;
        private MarkdownEditor editorTemplate = new MarkdownEditor();
        private MarkdownDeep.Markdown markdown = new MarkdownDeep.Markdown();
        const string JavaScriptFunction = "function invokeCSharpAction(data){jsBridge.invokeAction(data);}";

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
           
            xview.BatchBegin();
            var vch = view.ContentHeight; // FIXME why not 3?
            xview.HeightRequest = vch > xview.MinimumHeightRequest ? vch : xview.MinimumHeightRequest;
            xview.BatchCommit();
        }

        private void SetMDEditorText(string text)
        {
            editorTemplate.Model = (text == null) ? null : markdown.Transform(text);
            var html = editorTemplate.GenerateString();
            EditorView.LoadDataWithBaseURL("file:///android_asset/",
            html, "text/html", "utf-8", null);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<MarkdownView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                SetNativeControl(CreateNativeControl());
                InjectJS(JavaScriptFunction);
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
                EditorView.SetWebViewClient(viewclient);
                Control.AddJavascriptInterface(new JsBridgeMarkdown(this), "jsBridge");
                SetMDEditorText(e.NewElement.Markdown);
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
            
            //  editorView.SetMinimumHeight(300);
            return EditorView;
        }
        // FIXME no impact...
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            MeasureSpecMode widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            MeasureSpecMode heightMode = MeasureSpec.GetMode(heightMeasureSpec);
            int widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            int heightSize = MeasureSpec.GetSize(heightMeasureSpec);
            int pxHeight = (int)ContextExtensions.ToPixels(Context, Element.HeightRequest);
            int pxWidth = (int)ContextExtensions.ToPixels(Context, Element.WidthRequest);
            var measuredWidth = widthMode != MeasureSpecMode.Exactly ? (widthMode != MeasureSpecMode.AtMost ? pxHeight : Math.Min(pxHeight, widthSize)) : widthSize;
            var measuredHeight = heightMode != MeasureSpecMode.Exactly ? (heightMode != MeasureSpecMode.AtMost ? pxWidth : Math.Min(pxWidth, heightSize)) : heightSize;
            SetMeasuredDimension(measuredWidth, measuredHeight< Element.HeightRequest ? (int) Element.HeightRequest : measuredHeight);
        }
        /*
        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            Element.Layout(new Xamarin.Forms.Rectangle(0, 0, ContextExtensions.FromPixels(Context, right - left), ContextExtensions.FromPixels(Context, bottom - top)));
            base.OnLayout(changed, left, top, right, bottom);
        }
        */
    }
}