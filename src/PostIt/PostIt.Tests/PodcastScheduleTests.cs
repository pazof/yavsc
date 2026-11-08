using Avalonia.Headless.XUnit;
using PostIt.Services;
using PostIt.ViewModels;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public sealed class PodcastScheduleTests
{
    [AvaloniaFact]
    public async Task Late_obs_credentials_are_not_displayed_after_selection_changes_or_navigation()
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
    }

    [AvaloniaFact]
    public async Task Obs_activation_is_owned_displays_credentials_and_clears_them_on_navigation_and_revocation()
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
    }

    [AvaloniaFact]
    public async Task Preparation_and_editing_send_schedule_and_optional_announcement_without_capture()
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
    }

    private sealed class ScheduleApi(TokenStore store, string settingsPath)
        : YavscApiClient(new Settings(settingsPath), store), IYavscApiClient
    {
        public PodcastResponse Plan { get; private set; } = new(Guid.NewGuid(), "Concert", "owner", "Owner",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, PodcastState.Planned, 0, 0, 0) { FlowId = 1 };
        public bool ObsEnabled { get; private set; }
        public string? RevokedPath { get; private set; }
        public PreparePodcastRequest? Saved { get; set; }
        public string? SavedPath { get; private set; }
        public HttpMethod? SavedMethod { get; private set; }
        public TaskCompletionSource<PodcastObsCredentials>? PendingActivation { get; set; }

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
            else if (method == HttpMethod.Get) response = new List<PodcastResponse> { Plan };
            else
            {
                Saved = Assert.IsType<PreparePodcastRequest>(body);
                SavedPath = path;
                SavedMethod = method;
                Plan = Plan with
                {
                    Title = Saved.Title, ScheduledStartAt = Saved.ScheduledStartAt,
                    PlannedDurationMinutes = Saved.PlannedDurationMinutes, PublicAnnouncement = Saved.PublicAnnouncement
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
