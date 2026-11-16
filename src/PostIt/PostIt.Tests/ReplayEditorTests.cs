using System.Net;
using PostIt.ViewModels;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Tests;

[Collection(AvaloniaUiCollection.Name)]
public sealed class ReplayEditorTests(AvaloniaUiFixture ui)
{
    [Fact]
    public Task Marks_are_added_at_the_current_time_once_and_ranges_flag_their_marks()
        => ui.RunAsync(() =>
    {
        var vm = Editor(new DraftApi());
        long? seek = null;
        vm.SeekRequested += t => seek = t;

        vm.CurrentTimeMs = 12_000;
        vm.AddMarkCommand.Execute(null);
        vm.AddMarkCommand.Execute(null);
        Assert.Single(vm.Marks);
        Assert.Contains("existe deja", vm.Status);
        vm.CurrentTimeMs = 4_000;
        vm.AddMarkCommand.Execute(null);
        vm.CurrentTimeMs = 20_000;
        vm.AddMarkCommand.Execute(null);
        Assert.Equal([4_000L, 12_000, 20_000], vm.Marks.Select(m => m.TimeMs));

        vm.GoToMarkCommand.Execute(vm.Marks[1]);
        Assert.Equal(12_000, seek);
        // The end mark may be chosen first: the range is ordered.
        vm.SetRangeEndCommand.Execute(vm.Marks[0]);
        vm.SetRangeStartCommand.Execute(vm.Marks[2]);
        Assert.True(vm.Marks[0].IsSelected);
        vm.RemoveSelectionCommand.Execute(null);

        var range = Assert.Single(vm.RemovedRanges);
        Assert.Equal((4_000L, 20_000L), (range.StartMs, range.EndMs));
        Assert.Equal([true, true, false], vm.Marks.Select(m => m.IsRemoved));
        Assert.All(vm.Marks, m => Assert.False(m.IsSelected));
        Assert.True(vm.IsDirty);

        vm.RestoreRangeCommand.Execute(range);
        Assert.Empty(vm.RemovedRanges);
        Assert.All(vm.Marks, m => Assert.False(m.IsRemoved));
        return Task.CompletedTask;
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Overlapping_ranges_are_merged_and_removing_everything_is_refused()
        => ui.RunAsync(() =>
    {
        var vm = Editor(new DraftApi());
        foreach (var t in new long[] { 0, 10_000, 20_000, 30_000, 60_000 })
        {
            vm.CurrentTimeMs = t;
            vm.AddMarkCommand.Execute(null);
        }
        RemoveBetween(vm, 10_000, 30_000);
        RemoveBetween(vm, 20_000, 60_000);
        var merged = Assert.Single(vm.RemovedRanges);
        Assert.Equal((10_000L, 60_000L), (merged.StartMs, merged.EndMs));

        RemoveBetween(vm, 0, 10_000);
        Assert.Contains("Plage refusee", vm.Status);
        Assert.Equal(merged, Assert.Single(vm.RemovedRanges));
        Assert.Equal(0, vm.RangeStart?.TimeMs);
        return Task.CompletedTask;
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Saving_sends_the_read_revision_and_reloads_the_server_draft()
        => ui.RunAsync(async () =>
    {
        var api = new DraftApi();
        var vm = Editor(api);
        foreach (var t in new long[] { 5_000, 15_000, 25_000 })
        {
            vm.CurrentTimeMs = t;
            await vm.AddMarkCommand.ExecuteAsync(null);
        }
        RemoveBetween(vm, 5_000, 15_000);

        Assert.True(await vm.SaveAsync());

        var request = Assert.IsType<SavePodcastDraftRequest>(api.Saved);
        Assert.Equal(3, request.Revision);
        Assert.Equal([5_000L, 15_000, 25_000], request.Marks);
        Assert.Equal([new PodcastDraftRange(5_000, 15_000)], request.RemovedRanges);
        // The server drops the marks inside removed ranges.
        Assert.Equal([15_000L, 25_000], vm.Marks.Select(m => m.TimeMs));
        Assert.Equal(4, vm.Podcast.Draft?.Revision);
        Assert.False(vm.IsDirty);
        Assert.False(vm.SaveCommand.CanExecute(null));
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task A_revision_conflict_keeps_local_changes_and_asks_for_a_reload()
        => ui.RunAsync(async () =>
    {
        var api = new DraftApi { Conflict = true };
        var vm = Editor(api);
        vm.CurrentTimeMs = 5_000;
        await vm.AddMarkCommand.ExecuteAsync(null);

        Assert.False(await vm.SaveAsync());

        Assert.Contains("Rechargez", vm.Status);
        Assert.True(vm.IsDirty);
        Assert.Single(vm.Marks);
        Assert.Equal(3, vm.Podcast.Draft?.Revision);
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Publishing_saves_pending_changes_first_and_publishes_the_saved_revision()
        => ui.RunAsync(async () =>
    {
        var api = new DraftApi();
        var vm = Editor(api);
        vm.CurrentTimeMs = 5_000;
        await vm.AddMarkCommand.ExecuteAsync(null);

        await vm.PublishCommand.ExecuteAsync(null);

        Assert.Equal([HttpMethod.Put, HttpMethod.Post], api.Calls.Select(c => c.Method));
        Assert.Equal(4, Assert.IsType<PublishPodcastDraftRequest>(api.Calls[1].Body).Revision);
        Assert.Equal(PodcastDraftState.Publishing, vm.Podcast.Draft?.State);
        Assert.False(vm.CanEdit);
        Assert.False(vm.AddMarkCommand.CanExecute(null));
    }, TestContext.Current.CancellationToken);

    private static ReplayEditorPageViewModel Editor(DraftApi api)
        => new(new PodcastApiClient(api, () => "https://api.example/"), api.Podcast);

    private static void RemoveBetween(ReplayEditorPageViewModel vm, long start, long end)
    {
        vm.SetRangeStartCommand.Execute(vm.Marks.Single(m => m.TimeMs == start));
        vm.SetRangeEndCommand.Execute(vm.Marks.Single(m => m.TimeMs == end));
        vm.RemoveSelectionCommand.Execute(null);
    }

    private sealed class DraftApi : IYavscApiClient
    {
        public PodcastResponse Podcast { get; private set; } = new(Guid.NewGuid(), "Concert", "owner", "Owner",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, PodcastState.Completed, 1, 1024, 60)
        {
            RefineReplay = true,
            Draft = new PodcastDraftResponse(PodcastDraftState.Ready, 3, [], [], 1024)
        };
        public bool Conflict { get; init; }
        public SavePodcastDraftRequest? Saved { get; private set; }
        public List<(HttpMethod Method, string Path, object? Body)> Calls { get; } = [];
        public HttpClient Http { get; } = new();

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Calls.Add((method, path, body));
            if (Conflict) throw new HttpRequestException("conflict", null, HttpStatusCode.Conflict);
            var draft = Podcast.Draft!;
            if (path.EndsWith("/draft", StringComparison.Ordinal))
            {
                Saved = Assert.IsType<SavePodcastDraftRequest>(body);
                var ranges = Saved.RemovedRanges;
                Podcast = Podcast with
                {
                    Draft = draft with
                    {
                        Revision = draft.Revision + 1,
                        Marks = Saved.Marks.Where(m => !ranges.Any(r => m >= r.StartMs && m < r.EndMs)).ToArray(),
                        RemovedRanges = ranges
                    }
                };
            }
            else if (path.EndsWith("/draft/publish", StringComparison.Ordinal))
                Podcast = Podcast with { Draft = draft with { State = PodcastDraftState.Publishing } };
            else throw new InvalidOperationException(path);
            return Task.FromResult((T)(object)Podcast);
        }

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => throw new NotSupportedException();
        public ValueTask DisposeAsync()
        {
            Http.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
