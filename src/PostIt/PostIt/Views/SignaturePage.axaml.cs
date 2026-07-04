using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using PostIt.Controls;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class SignaturePage : ContentPage
{
    private static readonly IBrush StrokeBrush = new SolidColorBrush(Color.FromRgb(0x10, 0x10, 0x10));
    private const double StrokeThickness = 2.0;
    private const double CoordinateMax = 10_000.0;

    private SignaturePageViewModel? _vm;
    private SignaturePadControl? _control;

    public SignaturePage()
    {
        InitializeComponent();

        // Wire the capture area: the Pad itself is the control, the
        // surrounding Border (PadFrame) is the hit-test region. We
        // set CaptureArea once the control's template has been
        // applied — for an inline control with no template, that
        // happens on first measure, which is guaranteed before
        // the user can interact, so attaching here is safe.
        _control = Pad;
        _control.CaptureArea = PadFrame;

        DataContextChanged += (_, _) => RebindViewModel(DataContext as SignaturePageViewModel);
    }

    private void RebindViewModel(SignaturePageViewModel? vm)
    {
        if (_vm is not null)
        {
            _vm.Detach();
            _control!.RedrawRequested -= OnRedrawRequested;
        }

        _vm = vm;

        if (_vm is null || _control is null) return;

        _vm.Attach(_control);
        _control.RedrawRequested += OnRedrawRequested;
        Repaint();
    }

    private void OnRedrawRequested(object? sender, EventArgs e) => Repaint();

    private void Repaint()
    {
        if (_control is null || InkLayer is null) return;

        InkLayer.Children.Clear();
        var w = PadFrame.Bounds.Width;
        var h = PadFrame.Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var strokes = _control.Strokes;
        int i = 0;
        while (i < strokes.Count)
        {
            int k = strokes[i];
            if (k <= 0) break;
            i++; // skip the length prefix

            var poly = new Polyline
            {
                Stroke = StrokeBrush,
                StrokeThickness = StrokeThickness,
                StrokeLineCap = PenLineCap.Round,
                StrokeJoin = PenLineJoin.Round,
            };
            var pts = new List<Point>(k);
            for (int p = 0; p < k; p++)
            {
                int nx = strokes[i++];
                int ny = strokes[i++];
                pts.Add(new Point(nx / CoordinateMax * w, ny / CoordinateMax * h));
            }
            poly.Points = pts;
            InkLayer.Children.Add(poly);
        }
    }
}
