using PostIt.Services;
using PostIt.ViewModels;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
[Collection(AvaloniaUiCollection.Name)]
public sealed class PodcastScheduleTests(AvaloniaUiFixture ui)
{
    [Fact]
    public Task Late_obs_credentials_are_not_displayed_after_selection_changes_or_navigation()
        => ui.RunAsync(async () =>
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-obs-late-{Guid.NewGuid():N}.json");
        var store = new TokenStore(path);
        store.Save(new RefreshTokenRecord("test-access", "test-refresh", DateTimeOffset.UtcNow.AddHours(1), null));
        try
        {
            await using var api = new ScheduleApi(store, path + ".settings");
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            var vm = new PodcastsPageViewModel(new PodcastApiClient(api, () => "https://api.example/"), api, session);
            await vm.ActivateAsync();
            vm.SelectedPodcast = api.Plan;
            api.PendingActivation = new(TaskCreationOptions.RunContinuationsAsynchronously);
            var activation = vm.ActivateObsCommand.ExecuteAsync(null);
            vm.SelectedPodcast = api.Plan with { Id = Guid.NewGuid() };
            vm.SelectedPodcast = api.Plan;
            api.PendingActivation.SetResult(new("srt://obs.example:10000?passphrase=secret", "", DateTimeOffset.UtcNow.AddMinutes(30)));
            await activation;
            Assert.Empty(vm.ObsServerUrl);
            Assert.Empty(vm.ObsStreamKey);
            api.PendingActivation = new(TaskCreationOptions.RunContinuationsAsynchronously);
            activation = vm.ActivateObsCommand.ExecuteAsync(null);
            await vm.DeactivateAsync();
            api.PendingActivation.SetResult(new("srt://obs.example:10000?passphrase=secret", "", DateTimeOffset.UtcNow.AddMinutes(30)));
            await activation;
            Assert.Empty(vm.ObsServerUrl);
            Assert.Empty(vm.ObsStreamKey);
        }
        finally { File.Delete(path); File.Delete(path + ".settings"); }
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Obs_activation_is_owned_displays_credentials_and_clears_them_on_navigation_and_revocation()
        => ui.RunAsync(async () =>
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-obs-{Guid.NewGuid():N}.json");
        var store = new TokenStore(path);
        store.Save(new RefreshTokenRecord("test-access", "test-refresh", DateTimeOffset.UtcNow.AddHours(1), null));
        try
        {
            await using var api = new ScheduleApi(store, path + ".settings");
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            var vm = new PodcastsPageViewModel(new PodcastApiClient(api, () => "https://api.example/"), api, session);
            await vm.ActivateAsync();
            vm.SelectedPodcast = api.Plan;
            Assert.True(vm.ActivateObsCommand.CanExecute(null));
            await vm.ActivateObsCommand.ExecuteAsync(null);
            Assert.Equal("srt://obs.example:10000?passphrase=stream-secret", vm.ObsServerUrl);
            Assert.Empty(vm.ObsStreamKey);
            Assert.False(vm.IsBroadcasting);
            api.Settings.Save();
            Assert.DoesNotContain("stream-secret", await File.ReadAllTextAsync(path + ".settings"));
            vm.SelectedPodcast = api.Plan with { Id = Guid.NewGuid(), OwnerId = "other" };
            Assert.Empty(vm.ObsServerUrl);
            Assert.Empty(vm.ObsStreamKey);
            Assert.False(vm.ActivateObsCommand.CanExecute(null));
            vm.SelectedPodcast = api.Plan;
            await vm.ActivateObsCommand.ExecuteAsync(null);
            await vm.DeactivateAsync();
            Assert.Empty(vm.ObsServerUrl);
            Assert.Empty(vm.ObsStreamKey);
            Assert.True(api.ObsEnabled);
            await vm.ActivateAsync();
            vm.SelectedPodcast = api.Plan;
            await vm.ActivateObsCommand.ExecuteAsync(null);
            await vm.DeactivateObsCommand.ExecuteAsync(null);
            Assert.False(api.ObsEnabled);
            Assert.Empty(vm.ObsServerUrl);
            Assert.Empty(vm.ObsStreamKey);
            Assert.EndsWith("/obs", api.RevokedPath);
            await vm.DeactivateAsync();
        }
        finally { File.Delete(path); File.Delete(path + ".settings"); }
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Preparation_and_editing_send_schedule_and_optional_announcement_without_capture()
        => ui.RunAsync(async () =>
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), $"postit-plan-{Guid.NewGuid():N}.json");
        var store = new TokenStore(tokenPath);
        store.Save(new RefreshTokenRecord("test-access-token", "test-refresh-token", DateTimeOffset.UtcNow.AddHours(1), null));
        try
        {
            await using var api = new ScheduleApi(store, tokenPath + ".settings");
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            var vm = new PodcastsPageViewModel(new PodcastApiClient(api, () => "https://api.example/"), api, session)
            {
                Title = "Concert", ScheduledDate = DateTimeOffset.Now.AddDays(7),
                ScheduledTime = new TimeSpan(14, 15, 0), PlannedDurationMinutes = 90
            };
            Assert.True(vm.PrepareCommand.CanExecute(null));
            await vm.PrepareCommand.ExecuteAsync(null);
            Assert.Equal(HttpMethod.Post, api.SavedMethod);
            Assert.EndsWith("/podcasts/planned", api.SavedPath);
            Assert.Equal(90, api.Saved?.PlannedDurationMinutes);
            Assert.Null(api.Saved?.PublicAnnouncement);
            Assert.Equal(14, api.Saved?.ScheduledStartAt.Hour);
            Assert.Equal(PodcastState.Planned, vm.SelectedPodcast?.State);
            Assert.False(vm.FinishCommand.CanExecute(null));
            Assert.True(vm.DeleteCommand.CanExecute(null));
            Assert.True(vm.UpdatePlanCommand.CanExecute(null));
            vm.Title = "Nouveau titre";
            vm.PublishAnnouncement = true;
            vm.Announcement = "Venez au concert";
            await vm.UpdatePlanCommand.ExecuteAsync(null);
            Assert.Equal(HttpMethod.Put, api.SavedMethod);
            Assert.EndsWith($"/podcasts/{api.Plan.Id}/planned", api.SavedPath);
            Assert.Equal("Venez au concert", api.Saved?.PublicAnnouncement);
            Assert.Equal("Nouveau titre", vm.SelectedPodcast?.Title);
            vm.PublishAnnouncement = false;
            await vm.UpdatePlanCommand.ExecuteAsync(null);
            Assert.Null(api.Saved?.PublicAnnouncement);
            vm.PlannedDurationMinutes = 0;
            api.Saved = null;
            await vm.UpdatePlanCommand.ExecuteAsync(null);
            Assert.Null(api.Saved);
            Assert.Contains("Preparation impossible", vm.Status);
            Assert.False(vm.IsBroadcasting);
            vm.SelectedPodcast = api.Plan with { Title = "Titre programme distinct" };
            Assert.Equal("Titre programme distinct", vm.Title);
            Assert.Equal("Nouveau titre", api.Settings.PodcastDefaultTitle);
            vm.NewPlanCommand.Execute(null);
            Assert.Equal("Nouveau titre", vm.Title);
        }
        finally { File.Delete(tokenPath); File.Delete(tokenPath + ".settings"); }
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Refinement_options_are_sent_with_the_plan_and_reloaded_on_selection()
        => ui.RunAsync(async () =>
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), $"postit-refine-{Guid.NewGuid():N}.json");
        var store = new TokenStore(tokenPath);
        store.Save(new RefreshTokenRecord("test-access-token", "test-refresh-token", DateTimeOffset.UtcNow.AddHours(1), null));
        try
        {
            await using var api = new ScheduleApi(store, tokenPath + ".settings");
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            var vm = new PodcastsPageViewModel(new PodcastApiClient(api, () => "https://api.example/"), api, session)
            {
                Title = "Concert", RefineReplay = true, RetentionDays = 30
            };
            await vm.PrepareCommand.ExecuteAsync(null);
            Assert.True(api.Saved?.RefineReplay);
            Assert.Equal(30, api.Saved?.RetentionDays);
            vm.RefineReplay = false;
            vm.RetentionDays = 0;
            vm.SelectedPodcast = null;
            vm.SelectedPodcast = api.Plan;
            Assert.True(vm.RefineReplay);
            Assert.Equal(30, vm.RetentionDays);
            vm.RetentionDays = 366;
            api.Saved = null;
            await vm.UpdatePlanCommand.ExecuteAsync(null);
            Assert.Null(api.Saved);
            Assert.Contains("conservation des originaux", vm.Status);
        }
        finally { File.Delete(tokenPath); File.Delete(tokenPath + ".settings"); }
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Owner_publishes_an_unchanged_draft_then_deletes_the_originals()
        => ui.RunAsync(async () =>
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), $"postit-draft-{Guid.NewGuid():N}.json");
        var store = new TokenStore(tokenPath);
        store.Save(new RefreshTokenRecord("test-access-token", "test-refresh-token", DateTimeOffset.UtcNow.AddHours(1), null));
        try
        {
            await using var api = new ScheduleApi(store, tokenPath + ".settings");
            api.Plan = api.Plan with
            {
                State = PodcastState.Completed, SegmentCount = 3, RefineReplay = true, FlowId = null,
                Replay = new(PodcastReplayState.Pending), Draft = new(PodcastDraftState.Ready, 2, [], []),
                OriginalsExpireAt = DateTimeOffset.UtcNow.AddDays(7)
            };
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            var vm = new PodcastsPageViewModel(new PodcastApiClient(api, () => "https://api.example/"), api, session);
            await vm.ActivateAsync();
            vm.SelectedPodcast = api.Plan;
            Assert.Contains("non publie", vm.DraftSummary);
            Assert.Contains("sera supprimee", vm.DraftSummary);
            Assert.False(vm.DeleteOriginalsCommand.CanExecute(null));
            await vm.PublishDraftCommand.ExecuteAsync(null);
            var publish = Assert.Single(api.DraftCalls);
            Assert.Equal(2, Assert.IsType<PublishPodcastDraftRequest>(publish.Body).Revision);
            Assert.Equal(PodcastDraftState.Publishing, vm.SelectedPodcast?.Draft?.State);
            Assert.False(vm.PublishDraftCommand.CanExecute(null));

            api.Plan = api.Plan with
            {
                Replay = new(PodcastReplayState.Ready, 100),
                Draft = api.Plan.Draft! with { State = PodcastDraftState.Ready, PublishedRevision = 2 }
            };
            await vm.RefreshCommand.ExecuteAsync(null);
            vm.SelectedPodcast = api.Plan;
            Assert.Contains("Revision 2 publiee", vm.DraftSummary);
            Assert.False(vm.PublishDraftCommand.CanExecute(null));
            await vm.DeleteOriginalsCommand.ExecuteAsync(null);
            Assert.Equal(HttpMethod.Delete, api.DraftCalls[^1].Method);
            Assert.EndsWith($"/podcasts/{api.Plan.Id}/originals", api.DraftCalls[^1].Path);
            Assert.True(vm.SelectedPodcast?.OriginalsDeleted);
            Assert.Contains("Originaux supprimes", vm.DraftSummary);
            Assert.False(vm.DeleteOriginalsCommand.CanExecute(null));
            Assert.Equal(PodcastPlaybackSource.PublishedReplay, PodcastPlayback.Choose(vm.SelectedPodcast!).Source);

            vm.SelectedPodcast = api.Plan with { OwnerId = "other" };
            Assert.Empty(vm.DraftSummary);
            await vm.DeactivateAsync();
        }
        finally { File.Delete(tokenPath); File.Delete(tokenPath + ".settings"); }
    }, TestContext.Current.CancellationToken);

    [Theory]
    [InlineData(PodcastState.Live, true, false, false, PodcastPlaybackSource.Originals)]
    [InlineData(PodcastState.Completed, false, false, false, PodcastPlaybackSource.Pending)]
    [InlineData(PodcastState.Completed, false, false, true, PodcastPlaybackSource.PublishedReplay)]
    [InlineData(PodcastState.Completed, true, true, false, PodcastPlaybackSource.Originals)]
    [InlineData(PodcastState.Completed, true, false, false, PodcastPlaybackSource.Unavailable)]
    [InlineData(PodcastState.Interrupted, true, false, false, PodcastPlaybackSource.Unavailable)]
    [InlineData(PodcastState.Interrupted, true, false, true, PodcastPlaybackSource.PublishedReplay)]
    [InlineData(PodcastState.Interrupted, false, false, false, PodcastPlaybackSource.Originals)]
    public void Playback_hides_unpublished_refined_originals_from_other_viewers(
        PodcastState state, bool refine, bool owner, bool published, PodcastPlaybackSource expected)
    {
        var podcast = new PodcastResponse(Guid.NewGuid(), "Concert", "owner", "Owner",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, state, 3, 300, 6)
        {
            RefineReplay = refine,
            Draft = owner ? new(PodcastDraftState.Ready, 0, [], []) : null,
            Replay = published ? new(PodcastReplayState.Ready, 100) : new(PodcastReplayState.Pending)
        };
        Assert.Equal(expected, PodcastPlayback.Choose(podcast).Source);
    }

    private sealed class ScheduleApi(TokenStore store, string settingsPath)
        : YavscApiClient(new Settings(settingsPath), store), IYavscApiClient
    {
        public PodcastResponse Plan { get; set; } = new(Guid.NewGuid(), "Concert", "owner", "Owner",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, PodcastState.Planned, 0, 0, 0) { FlowId = 1 };
        public bool ObsEnabled { get; private set; }
        public string? RevokedPath { get; private set; }
        public PreparePodcastRequest? Saved { get; set; }
        public string? SavedPath { get; private set; }
        public HttpMethod? SavedMethod { get; private set; }
        public TaskCompletionSource<PodcastObsCredentials>? PendingActivation { get; set; }
        public List<(HttpMethod Method, string Path, object? Body)> DraftCalls { get; } = [];

        public override async Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            object response;
            if (path.EndsWith("/account/me", StringComparison.Ordinal)) response = new Me { UserId = "owner" };
            else if (path.EndsWith("/obs", StringComparison.Ordinal))
            {
                if (method == HttpMethod.Post)
                {
                    ObsEnabled = true;
                    response = PendingActivation is null
                        ? new PodcastObsCredentials("srt://obs.example:10000?passphrase=stream-secret", "", DateTimeOffset.UtcNow.AddMinutes(30))
                        : await PendingActivation.Task;
                }
                else response = new PodcastObsStatus(ObsEnabled, DateTimeOffset.UtcNow.AddMinutes(30), null);
            }
            else if (path.EndsWith("/draft/publish", StringComparison.Ordinal) || path.EndsWith("/originals", StringComparison.Ordinal))
            {
                DraftCalls.Add((method, path, body));
                Plan = method == HttpMethod.Delete
                    ? Plan with { OriginalsDeleted = true, OriginalsExpireAt = null, Draft = null }
                    : Plan with { Draft = Plan.Draft! with { State = PodcastDraftState.Publishing } };
                response = Plan;
            }
            else if (method == HttpMethod.Get) response = new List<PodcastResponse> { Plan };
            else
            {
                Saved = Assert.IsType<PreparePodcastRequest>(body);
                SavedPath = path;
                SavedMethod = method;
                Plan = Plan with
                {
                    Title = Saved.Title, ScheduledStartAt = Saved.ScheduledStartAt,
                    PlannedDurationMinutes = Saved.PlannedDurationMinutes, PublicAnnouncement = Saved.PublicAnnouncement,
                    RefineReplay = Saved.RefineReplay, RetentionDays = Saved.RetentionDays
                };
                response = Plan;
            }
            return (T)response;
        }

        Task IYavscApiClient.CallAsync(HttpMethod method, string path, object? body, CancellationToken ct)
        {
            Assert.Equal(HttpMethod.Delete, method);
            Assert.EndsWith("/obs", path);
            ObsEnabled = false;
            RevokedPath = path;
            return Task.CompletedTask;
        }
    }
}
