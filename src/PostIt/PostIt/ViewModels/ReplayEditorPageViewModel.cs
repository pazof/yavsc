using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

/// <summary>A mark of the original source timeline, with its local private thumbnail.</summary>
public sealed partial class ReplayMarkItem(long timeMs) : ObservableObject
{
    public long TimeMs { get; } = timeMs;
    public string Time => ReplayControlsState.FormatPreciseTime(TimeMs);
    [ObservableProperty]
    public partial Bitmap? Thumbnail { get; set; }
    /// <summary>The mark lies in a removed range and will be dropped on save.</summary>
    [ObservableProperty]
    public partial bool IsRemoved { get; set; }
    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

public sealed record ReplayRangeItem(long StartMs, long EndMs)
{
    public string Label => $"{ReplayControlsState.FormatPreciseTime(StartMs)} → {ReplayControlsState.FormatPreciseTime(EndMs)}"
        + $" ({ReplayControlsState.FormatPreciseTime(EndMs - StartMs)})";
}

/// <summary>
/// Edits the marks and removed ranges of a refined replay draft. Times use the original source timeline;
/// a removed range [start, end) drops the frame at its start mark and keeps the frame at its end mark.
/// </summary>
public sealed partial class ReplayEditorPageViewModel : ViewModelBase
{
    public const int MaxMarks = 500;
    private readonly PodcastApiClient _client;
    private PodcastResponse _podcast;
    private string _status = "";
    private bool _busy;
    private bool _dirty;

    public ReplayEditorPageViewModel(PodcastApiClient client, PodcastResponse podcast)
    {
        _client = client;
        _podcast = podcast;
        AddMarkCommand = new AsyncRelayCommand(AddMarkAsync, () => CanEdit && Marks.Count < MaxMarks);
        GoToMarkCommand = new RelayCommand<ReplayMarkItem>(m => { if (m is not null) SeekRequested?.Invoke(m.TimeMs); });
        RemoveMarkCommand = new RelayCommand<ReplayMarkItem>(RemoveMark, _ => CanEdit);
        ClearMarksCommand = new RelayCommand(ClearMarks, () => CanEdit && Marks.Count > 0);
        SetRangeStartCommand = new RelayCommand<ReplayMarkItem>(m => Select(m, start: true), _ => CanEdit);
        SetRangeEndCommand = new RelayCommand<ReplayMarkItem>(m => Select(m, start: false), _ => CanEdit);
        ClearSelectionCommand = new RelayCommand(() => { RangeStart = null; RangeEnd = null; },
            () => RangeStart is not null || RangeEnd is not null);
        RemoveSelectionCommand = new RelayCommand(RemoveSelection,
            () => CanEdit && RangeStart is not null && RangeEnd is not null);
        RestoreRangeCommand = new RelayCommand<ReplayRangeItem>(RestoreRange, _ => CanEdit);
        SaveCommand = new AsyncRelayCommand(() => SaveAsync(), () => CanEdit && IsDirty);
        ReloadCommand = new AsyncRelayCommand(ReloadAsync, () => !IsBusy);
        PublishCommand = new AsyncRelayCommand(PublishAsync,
            () => CanEdit && (IsDirty || _podcast.Draft?.PublishedRevision != _podcast.Draft?.Revision));
        Load(podcast, keepThumbnails: false);
    }

    public PodcastApiClient PodcastClient => _client;
    public PodcastResponse Podcast => _podcast;
    public string Title => $"Raffinage : {_podcast.Title}";
    public long SourceBytes => _podcast.Draft?.SourceBytes ?? 0;
    /// <summary>Duration used by the server to validate marks and ranges.</summary>
    public long SourceDurationMs => (long)Math.Ceiling(_podcast.DurationSeconds * 1000);
    public ObservableCollection<ReplayMarkItem> Marks { get; } = new();
    public ObservableCollection<ReplayRangeItem> RemovedRanges { get; } = new();

    /// <summary>Requests the player to show this source time.</summary>
    public event Action<long>? SeekRequested;
    /// <summary>Captures a private thumbnail of the frame currently shown; set by the view.</summary>
    public Func<CancellationToken, Task<Bitmap?>>? CaptureThumbnail { get; set; }

    [ObservableProperty]
    public partial long CurrentTimeMs { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionSummary))]
    public partial ReplayMarkItem? RangeStart { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionSummary))]
    public partial ReplayMarkItem? RangeEnd { get; set; }

    public string CurrentTime => ReplayControlsState.FormatPreciseTime(CurrentTimeMs);
    public string SelectionSummary => (RangeStart, RangeEnd) switch
    {
        (null, null) => "Choisissez une marque de debut et une marque de fin.",
        ({ } s, null) => $"Debut {s.Time} ; choisissez la marque de fin.",
        (null, { } e) => $"Fin {e.Time} ; choisissez la marque de debut.",
        var (s, e) => $"Plage a supprimer : {s.Time} → {e.Time}. L'image de la marque de debut est supprimee, celle de la marque de fin est conservee."
    };
    public string DurationSummary
    {
        get
        {
            var removed = RemovedRanges.Sum(r => r.EndMs - r.StartMs);
            return $"Source : {ReplayControlsState.FormatPreciseTime(SourceDurationMs)} ; resultat estime : "
                + $"{ReplayControlsState.FormatPreciseTime(Math.Max(0, SourceDurationMs - removed))} ({RemovedRanges.Count} plage(s) supprimee(s)).";
        }
    }
    public string DraftState => _podcast.Draft switch
    {
        null => "Brouillon indisponible.",
        { State: PodcastDraftState.Publishing } d => $"Publication de la revision {d.Revision} en cours : modification impossible.",
        { State: not PodcastDraftState.Ready } => "Brouillon non modifiable pour le moment.",
        { Error: { Length: > 0 } error } => $"Derniere publication en echec : {error}",
        { PublishedRevision: { } p } d when p == d.Revision => $"Revision {d.Revision} publiee.",
        var d => $"Revision {d.Revision}, non publiee."
    };

    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public bool IsBusy
    {
        get => _busy;
        private set { if (SetProperty(ref _busy, value)) UpdateCommands(); }
    }
    public bool IsDirty
    {
        get => _dirty;
        private set { if (SetProperty(ref _dirty, value)) UpdateCommands(); }
    }
    public bool CanEdit => !IsBusy && _podcast.Draft?.State == PodcastDraftState.Ready && !_podcast.OriginalsDeleted;

    public IAsyncRelayCommand AddMarkCommand { get; }
    public IRelayCommand<ReplayMarkItem> GoToMarkCommand { get; }
    public IRelayCommand<ReplayMarkItem> RemoveMarkCommand { get; }
    public IRelayCommand ClearMarksCommand { get; }
    public IRelayCommand<ReplayMarkItem> SetRangeStartCommand { get; }
    public IRelayCommand<ReplayMarkItem> SetRangeEndCommand { get; }
    public IRelayCommand ClearSelectionCommand { get; }
    public IRelayCommand RemoveSelectionCommand { get; }
    public IRelayCommand<ReplayRangeItem> RestoreRangeCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand ReloadCommand { get; }
    public IAsyncRelayCommand PublishCommand { get; }
    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => true; protected set { } }

    partial void OnCurrentTimeMsChanged(long value) => OnPropertyChanged(nameof(CurrentTime));
    partial void OnRangeStartChanged(ReplayMarkItem? oldValue, ReplayMarkItem? newValue) => UpdateSelection(oldValue, newValue);
    partial void OnRangeEndChanged(ReplayMarkItem? oldValue, ReplayMarkItem? newValue) => UpdateSelection(oldValue, newValue);

    private void UpdateSelection(ReplayMarkItem? oldValue, ReplayMarkItem? newValue)
    {
        if (oldValue is not null) oldValue.IsSelected = ReferenceEquals(oldValue, RangeStart) || ReferenceEquals(oldValue, RangeEnd);
        if (newValue is not null) newValue.IsSelected = true;
        UpdateCommands();
    }

    private async Task AddMarkAsync()
    {
        var time = Math.Clamp(CurrentTimeMs, 0, SourceDurationMs);
        if (Marks.Any(m => m.TimeMs == time))
        {
            Status = $"Une marque existe deja a {ReplayControlsState.FormatPreciseTime(time)}.";
            return;
        }
        var mark = new ReplayMarkItem(time);
        var index = Marks.TakeWhile(m => m.TimeMs < time).Count();
        Marks.Insert(index, mark);
        UpdateRemovedMarks();
        IsDirty = true;
        Status = $"Marque ajoutee a {mark.Time}.";
        UpdateCommands();
        if (CaptureThumbnail is { } capture)
        {
            try { mark.Thumbnail = await capture(CancellationToken.None); }
            catch (Exception ex) { Status = $"Marque ajoutee a {mark.Time}, sans miniature : {ex.Message}"; }
        }
    }

    private void RemoveMark(ReplayMarkItem? mark)
    {
        if (mark is null || !Marks.Remove(mark)) return;
        if (ReferenceEquals(RangeStart, mark)) RangeStart = null;
        if (ReferenceEquals(RangeEnd, mark)) RangeEnd = null;
        mark.Thumbnail?.Dispose();
        IsDirty = true;
        UpdateCommands();
    }

    private void ClearMarks()
    {
        RangeStart = null;
        RangeEnd = null;
        foreach (var mark in Marks) mark.Thumbnail?.Dispose();
        Marks.Clear();
        IsDirty = true;
        UpdateCommands();
    }

    private void Select(ReplayMarkItem? mark, bool start)
    {
        if (mark is null) return;
        if (start) RangeStart = mark;
        else RangeEnd = mark;
        if (ReferenceEquals(RangeStart, RangeEnd)) { if (start) RangeEnd = null; else RangeStart = null; }
    }

    private void RemoveSelection()
    {
        if (RangeStart is not { } a || RangeEnd is not { } b) return;
        var range = new ReplayRange(Math.Min(a.TimeMs, b.TimeMs), Math.Max(a.TimeMs, b.TimeMs));
        if (!TrySetRanges(RemovedRanges.Select(r => new ReplayRange(r.StartMs, r.EndMs)).Append(range))) return;
        RangeStart = null;
        RangeEnd = null;
        Status = "Plage ajoutee aux suppressions. Les marques qu'elle contient seront retirees a l'enregistrement.";
    }

    private void RestoreRange(ReplayRangeItem? range)
    {
        if (range is null) return;
        if (TrySetRanges(RemovedRanges.Where(r => r != range).Select(r => new ReplayRange(r.StartMs, r.EndMs))))
            Status = $"Plage {range.Label} retablie.";
    }

    private bool TrySetRanges(IEnumerable<ReplayRange> ranges)
    {
        IReadOnlyList<ReplayRange> normalized;
        try { normalized = ReplayEditPlan.RemovedRanges(ranges.ToArray(), SourceDurationMs); }
        catch (ArgumentException ex)
        {
            Status = $"Plage refusee : {ex.Message.Split(" (Parameter")[0]}";
            return false;
        }
        RemovedRanges.Clear();
        foreach (var range in normalized) RemovedRanges.Add(new(range.StartMs, range.EndMs));
        UpdateRemovedMarks();
        IsDirty = true;
        OnPropertyChanged(nameof(DurationSummary));
        UpdateCommands();
        return true;
    }

    private void UpdateRemovedMarks()
    {
        foreach (var mark in Marks)
            mark.IsRemoved = RemovedRanges.Any(r => mark.TimeMs >= r.StartMs && mark.TimeMs < r.EndMs);
    }

    /// <summary>Saves local changes; returns false on failure, including a revision conflict.</summary>
    public async Task<bool> SaveAsync()
    {
        IsBusy = true;
        try
        {
            var draft = _podcast.Draft ?? throw new InvalidOperationException("Brouillon indisponible.");
            var saved = await _client.SaveDraftAsync(_podcast.Id, new SavePodcastDraftRequest
            {
                Revision = draft.Revision,
                Marks = Marks.Select(m => m.TimeMs).ToArray(),
                RemovedRanges = RemovedRanges.Select(r => new PodcastDraftRange(r.StartMs, r.EndMs)).ToArray()
            });
            Load(saved, keepThumbnails: true);
            Status = $"Brouillon enregistre (revision {saved.Draft?.Revision}).";
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            Status = "Enregistrement refuse : le brouillon a ete modifie ailleurs ou n'est plus modifiable. "
                + "Rechargez-le (vos modifications locales seront perdues).";
            return false;
        }
        catch (Exception ex)
        {
            Status = $"Enregistrement impossible : {ex.Message}";
            return false;
        }
        finally { IsBusy = false; }
    }

    private async Task ReloadAsync()
    {
        IsBusy = true;
        try
        {
            var manifest = await _client.GetAsync(_podcast.Id);
            Load(manifest.Podcast, keepThumbnails: true);
            Status = "Brouillon recharge depuis le serveur.";
        }
        catch (Exception ex) { Status = $"Rechargement impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private async Task PublishAsync()
    {
        if (IsDirty && !await SaveAsync()) return;
        IsBusy = true;
        try
        {
            var draft = _podcast.Draft ?? throw new InvalidOperationException("Brouillon indisponible.");
            Load(await _client.PublishDraftAsync(_podcast.Id, draft.Revision), keepThumbnails: true);
            Status = $"Publication de la revision {draft.Revision} demandee ; le MP4 remplacera le replay une fois prepare.";
        }
        catch (Exception ex) { Status = $"Publication impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private void Load(PodcastResponse podcast, bool keepThumbnails)
    {
        var thumbnails = keepThumbnails ? Marks.Where(m => m.Thumbnail is not null).ToDictionary(m => m.TimeMs, m => m.Thumbnail) : new Dictionary<long, Bitmap?>();
        var marks = podcast.Draft?.Marks ?? [];
        foreach (var mark in Marks)
            if (!marks.Contains(mark.TimeMs)) mark.Thumbnail?.Dispose();
        _podcast = podcast;
        RangeStart = null;
        RangeEnd = null;
        Marks.Clear();
        foreach (var time in marks.Order())
            Marks.Add(new ReplayMarkItem(time) { Thumbnail = thumbnails.GetValueOrDefault(time) });
        RemovedRanges.Clear();
        foreach (var range in (podcast.Draft?.RemovedRanges ?? []).OrderBy(r => r.StartMs))
            RemovedRanges.Add(new(range.StartMs, range.EndMs));
        UpdateRemovedMarks();
        IsDirty = false;
        OnPropertyChanged(nameof(Podcast));
        OnPropertyChanged(nameof(DraftState));
        OnPropertyChanged(nameof(DurationSummary));
        OnPropertyChanged(nameof(CanEdit));
        UpdateCommands();
    }

    private void UpdateCommands()
    {
        OnPropertyChanged(nameof(CanEdit));
        AddMarkCommand?.NotifyCanExecuteChanged();
        RemoveMarkCommand?.NotifyCanExecuteChanged();
        ClearMarksCommand?.NotifyCanExecuteChanged();
        SetRangeStartCommand?.NotifyCanExecuteChanged();
        SetRangeEndCommand?.NotifyCanExecuteChanged();
        ClearSelectionCommand?.NotifyCanExecuteChanged();
        RemoveSelectionCommand?.NotifyCanExecuteChanged();
        RestoreRangeCommand?.NotifyCanExecuteChanged();
        SaveCommand?.NotifyCanExecuteChanged();
        ReloadCommand?.NotifyCanExecuteChanged();
        PublishCommand?.NotifyCanExecuteChanged();
    }

    /// <summary>Releases the local thumbnails when leaving the editor.</summary>
    public void ReleaseThumbnails()
    {
        foreach (var mark in Marks)
        {
            mark.Thumbnail?.Dispose();
            mark.Thumbnail = null;
        }
    }
}
