using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Xamarin.Forms;
using ZicMoove.Views;
using XamSvg;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SvgImage), typeof(ZicMoove.Droid.Rendering.SvgRenderer))]
namespace ZicMoove.Droid.Rendering
{
    class SvgRenderer : Xamarin.Forms.Platform.Android.AppCompat.ViewRenderer<SvgImage, ImageView>
    {
        private ImageView view;
        private ImageView CreateNativeControl()
        {
            view = new ImageView(Context);
            view.LayoutParameters = new Gallery.LayoutParams(LayoutParams.WrapContent, LayoutParams.FillParent);
            
            return view;
        }
        protected override void OnElementChanged(ElementChangedEventArgs<SvgImage> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                // Init
                SetNativeControl(CreateNativeControl());
            }
            if (e.OldElement != null)
            {
                // Unsubscribe
            }
            if (e.NewElement != null) if (e.NewElement.Svg != null)
                {
                // Subscribe
                
                if (!e.NewElement.Svg.EndsWith(".svg"))
                    throw new NotSupportedException("Source must end width '.svg'");
                var fi = new System.IO.FileInfo(e.NewElement.Svg);
                if (fi.Exists)
                {
                    using (var stream = fi.OpenRead())
                    {
                        var svg = SvgFactory.GetSvg(System.Threading.CancellationToken.None,
                                     stream);
                       var drawable = XamSvg.SvgFactory.GetDrawable(svg, XamSvg.Shared.Cross.SvgFillMode.Fill);
                        view.SetImageDrawable(drawable);
                    }
                }
             
            }
        }
    }
}