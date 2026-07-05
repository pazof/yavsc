using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Controls;
using PostIt.Models;

namespace PostIt.ViewModels;

/// <summary>
/// Backing state for <see cref="PostIt.Views.SignaturePage"/>.
///
/// The page exists to produce a <see cref="SignaturePadData"/>
/// (length-prefixed normalised int[]) from a human signature drawn
/// with the mouse (Desktop) or finger (touch / Android). The page
/// is a recipient of an external trigger — a SignalR push from
/// Yavsc.Org telling PostIt "a devis has been sent, sign here" —
/// so it intentionally has no first-class entry point in
/// <see cref="MainPage"/>. The only "open" affordance today is a
/// dev-only shortcut on the blog editor, marked for removal once
/// the SignalR handler lands.
///
/// Output path is the platform-friendly per-user data directory
/// (XDG_DATA_HOME / AppData / NSDocumentDirectory on iOS). Files
/// are JSON, one per capture, named
/// <c>signature-{yyyyMMdd-HHmmssfff}.json</c>. This is a stop-gap
/// until the Yavsc.Org endpoint exists; the contract there will
/// be <c>POST /api/signature/{devisId}</c> with this same payload.
/// </summary>
public partial class SignaturePageViewModel : ViewModelBase
{
    /// <summary>
    /// Default capture surface, in DIPs. 3:1 ratio matches a
    /// signature line at the bottom of an A4 contract.
    /// </summary>
    public const double DefaultWidth = 600;
    public const double DefaultHeight = 200;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Prêt.";

    [ObservableProperty]
    public partial int StrokeCount { get; set; }

    [ObservableProperty]
    public partial int PointCount { get; set; }

    [ObservableProperty]
    public partial string? LastCapturedPath { get; set; }

    public double Width { get; }
    public double Height { get; }

    private SignaturePadControl? _control;

    public override bool CanNavigateNext
    {
        get => false;
        protected set { _ = value; }
    }

    public override bool CanNavigatePrevious
    {
        get => true;
        protected set { _ = value; }
    }

    public SignaturePageViewModel()
        : this(DefaultWidth, DefaultHeight)
    {
    }

    public SignaturePageViewModel(double width, double height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Bind a freshly-constructed (or re-templated) control to this
    /// VM. Called from the view's code-behind once the control has
    /// been added to the visual tree and its template applied (so
    /// <see cref="SignaturePadControl.CaptureArea"/> is wired).
    /// </summary>
    public void Attach(SignaturePadControl control)
    {
        if (control is null) throw new ArgumentNullException(nameof(control));
        Detach();
        _control = control;
        _control.RedrawRequested += OnRedraw;
        _control.StrokeCompleted += OnStrokeCompleted;
        RefreshCounts();
    }

    public void Detach()
    {
        if (_control is null) return;
        _control.RedrawRequested -= OnRedraw;
        _control.StrokeCompleted -= OnStrokeCompleted;
        _control = null;
    }

    private void OnStrokeCompleted(object? sender, SignaturePadData data)
    {
        StatusMessage = $"Trait terminé. {data.StrokeCount} trait(s).";
        RefreshCounts();
    }

    private void OnRedraw(object? sender, EventArgs e) => RefreshCounts();

    private void RefreshCounts()
    {
        if (_control is null) return;
        var snap = _control.Snapshot();
        StrokeCount = snap.StrokeCount;
        PointCount = snap.PointCount;
    }

    [RelayCommand]
    public void Clear()
    {
        _control?.Clear();
        StatusMessage = "Effacé.";
        RefreshCounts();
    }

    [RelayCommand]
    public async Task CaptureAsync()
    {
        if (_control is null)
        {
            StatusMessage = "Contrôle non attaché.";
            return;
        }

        var data = _control.Snapshot();
        if (data.IsEmpty)
        {
            StatusMessage = "Rien à capturer.";
            return;
        }

        try
        {
            var path = WriteCapture(data);
            LastCapturedPath = path;
            StatusMessage = $"Capture enregistrée: {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        await Task.CompletedTask;
    }

    private static string WriteCapture(SignaturePadData data)
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PostIt", "signatures");
        Directory.CreateDirectory(dir);

        var fileName = $"signature-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.json";
        var path = Path.Combine(dir, fileName);

        var payload = new
        {
            format = "yavsc.signature/v1",
            coordinateMax = SignaturePadData.CoordinateMax,
            capturedAtUtc = DateTime.UtcNow,
            strokes = data.Strokes,
            strokeCount = data.StrokeCount,
        };
        File.WriteAllText(
            path,
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);
        return path;
    }
}
