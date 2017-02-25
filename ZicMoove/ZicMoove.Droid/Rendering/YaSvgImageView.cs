using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace ZicMoove.Droid.Rendering
{
    public class YaSvgImageView : ImageView
    {
        Bitmap currentBitmap;
        public YaSvgImageView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }
        public YaSvgImageView(Context context) : base(context)
        {
            Initialize();
        }
        public YaSvgImageView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
        }
        XamSvg.Svg svg;
        XamSvg.SvgPictureDrawable drawable;

        public void SetSvg(XamSvg.Svg svg)
        {
            this.svg = svg;
            this.drawable = XamSvg.SvgFactory.GetDrawable(svg, XamSvg.Shared.Cross.SvgFillMode.Fill);
            this.SetImageDrawable(drawable);
        }
    }
}