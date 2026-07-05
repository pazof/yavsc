using System;
using System.Linq;
using PostIt.Controls;
using PostIt.Models;
using Xunit;

namespace PostIt.Tests;

/// <summary>
/// Targeted tests for <see cref="SignaturePadControl"/> and
/// <see cref="SignaturePadData"/>.
///
/// The control exposes <c>internal</c> test hooks so we can drive
/// the buffer without standing up a headless XAML tree just to
/// deliver synthetic pointer events. The headless surface is used
/// only to assert that the control's pointer handlers are wired
/// when a template is applied; see
/// <see cref="Pointer_handlers_attach_when_capture_area_is_set"/>.
/// </summary>
public class SignaturePadControlTests
{
    // --- SignaturePadData (pure) ---------------------------------------

    [Fact]
    public void Data_empty_array_is_empty()
    {
        var d = new SignaturePadData(Array.Empty<int>());
        Assert.True(d.IsEmpty);
        Assert.Equal(0, d.StrokeCount);
    }

    [Fact]
    public void Data_single_dot_is_one_stroke_with_k_equals_one()
    {
        var d = new SignaturePadData(new[] { 1, 5_000, 5_000 });
        Assert.False(d.IsEmpty);
        Assert.Equal(1, d.StrokeCount);
    }

    [Fact]
    public void Data_two_strokes_are_independent()
    {
        var d = new SignaturePadData(new[]
        {
            2, 100, 100, 200, 200,
            1, 9_000, 9_000,
        });
        Assert.Equal(2, d.StrokeCount);
    }

    [Fact]
    public void Data_malformed_payload_does_not_throw_on_read()
    {
        // k=0 at the head would underflow the walker. The reader
        // short-circuits instead of throwing.
        var d = new SignaturePadData(new[] { 0, 1, 2, 3 });
        Assert.Equal(0, d.StrokeCount);
    }

    [Fact]
    public void Data_constructor_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => new SignaturePadData(null!));
    }

    // --- SignaturePadControl (buffer / events) -------------------------

    [Fact]
    public void New_control_has_empty_buffer()
    {
        var pad = new SignaturePadControl();
        Assert.Empty(pad.Strokes);
        Assert.True(pad.Snapshot().IsEmpty);
    }

    [Fact]
    public void Snapshot_returns_a_distinct_array_each_call()
    {
        var pad = new SignaturePadControl();
        pad.AppendPointForTest(1_000, 2_000);
        pad.AppendPointForTest(3_000, 4_000);
        pad.SealStrokeForTest();

        var first = pad.Snapshot();
        var second = pad.Snapshot();

        // Distinct array instances — the consumer of the first
        // snapshot can hold onto it after the control mutates.
        Assert.NotSame(first.Strokes, second.Strokes);
        // Same logical content (no mutation in between).
        Assert.Equal(first.Strokes, second.Strokes);

        pad.AppendPointForTest(5_000, 6_000);
        pad.SealStrokeForTest();

        var third = pad.Snapshot();
        Assert.NotEqual(first.Strokes, third.Strokes);
    }

    [Fact]
    public void Clear_empties_buffer_and_raises_redraw()
    {
        var pad = new SignaturePadControl();
        pad.AppendPointForTest(1, 1);
        pad.SealStrokeForTest();
        Assert.NotEmpty(pad.Strokes);

        int redraws = 0;
        pad.RedrawRequested += (_, _) => redraws++;
        pad.Clear();

        Assert.Empty(pad.Strokes);
        Assert.True(pad.Snapshot().IsEmpty);
        Assert.Equal(1, redraws);
    }

    [Fact]
    public void SealStrokeForTest_raises_redraw()
    {
        var pad = new SignaturePadControl();
        int redraws = 0;
        pad.RedrawRequested += (_, _) => redraws++;
        pad.AppendPointForTest(1, 1);
        pad.AppendPointForTest(2, 2);
        pad.SealStrokeForTest();
        Assert.Equal(1, redraws);
    }

    [Fact]
    public void SealStrokeForTest_with_no_pending_points_is_a_no_op()
    {
        var pad = new SignaturePadControl();
        int redraws = 0;
        pad.RedrawRequested += (_, _) => redraws++;
        pad.SealStrokeForTest();
        Assert.Equal(0, redraws);
    }

    [Fact]
    public void Two_sealed_strokes_produce_two_length_prefixes()
    {
        var pad = new SignaturePadControl();
        // Stroke 0: one point.
        pad.AppendPointForTest(1_000, 1_000);
        pad.SealStrokeForTest();
        // Stroke 1: two points.
        pad.AppendPointForTest(2_000, 2_000);
        pad.AppendPointForTest(3_000, 3_000);
        pad.SealStrokeForTest();

        var s = pad.Strokes;
        // Layout: [k0, x0, y0, k1, x1, y1, x2, y2]
        Assert.Equal(1, s[0]);
        Assert.Equal(1_000, s[1]);
        Assert.Equal(1_000, s[2]);
        Assert.Equal(2, s[3]);
        Assert.Equal(2_000, s[4]);
        Assert.Equal(2_000, s[5]);
        Assert.Equal(3_000, s[6]);
        Assert.Equal(3_000, s[7]);
    }

    [Fact]
    public void StrokeCompleted_fires_on_seal()
    {
        var pad = new SignaturePadControl();
        int events = 0;
        pad.StrokeCompleted += (_, _) => events++;
        pad.AppendPointForTest(1, 1);
        pad.SealStrokeForTest();
        pad.AppendPointForTest(2, 2);
        pad.SealStrokeForTest();
        Assert.Equal(2, events);
    }

    [Fact]
    public void StrokeCompleted_carries_a_snapshot_with_k_count()
    {
        var pad = new SignaturePadControl();
        SignaturePadData? captured = null;
        pad.StrokeCompleted += (_, d) => captured = d;
        pad.AppendPointForTest(1, 1);
        pad.AppendPointForTest(2, 2);
        pad.SealStrokeForTest();
        Assert.NotNull(captured);
        Assert.Equal(1, captured!.StrokeCount);
    }
}
