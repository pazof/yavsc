using System;
using Android.Views;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;

namespace PostIt.Android.Services;

internal sealed class AndroidCapturePreview : Grid, IDisposable
{
    private readonly PreviewHost _host;
    private readonly TextBlock _status = new() { TextWrapping = Avalonia.Media.TextWrapping.Wrap };

    internal AndroidCapturePreview(AndroidCameraFrameRotator rotator)
    {
        RowDefinitions = new RowDefinitions("*,Auto");
        _host = new PreviewHost(rotator, ReportFailure);
        Children.Add(_host);
        SetRow(_status, 1);
        Children.Add(_status);
    }

    private void ReportFailure(Exception error)
    {
        global::Android.Util.Log.Error("PostIt.Podcast", $"Apercu indisponible : {error}");
        Dispatcher.UIThread.Post(() => _status.Text = $"Apercu indisponible : {error.Message}. La capture continue.");
    }

    public void Dispose() => _host.Dispose();

    private sealed class PreviewHost(AndroidCameraFrameRotator rotator, Action<Exception> failed)
        : NativeControlHost, IDisposable
    {
        private ISurfaceHolder? _holder;
        private SurfaceCallback? _callback;
        private bool _disposed;

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            var activity = MainActivity.Current
                ?? throw new InvalidOperationException("Activite Android indisponible pour l'apercu.");
            var view = new SurfaceView(activity);
            if (!_disposed)
            {
                _holder = view.Holder
                    ?? throw new InvalidOperationException("Surface Android indisponible pour l'apercu.");
                _callback = new SurfaceCallback(this);
                _holder.AddCallback(_callback);
            }
            return new AndroidViewControlHandle(view);
        }

        private void SetSurface(ISurfaceHolder holder, int width, int height)
        {
            if (_disposed) return;
            try { rotator.SetPreviewAsync(holder.Surface, width, height, failed).GetAwaiter().GetResult(); }
            catch (Exception ex) { failed(ex); }
        }

        private void Detach()
        {
            try { rotator.SetPreviewAsync(null).GetAwaiter().GetResult(); }
            catch (Exception ex) { failed(ex); }
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            ReleaseSurface();
            base.DestroyNativeControlCore(control);
        }

        private void ReleaseSurface()
        {
            Detach();
            if (_callback is not null)
            {
                // The native view wrapper may already be disposed during deferred host destruction.
                _holder?.RemoveCallback(_callback);
                _callback.Dispose();
                _callback = null;
            }
            _holder?.Dispose();
            _holder = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            ReleaseSurface();
        }

        private sealed class SurfaceCallback(PreviewHost owner) : Java.Lang.Object, ISurfaceHolderCallback
        {
            public void SurfaceCreated(ISurfaceHolder holder) { }
            public void SurfaceChanged(ISurfaceHolder holder, global::Android.Graphics.Format format, int width, int height)
                => owner.SetSurface(holder, width, height);
            public void SurfaceDestroyed(ISurfaceHolder holder) => owner.Detach();
        }
    }
}
