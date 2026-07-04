using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PostIt.Models;

namespace PostIt.Controls;

/// <summary>
/// Pointer-driven capture surface that records a signature as a list
/// of strokes, each stroke being a length-prefixed sequence of (x, y)
/// coordinates normalised to <c>[0, CoordinateMax]</c>.
///
/// The control is render-agnostic: it does not draw anything. The
/// host view templates a <see cref="InputElement"/> (typically a
/// <c>Border</c>) as <c>PART_CaptureArea</c> for pointer capture,
/// and binds a separate visual layer (e.g. a <c>Canvas</c>) to
/// <see cref="Strokes"/> for redraw. Keeping the control headless of
/// rendering makes it usable from a headless test where no
/// composition happens.
///
/// Wire format (see <see cref="SignaturePadData"/>):
/// <code>int[] = [k0, x0, y0, ..., k1, x0, y0, ...]</code>
/// with <c>x, y ∈ [0, 10_000]</c>.
///
/// Threading: pointer events are dispatched on the UI thread, which
/// is the only thread that ever mutates <see cref="Strokes"/>. The
/// buffer is safe to read from any thread as long as no read
/// straddles a pointer event — for cross-thread transfer use
/// <see cref="Snapshot"/>, which copies.
/// </summary>
public class SignaturePadControl : TemplatedControl
{
    /// <summary>
    /// Styled property pointing at the <see cref="InputElement"/>
    /// that receives pointer events. Set it in the control's
    /// template (<c>PART_CaptureArea</c>).
    /// </summary>
    public static readonly StyledProperty<InputElement?> CaptureAreaProperty =
        AvaloniaProperty.Register<SignaturePadControl, InputElement?>(nameof(CaptureArea));

    public InputElement? CaptureArea
    {
        get => GetValue(CaptureAreaProperty);
        set => SetValue(CaptureAreaProperty, value);
    }

    /// <summary>
    /// Captured strokes in wire form. Exposed as a read-only view
    /// over the internal buffer. The buffer only mutates on the UI
    /// thread, between pointer events.
    /// </summary>
    public IReadOnlyList<int> Strokes => _strokes;

    /// <summary>
    /// Raised when the user finishes a stroke (pointer release).
    /// The argument is a snapshot of the buffer at release time.
    /// </summary>
    public event EventHandler<SignaturePadData>? StrokeCompleted;

    /// <summary>
    /// Raised when the buffer changes: at the end of every stroke
    /// and on <see cref="Clear"/>. Mid-stroke points do not raise
    /// this event (pointer-move is too dense); bind a separate
    /// visual layer if you need a live preview.
    /// </summary>
    public event EventHandler? RedrawRequested;

    private readonly List<int> _strokes = new(capacity: 256);
    private int _pendingPoints; // number of (x, y) pairs awaiting a length prefix
    private bool _capturing;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (CaptureArea is { } previous)
        {
            previous.PointerPressed -= OnCapturePressed;
            previous.PointerMoved -= OnCaptureMoved;
            previous.PointerReleased -= OnCaptureReleased;
        }

        if (CaptureArea is { } area)
        {
            area.PointerPressed += OnCapturePressed;
            area.PointerMoved += OnCaptureMoved;
            area.PointerReleased += OnCaptureReleased;
        }
    }

    private void OnCapturePressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(CaptureArea).Properties.IsLeftButtonPressed) return;
        e.Pointer.Capture(CaptureArea);
        _capturing = true;
        _pendingPoints = 0;
        AppendPoint(e.GetPosition(CaptureArea));
    }

    private void OnCaptureMoved(object? sender, PointerEventArgs e)
    {
        if (!_capturing) return;
        AppendPoint(e.GetPosition(CaptureArea));
    }

    private void OnCaptureReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_capturing) return;
        AppendPoint(e.GetPosition(CaptureArea));
        _capturing = false;

        if (_pendingPoints == 0)
        {
            // Press + immediate release without movement yields no
            // point at all (the press fired AppendPoint, so this
            // branch is unreachable — kept for clarity if a future
            // change skips the press append).
            return;
        }

        // Seal the current stroke by inserting its length at the
        // head of its slice. The slice is the trailing
        // 2 * _pendingPoints entries.
        int sliceStart = _strokes.Count - 2 * _pendingPoints;
        _strokes.Insert(sliceStart, _pendingPoints);
        _pendingPoints = 0;

        StrokeCompleted?.Invoke(this, Snapshot());
        RedrawRequested?.Invoke(this, EventArgs.Empty);
    }

    private void AppendPoint(Point p)
    {
        var (nx, ny) = Normalise(p);
        _strokes.Add(nx);
        _strokes.Add(ny);
        _pendingPoints++;
    }

    private (int x, int y) Normalise(Point p)
    {
        if (CaptureArea is null) return (0, 0);
        var bounds = CaptureArea.Bounds;
        double w = bounds.Width;
        double h = bounds.Height;
        if (w <= 0 || h <= 0) return (0, 0);
        int nx = (int)Math.Round(Math.Clamp(p.X / w, 0.0, 1.0) * SignaturePadData.CoordinateMax);
        int ny = (int)Math.Round(Math.Clamp(p.Y / h, 0.0, 1.0) * SignaturePadData.CoordinateMax);
        return (nx, ny);
    }

    /// <summary>
    /// Forget every captured stroke. Raises <see cref="RedrawRequested"/>.
    /// </summary>
    public void Clear()
    {
        _strokes.Clear();
        _pendingPoints = 0;
        _capturing = false;
        RedrawRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Defensive copy of the current buffer wrapped in a
    /// <see cref="SignaturePadData"/>. Cheap; call only when the
    /// view needs to ship the data off (e.g. to a backend).
    /// </summary>
    public SignaturePadData Snapshot() => new(_strokes.ToArray());

    // --- Test-only surface (visible to PostIt.Tests) -------------------

    /// <summary>
    /// Test hook: append a single normalised point without going
    /// through the pointer pipeline. Does not raise
    /// <see cref="RedrawRequested"/>.
    /// </summary>
    internal void AppendPointForTest(int x, int y)
    {
        _strokes.Add(x);
        _strokes.Add(y);
        _pendingPoints++;
    }

    /// <summary>
    /// Test hook: seal the currently-pending stroke with a length
    /// prefix. Mirrors what <see cref="OnCaptureReleased"/> does at
    /// pointer release time, including the
    /// <see cref="StrokeCompleted"/> and <see cref="RedrawRequested"/>
    /// events, so test scenarios observe the same notification
    /// contract as production. Idempotent: a second call without
    /// intermediate appends is a no-op.
    /// </summary>
    internal void SealStrokeForTest()
    {
        if (_pendingPoints == 0) return;
        int sliceStart = _strokes.Count - 2 * _pendingPoints;
        _strokes.Insert(sliceStart, _pendingPoints);
        _pendingPoints = 0;
        StrokeCompleted?.Invoke(this, Snapshot());
        RedrawRequested?.Invoke(this, EventArgs.Empty);
    }
}
