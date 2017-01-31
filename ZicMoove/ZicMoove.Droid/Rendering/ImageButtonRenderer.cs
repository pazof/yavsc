// ***********************************************************************
// Assembly         : XLabs.Forms.Droid
// Author           : XLabs Team
// Created          : 12-27-2015
// 
// Last Modified By : XLabs Team
// Last Modified On : 01-04-2016
// ***********************************************************************
// <copyright file="ImageButtonRenderer.cs" company="XLabs Team">
//     Copyright (c) XLabs Team. All rights reserved.
// </copyright>
// <summary>
//       This project is licensed under the Apache 2.0 license
//       https://github.com/XLabs/Xamarin-Forms-Labs/blob/master/LICENSE
//       
//       XLabs is a open source project that aims to provide a powerfull and cross 
//       platform set of controls tailored to work with Xamarin Forms.
// </summary>
// ***********************************************************************
// 

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using XLabs.Enums;
using XLabs.Forms.Extensions;
using Color = Xamarin.Forms.Color;
using View = Android.Views.View;
using ZicMoove.Rendering;
using ZicMoove.Views;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.IO;
using ZicMoove.Helpers;
using System.Reflection;
using System.Threading;

[assembly: ExportRenderer(typeof(ImageButton), typeof(ImageButtonRenderer))]
namespace ZicMoove.Rendering
{
    /// <summary>
    /// Draws a button on the Android platform with the image shown in the right 
    /// position with the right size.
    /// </summary>
    public partial class ImageButtonRenderer : Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer
    {
        private static float _density = float.MinValue;


        /// <summary>
        /// Sets up the button including the image. 
        /// </summary>
        /// <param name="e">The event arguments.</param>
        private ImageButton ImageButton
        {
            get { return (ImageButton)Element; }
        }

        protected override async void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            _density = Resources.DisplayMetrics.Density;

            var targetButton = Control;
            if (targetButton != null) targetButton.SetOnTouchListener(TouchListener.Instance.Value);

            if (Element != null && Element.Font != Font.Default && targetButton != null) targetButton.Typeface = Element.Font.ToExtendedTypeface(Context);

            if (Element != null && ImageButton.Source != null) await SetImageSourceAsync(targetButton, ImageButton).ConfigureAwait(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose (bool disposing)
        {
            base.Dispose (disposing);
            if (disposing && Control != null) {
                Control.Dispose ();
            }
        }

        int imageWitdh = 0;
        int imageHeight = 0;
        int imageWitdhRequest = 0;
        int imageHeightRequest = 0;

        // this is called before OnElementChanged ...
        /*
        public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            switch (ImageButton.Orientation)
            {
                case ImageOrientation.ImageOnBottom:
                case ImageOrientation.ImageOnTop:
                    return base.GetDesiredSize(widthConstraint, heightConstraint+ imageHeightRequest);
                case ImageOrientation.ImageToRight:
                case ImageOrientation.ImageToLeft:
                    return base.GetDesiredSize(widthConstraint + imageWitdhRequest, heightConstraint);
                default:
                    return base.GetDesiredSize(widthConstraint, heightConstraint);
            }
        }*/

        /// <summary>
        /// Sets the image source.
        /// </summary>
        /// <param name="targetButton">The target button.</param>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="Task"/> for the awaited operation.</returns>
        private async Task SetImageSourceAsync(Android.Widget.Button targetButton, ImageButton model)
        {
            if (targetButton == null || targetButton.Handle == IntPtr.Zero || model == null) return;

            // const int Padding = 10;
            var source = model.IsEnabled ? model.Source : model.DisabledSource ?? model.Source;

            using (var bitmap = await GetBitmapAsync(source))
            {
                if (bitmap == null)
                    targetButton.SetCompoundDrawables(null, null, null, null);
                else
                {
                    var drawable = new BitmapDrawable(bitmap);
                    var tintColor = model.IsEnabled ? model.ImageTintColor : model.DisabledImageTintColor;
                    if (tintColor != Color.Transparent)
                    {
                        drawable.SetTint(tintColor.ToAndroid());
                        drawable.SetTintMode(PorterDuff.Mode.SrcIn);
                    }
                    imageWitdh = drawable.Bitmap.Width;
                    imageHeight = drawable.Bitmap.Height;
                    imageWitdhRequest = (int)model.ImageWidthRequest;
                    imageHeightRequest = (int)model.ImageHeightRequest;

                    if (imageHeightRequest <= 0) imageHeightRequest = imageHeight;
                    if (imageWitdhRequest <= 0) imageWitdhRequest = imageWitdh;

                    using (var scaledDrawable = GetScaleDrawable(drawable, imageWitdh, imageHeight))
                    {
                        Drawable left = null;
                        Drawable right = null;
                        Drawable top = null;
                        Drawable bottom = null;
                        int padding = 2; // model.Padding;
                        targetButton.CompoundDrawablePadding = RequestToPixels(padding);
                        targetButton.Gravity = GravityFlags.CenterHorizontal | GravityFlags.CenterVertical;
                        switch (model.Orientation)
                        {
                            case ImageOrientation.ImageToLeft:
                                left = scaledDrawable;
                                if (ImageButton.HeightRequest < imageHeightRequest)
                                    ImageButton.HeightRequest = imageHeightRequest;
                                break;
                            case ImageOrientation.ImageToRight:
                                right = scaledDrawable;
                                if (ImageButton.HeightRequest < imageHeightRequest)
                                    ImageButton.HeightRequest = imageHeightRequest;
                                break;
                            case ImageOrientation.ImageOnTop:
                                top = scaledDrawable;
                                if (ImageButton.WidthRequest < imageWitdhRequest)
                                    ImageButton.WidthRequest = imageWitdhRequest;
                                break;
                            case ImageOrientation.ImageOnBottom:
                                bottom = scaledDrawable;
                                if (ImageButton.WidthRequest < imageWitdhRequest)
                                    ImageButton.WidthRequest = imageWitdhRequest;
                                break;
                            case ImageOrientation.ImageCentered:
                                top = scaledDrawable;
                                if (ImageButton.HeightRequest < imageHeightRequest)
                                    ImageButton.HeightRequest = imageHeightRequest;
                                if (ImageButton.WidthRequest < imageWitdhRequest)
                                    ImageButton.WidthRequest = imageWitdhRequest;
                                break;
                        }

                        targetButton.SetCompoundDrawables(left, top, right, bottom);
                      //  this.MeasureChildren(model.ImageWidthRequest, model.ImageHeightRequest);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Bitmap"/> for the supplied <see cref="ImageSource"/>.
        /// </summary>
        /// <param name="source">The <see cref="ImageSource"/> to get the image for.</param>
        /// <returns>A loaded <see cref="Bitmap"/>.</returns>
        private async Task<Bitmap> GetBitmapAsync(ImageSource imagesource)

        {
            var uriImageLoader = imagesource as UriImageSource;
            if (uriImageLoader != null && uriImageLoader.Uri != null)
            {
                using (var client = UserHelpers.CreateJsonClient())
                {
                    using (var response = await client.GetAsync(uriImageLoader.Uri))
                    {
                        var data = await response.Content.ReadAsByteArrayAsync();
                        return await BitmapFactory.DecodeByteArrayAsync(data, 0, data.Length);
                    }
                }
            }

            var resImageLoader = imagesource as StreamImageSource;
            if (resImageLoader != null && resImageLoader.Stream != null)
            {
                    return await BitmapFactory.DecodeStreamAsync(await resImageLoader.Stream(CancellationToken.None));
            }
            return null;
        }

        /// <summary>
        /// Called when the underlying model's properties are changed.
        /// </summary>
        /// <param name="sender">The Model used.</param>
        /// <param name="e">The event arguments.</param>
        protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == ImageButton.SourceProperty.PropertyName ||
                e.PropertyName == ImageButton.DisabledSourceProperty.PropertyName ||
                e.PropertyName == VisualElement.IsEnabledProperty.PropertyName ||
                e.PropertyName == ImageButton.ImageTintColorProperty.PropertyName ||
                e.PropertyName == ImageButton.DisabledImageTintColorProperty.PropertyName)
            {
                await SetImageSourceAsync(Control, ImageButton).ConfigureAwait(false);
            }
            base.OnElementPropertyChanged(sender, e);
        }

        /// <summary>
        /// Returns a <see cref="Drawable"/> with the correct dimensions from an 
        /// Android resource id.
        /// </summary>
        /// <param name="drawable">An android <see cref="Drawable"/>.</param>
        /// <param name="width">The width to scale to.</param>
        /// <param name="height">The height to scale to.</param>
        /// <returns>A scaled <see cref="Drawable"/>.</returns>
        private Drawable GetScaleDrawable(Drawable drawable, int width, int height)
        {
            var returnValue = new ScaleDrawable(drawable, 0, 100, 100).Drawable;

            returnValue.SetBounds(0, 0, RequestToPixels(width), RequestToPixels(height));

            return returnValue;
        }

        /// <summary>
        /// Returns a drawable dimension modified according to the current display DPI.
        /// </summary>
        /// <param name="sizeRequest">The requested size in relative units.</param>
        /// <returns>Size in pixels.</returns>
        public int RequestToPixels(int sizeRequest)
        {
            if (_density == float.MinValue)
            {
                if (Resources.Handle == IntPtr.Zero || Resources.DisplayMetrics.Handle == IntPtr.Zero)
                    _density = 1.0f;
                else
                    _density = Resources.DisplayMetrics.Density;
            }

            return (int)(sizeRequest * _density);
        }
    }

    //Hot fix for the layout positioning issue on Android as described in http://forums.xamarin.com/discussion/20608/fix-for-button-layout-bug-on-android
    class TouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        public static readonly Lazy<TouchListener> Instance = new Lazy<TouchListener>(() => new TouchListener());

        /// <summary>
        /// Make TouchListener a singleton.
        /// </summary>
        private TouchListener()
        { }

        public bool OnTouch(View v, MotionEvent e)
        {
            var buttonRenderer = v.Tag as Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer;
            if (buttonRenderer != null && e.Action == MotionEventActions.Down) buttonRenderer.Control.Text = buttonRenderer.Element.Text;

            return false;
        }
    }

}
