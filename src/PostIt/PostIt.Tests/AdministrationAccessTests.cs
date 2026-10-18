using System.Net.Http;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class AdministrationAccessTests
{
    [AvaloniaTheory]
    [InlineData("Moderator", false)]
    [InlineData("Administrator", true)]
    [InlineData("administrator", false)]
    [InlineData("Performer", false)]
    [InlineData("moderator", false)]
    [InlineData("", false)]
    public async Task Home_administration_visibility_and_command_follow_the_administrator_role(string role, bool allowed)
    {
        await using var api = new ProfileTransport(role);
        var session = new SessionStatusViewModel { Api = api };
        var vm = new HomePageViewModel(api, api.Settings, session, null!);
        var page = new HomePage { DataContext = vm };
        var window = new Window { Content = page };
        try
        {
            window.Show();
            var button = Assert.Single(page.GetVisualDescendants().OfType<Button>(),
                button => Equals(button.Content, "Administration"));
            Assert.False(button.IsVisible);
            await session.RefreshAsync();
            Assert.Equal(allowed, session.IsAdministrator);
            Assert.Equal(allowed, button.IsVisible);
            Assert.Equal(allowed, vm.OpenAdministration.CanExecute(null));
            Assert.DoesNotContain(page.GetVisualDescendants().OfType<Button>(),
                button => Equals(button.Content, "Mon usage / administration"));

            session.Api = null;
            session.Refresh();
            Assert.False(button.IsVisible);
            Assert.False(vm.OpenAdministration.CanExecute(null));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public async Task Late_role_response_cannot_restore_access_after_logout()
    {
        await using var api = new ProfileTransport("Administrator");
        var pending = new TaskCompletionSource<Me>();
        api.PendingProfile = pending.Task;
        var session = new SessionStatusViewModel { Api = api };
        var refresh = session.RefreshAsync();
        session.Api = null;
        session.Refresh();
        pending.SetResult(new Me { Roles = ["Administrator"] });
        await refresh;
        Assert.False(session.IsAdministrator);
    }

    [Fact]
    public async Task Role_lookup_failure_denies_access_and_reports_the_error()
    {
        await using var api = new ProfileTransport("Administrator") { FailProfile = true };
        var session = new SessionStatusViewModel { Api = api };
        await session.RefreshAsync();
        Assert.False(session.IsAdministrator);
        Assert.Contains("Impossible de vérifier les droits", session.SessionLabel);
    }

    [Fact]
    public async Task Administration_loads_users_and_usage_and_matches_by_id()
    {
        await using var api = new ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = api };
        await session.RefreshAsync();
        var vm = new AdministrationPageViewModel(api, api.UsageClient, session);
        await vm.RefreshAsync();
        vm.SelectedUser = Assert.Single(vm.Users);
        Assert.Equal("alice", vm.SelectedUsage?.UserName);
        Assert.Equal(12m, vm.SelectedUsage?.ApiCalls);
        Assert.Single(vm.UsageOverview);
        Assert.Contains(api.Requests, path => path.EndsWith("/resource-usage/admin"));
        Assert.DoesNotContain(api.Requests, path => path.EndsWith("/resource-usage/me"));
    }

    [AvaloniaFact]
    public async Task Administration_displays_both_tabs_and_selected_user_usage()
    {
        await using var api = new ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = api };
        await session.RefreshAsync();
        var vm = new AdministrationPageViewModel(api, api.UsageClient, session);
        await vm.RefreshAsync();
        vm.SelectedUser = Assert.Single(vm.Users);
        var page = new AdministrationPage { DataContext = vm };
        var window = new Window { Content = page, Width = 800, Height = 600 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var tabs = Assert.Single(page.GetVisualDescendants().OfType<TabControl>());
            Assert.Equal(7, tabs.Items.Count);
            Assert.Contains(page.GetVisualDescendants().OfType<TextBlock>(),
                text => text.Text == "Appels API : 12");
            tabs.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(page.GetVisualDescendants().OfType<TextBlock>(),
                text => text.Text == "alice-id");
            Assert.Contains(page.GetVisualDescendants().OfType<TextBlock>(),
                text => text.Text == "Appels API : 12");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public async Task Administration_denies_moderators_without_requesting_private_data()
    {
        await using var api = new ProfileTransport("Moderator");
        var session = new SessionStatusViewModel { Api = api };
        await session.RefreshAsync();
        api.Requests.Clear();
        var vm = new AdministrationPageViewModel(api, api.UsageClient, session);
        await vm.RefreshAsync();
        Assert.Empty(api.Requests);
        Assert.Empty(vm.Users);
        Assert.Contains("Administrator", vm.StatusMessage);
    }

    [Fact]
    public async Task Administration_failure_reports_error_without_falling_back_to_personal_usage()
    {
        await using var api = new ProfileTransport("Administrator") { FailUsage = true };
        var session = new SessionStatusViewModel { Api = api };
        await session.RefreshAsync();
        var vm = new AdministrationPageViewModel(api, api.UsageClient, session);
        await vm.RefreshAsync();
        Assert.Empty(vm.Users);
        Assert.Empty(vm.UsageOverview);
        Assert.Contains("indisponible", vm.StatusMessage);
        Assert.DoesNotContain(api.Requests, path => path.EndsWith("/resource-usage/me"));
    }

    [Fact]
    public async Task My_profile_loads_own_profile_enables_saving_and_only_loads_personal_usage()
    {
        await using var api = new ProfileTransport("Performer");
        var vm = new UserProfilePageViewModel(api, usageClient: api.UsageClient);
        await vm.RefreshAsync();
        Assert.True(vm.IsProfileLoaded);
        Assert.Equal(12m, vm.ResourceUsage?.ApiCalls);
        Assert.True(vm.SaveProfileCommand.CanExecute(null));
        vm.FullName = "Alice Updated";
        Assert.Equal("Alice Updated", vm.BuildProfileUpdateRequest().FullName);
        Assert.DoesNotContain(api.Requests, path => path.EndsWith("/resource-usage/admin"));
    }

    [Fact]
    public async Task Usage_failure_does_not_disable_profile_management()
    {
        await using var api = new ProfileTransport("Performer") { FailUsage = true };
        var vm = new UserProfilePageViewModel(api, usageClient: api.UsageClient);
        await vm.RefreshAsync();
        Assert.True(vm.IsProfileLoaded);
        Assert.True(vm.SaveProfileCommand.CanExecute(null));
        Assert.Null(vm.ResourceUsage);
        Assert.Contains("Impossible de charger mon usage", vm.UsageError);
    }

    internal sealed class ProfileTransport : YavscApiClient
    {
        private readonly string _tokenPath;
        private readonly string _role;
        public List<string> Requests { get; } = [];
        public Task<Me>? PendingProfile { get; set; }
        public bool FailProfile { get; set; }
        public bool FailUsage { get; set; }
        public ResourceUsageApiClient UsageClient => new(this, Settings.ApiUrl);

        public ProfileTransport(string role)
            : this(role, Path.Combine(Path.GetTempPath(), $"postit-admin-{Guid.NewGuid():N}.json")) { }

        private ProfileTransport(string role, string path) : base(new Settings(), CreateStore(path))
        {
            _role = role;
            _tokenPath = path;
        }

        private static TokenStore CreateStore(string path)
        {
            var store = new TokenStore(path);
            store.Save(new RefreshTokenRecord("access", "refresh", DateTimeOffset.UtcNow.AddHours(1), null));
            return store;
        }

        public override async Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null,
            CancellationToken ct = default)
        {
            Requests.Add(path);
            object result;
            if (path.EndsWith("/account/me"))
            {
                if (FailProfile)
                    throw new HttpRequestException("Profile unavailable");
                result = PendingProfile is not null ? await PendingProfile
                    : new Me { UserId = "alice-id", UserName = "alice", Roles = [_role] };
            }
            else if (path.Contains("/resource-usage/"))
            {
                if (FailUsage)
                    throw new HttpRequestException("Usage unavailable");
                var usage = new ResourceUsageSummary { UserId = "alice-id", UserName = "alice", ApiCalls = 12m };
                result = path.EndsWith("/admin") ? new List<ResourceUsageSummary> { usage } : usage;
            }
            else if (path.EndsWith("/users"))
                result = new List<UserInfo> { new() { UserId = "alice-id", UserName = "alice" } };
            else
                throw new InvalidOperationException($"Unexpected endpoint: {path}");

            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(result))!;
        }

        public new async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            File.Delete(_tokenPath);
        }
    }
}
