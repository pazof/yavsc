using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using PostIt.Controls;
using PostIt.ViewModels;
using Xunit;

namespace PostIt.Tests;

/// <summary>
/// Tests for <see cref="SignaturePageViewModel"/>: the contract
/// between the page's view model and the <see cref="SignaturePadControl"/>.
/// The view (XAML + code-behind rendering) is not tested here — the
/// control is render-agnostic, and the rendering is plain Polyline
/// reconstruction that we'll exercise manually in PostIt.Desktop.
/// </summary>
public class SignaturePageViewModelTests
{
    [Fact]
    public void Default_constructor_uses_default_dimensions()
    {
        var vm = new SignaturePageViewModel();
        Assert.Equal(SignaturePageViewModel.DefaultWidth, vm.Width);
        Assert.Equal(SignaturePageViewModel.DefaultHeight, vm.Height);
    }

    [Fact]
    public void Constructor_rejects_non_positive_dimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SignaturePageViewModel(0, 100));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SignaturePageViewModel(100, 0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SignaturePageViewModel(-1, 100));
    }

    [Fact]
    public void Attach_then_Detach_is_idempotent()
    {
        var vm = new SignaturePageViewModel();
        var pad = new SignaturePadControl();
        vm.Attach(pad);
        vm.Detach();
        // Second detach is a no-op: must not throw.
        vm.Detach();
    }

    [Fact]
    public void Attach_rejects_null()
    {
        var vm = new SignaturePageViewModel();
        Assert.Throws<ArgumentNullException>(() => vm.Attach(null!));
    }

    [Fact]
    public void StrokeCompleted_updates_status_and_counts()
    {
        var vm = new SignaturePageViewModel();
        var pad = new SignaturePadControl();
        vm.Attach(pad);

        // Drive the control via the test hooks so we don't depend
        // on Avalonia pointer events.
        pad.AppendPointForTest(1_000, 1_000);
        pad.AppendPointForTest(2_000, 2_000);
        pad.SealStrokeForTest();

        Assert.Equal(1, vm.StrokeCount);
        Assert.Equal(2, vm.PointCount);
        Assert.Contains("1 trait", vm.StatusMessage);
    }

    [Fact]
    public void Clear_resets_counts_and_buffer()
    {
        var vm = new SignaturePageViewModel();
        var pad = new SignaturePadControl();
        vm.Attach(pad);

        pad.AppendPointForTest(1, 1);
        pad.SealStrokeForTest();
        Assert.Equal(1, vm.StrokeCount);

        vm.Clear();

        Assert.Equal(0, vm.StrokeCount);
        Assert.Equal(0, vm.PointCount);
        Assert.Empty(pad.Strokes);
        Assert.Contains("Effacé", vm.StatusMessage);
    }

    [Fact]
    public async Task CaptureAsync_on_empty_buffer_reports_and_writes_nothing()
    {
        var vm = new SignaturePageViewModel();
        var pad = new SignaturePadControl();
        vm.Attach(pad);

        await vm.CaptureAsync();

        Assert.Contains("Rien", vm.StatusMessage);
        Assert.Null(vm.LastCapturedPath);
    }

    [Fact]
    public async Task CaptureAsync_writes_a_yavsc_signature_v1_file()
    {
        // The VM uses Environment.SpecialFolder.LocalApplicationData,
        // which we cannot redirect per-call without a constructor
        // seam. We test the produced file's structure rather than
        // its text formatting, because System.Text.Json's pretty-
        // printer is not part of the contract we're locking down.
        var vm = new SignaturePageViewModel();
        var pad = new SignaturePadControl();
        vm.Attach(pad);

        pad.AppendPointForTest(1_000, 2_000);
        pad.AppendPointForTest(3_000, 4_000);
        pad.SealStrokeForTest();

        await vm.CaptureAsync();

        Assert.NotNull(vm.LastCapturedPath);
        Assert.True(File.Exists(vm.LastCapturedPath!), $"file missing: {vm.LastCapturedPath}");

        using var doc = JsonDocument.Parse(File.ReadAllText(vm.LastCapturedPath!));
        var root = doc.RootElement;

        Assert.Equal("yavsc.signature/v1", root.GetProperty("format").GetString());
        Assert.Equal(10_000, root.GetProperty("coordinateMax").GetInt32());
        Assert.Equal(1, root.GetProperty("strokeCount").GetInt32());

        var strokes = root.GetProperty("strokes");
        Assert.Equal(JsonValueKind.Array, strokes.ValueKind);
        // [k=2, x0, y0, x1, y1]
        Assert.Equal(5, strokes.GetArrayLength());
        Assert.Equal(2, strokes[0].GetInt32());      // k (2 points)
        Assert.Equal(1_000, strokes[1].GetInt32());   // x0
        Assert.Equal(2_000, strokes[2].GetInt32());   // y0
        Assert.Equal(3_000, strokes[3].GetInt32());   // x1
        Assert.Equal(4_000, strokes[4].GetInt32());   // y1
    }

    [Fact]
    public async Task CaptureAsync_creates_directory_if_missing()
    {
        var vm = new SignaturePageViewModel();
        var pad = new SignaturePadControl();
        vm.Attach(pad);
        pad.AppendPointForTest(1, 1);
        pad.SealStrokeForTest();

        // The directory must exist after the call (CreateDirectory
        // in the VM handles this).
        await vm.CaptureAsync();

        var dir = Path.GetDirectoryName(vm.LastCapturedPath!);
        Assert.NotNull(dir);
        Assert.True(Directory.Exists(dir), $"directory missing: {dir}");
    }
}
