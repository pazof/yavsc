using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using PostIt.Models;

namespace PostIt.Controls;

/// <summary>
/// Reusable signature-capture surface: wraps the render-agnostic
/// <see cref="SignaturePadControl"/> in a sized <see cref="Border"/>
/// (the hit-test region) with an overlay <see cref="Canvas"/> that
/// the code-behind repaints from the control's stroke buffer. Owns
/// the ink-rendering logic so every page that captures a signature
/// (the dev-only <c>SignaturePage</c>, the estimate validation page,
/// ...) shares one implementation.
///
/// Hosts bind <see cref="CaptureWidth"/>/<see cref="CaptureHeight"/>
/// (DIPs, defaults 600×200) to size the surface. View models attach
/// to the inner <see cref="SignaturePad"/> control (exposed here) to
/// read snapshots, drive <c>Clear</c>, and subscribe to
/// <see cref="SignaturePadControl.StrokeCompleted"/> /
/// <see cref="SignaturePadControl.RedrawRequested"/> — the same
/// contract the headless tests use, since a <see cref="UserControl"/>
/// needs its XAML loaded to be drivable.
/// </summary>
public partial class SignaturePadView : UserControl
{
    private static readonly IBrush StrokeBrush = new SolidColorBrush(Color.FromRgb(0x10, 0x10, 0x10));
    private const double StrokeThickness = 2.0;
    private const double CoordinateMax = 10_000.0;

    public static readonly StyledProperty<double> CaptureWidthProperty =
        AvaloniaProperty.Register<SignaturePadView, double>(nameof(CaptureWidth), 600);

    public static readonly StyledProperty<double> CaptureHeightProperty =
        AvaloniaProperty.Register<SignaturePadView, double>(nameof(CaptureHeight), 200);

    public double CaptureWidth
    {
        get => GetValue(CaptureWidthProperty);
        set => SetValue(CaptureWidthProperty, value);
    }

    public double CaptureHeight
    {
        get => GetValue(CaptureHeightProperty);
        set => SetValue(CaptureHeightProperty, value);
    }

    /// <summary>
    /// The underlying capture control. View models attach to this to
    /// read <see cref="SignaturePadControl.Snapshot"/> / call
    /// <see cref="SignaturePadControl.Clear"/> and subscribe to its
    /// events; this view repaints the ink layer from the same buffer.
    /// </summary>
    public SignaturePadControl SignaturePad => InnerPad;

    public SignaturePadView()
    {
        InitializeComponent();

        // Wire the capture area: the Pad is the control, the
        // surrounding Border (PadFrame) is the hit-test region. For an
        // inline control with no template, capture wiring happens on
        // first measure, which is guaranteed before the user can
        // interact, so attaching here is safe.
        InnerPad.CaptureArea = PadFrame;
        ApplyFrameSize();

        InnerPad.RedrawRequested += OnPadRedrawRequested;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CaptureWidthProperty || change.Property == CaptureHeightProperty)
        {
            ApplyFrameSize();
            Repaint();
        }
    }

    private void ApplyFrameSize()
    {
        if (PadFrame is null) return;
        PadFrame.Width = Math.Max(1, CaptureWidth);
        PadFrame.Height = Math.Max(1, CaptureHeight);
    }

    private void OnPadRedrawRequested(object? sender, EventArgs e) => Repaint();

    /// <summary>Defensive copy of the current buffer.</summary>
    public SignaturePadData Snapshot() => InnerPad.Snapshot();

    /// <summary>Forget every captured stroke.</summary>
    public void Clear() => InnerPad.Clear();

    /// <summary>Wire-format strokes, forwarded from the inner control.</summary>
    public IReadOnlyList<int> Strokes => InnerPad.Strokes;

    private void Repaint()
    {
        if (InkLayer is null) return;

        InkLayer.Children.Clear();
        var w = PadFrame.Bounds.Width;
        var h = PadFrame.Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var pending = InnerPad.PendingStroke;
        var strokes = InnerPad.Strokes;
        int sealedCount = strokes.Count - pending.Count;
        if (sealedCount < 0)
        {
            sealedCount = 0;
        }

        int i = 0;
        while (i < sealedCount)
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

        if (pending.Count > 0)
        {
            var poly = new Polyline
            {
                Stroke = StrokeBrush,
                StrokeThickness = StrokeThickness,
                StrokeLineCap = PenLineCap.Round,
                StrokeJoin = PenLineJoin.Round,
            };

            var pts = new List<Point>(pending.Count / 2);
            for (int p = 0; p < pending.Count; p += 2)
            {
                int nx = pending[p];
                int ny = pending[p + 1];
                pts.Add(new Point(nx / CoordinateMax * w, ny / CoordinateMax * h));
            }

            poly.Points = pts;
            InkLayer.Children.Add(poly);
        }
    }
}