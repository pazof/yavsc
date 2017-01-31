
using ZicMoove.Views;
using Android.Webkit;
using Xamarin.Forms.Platform.Android;
using ZicMoove.Droid;
using System;
using Java.Interop;
using System.ComponentModel;
using Android.Views;

[assembly: Xamarin.Forms.ExportRenderer(typeof(MarkdownView), typeof(MarkdownViewRenderer))]
namespace ZicMoove.Droid
{
    using Markdown;
    using XLabs.Forms;
    using XLabs.Ioc;
    using XLabs.Platform.Mvvm;
    using static View;

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
       //   var oldH = xview.Height;
            var newH  = vch > xview.MinimumHeightRequest ? vch : xview.MinimumHeightRequest;
            xview.HeightRequest = newH;
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
                {
                    Content = e.NewElement.Markdown, Editable = e.NewElement.Editable
                };
                var html = editorTemplate.GenerateString();
                EditorView.LoadDataWithBaseURL("file:///android_asset/",
                html, "text/html", "utf-8", null);
                EditorView.SetBackgroundColor(e.NewElement.BackgroundColor.ToAndroid());
                
            }
        }

        void InjectJS(string script)
        {
            if (Control != null)
            {
                Control.LoadUrl(string.Format("javascript: {0}", script));
            }
        }
        MDContextMenu contextMenu;
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
            //var app = Resolver.Resolve<IXFormsApp>() as IXFormsApp<XFormsCompatApplicationDroid>;
            //contextMenu = new MDContextMenu(app.AppContext);
            //EditorView.SetOnCreateContextMenuListener(contextMenu);
            
            return EditorView;
        }

        private void EditorView_Touch(object sender, TouchEventArgs e)
        {
            
        }

        private void ViewTreeObserver_PreDraw(object sender, ViewTreeObserver.PreDrawEventArgs e)
        {
            AdjustHeightRequest(Element, Control);
        }
    }
}