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
    public async Task Preparation_and_editing_send_schedule_and_optional_announcement_without_capture()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), $"postit-plan-{Guid.NewGuid():N}.json");
        var store = new TokenStore(tokenPath);
        store.Save(new RefreshTokenRecord("test-access-token", "test-refresh-token", DateTimeOffset.UtcNow.AddHours(1), null));
        try
        {
            await using var api = new ScheduleApi(store);
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
        }
        finally { File.Delete(tokenPath); }
    }

    private sealed class ScheduleApi(TokenStore store) : YavscApiClient(new Settings(), store)
    {
        public PodcastResponse Plan { get; private set; } = new(Guid.NewGuid(), "Concert", "owner", "Owner",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, PodcastState.Planned, 0, 0, 0);
        public PreparePodcastRequest? Saved { get; set; }
        public string? SavedPath { get; private set; }
        public HttpMethod? SavedMethod { get; private set; }

        public override Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            object response;
            if (path.EndsWith("/account/me", StringComparison.Ordinal)) response = new Me { UserId = "owner" };
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
            return Task.FromResult((T)response);
        }
    }
}
