using System.Net;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Client;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
public sealed class ResourceUsageReliefPageTests
{
    private static ResourceUsageSummary Usage(string id, decimal consumption, decimal credits = 0m) => new()
    {
        UserId = id, UserName = id + "-name", Consumption = consumption, GrossConsumption = consumption + credits,
        AppliedCredits = credits, ConsumptionLimit = 1000m, IsQuotaReached = consumption >= 1000m,
        RemainingConsumption = Math.Max(0m, 1000m - consumption)
    };

    [AvaloniaTheory]
    [InlineData("Moderator")]
    [InlineData("administrator")]
    [InlineData("Performer")]
    public async Task Nonadministrators_cannot_load_or_erase_or_open_the_page(string role)
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport(role);
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new Transport();
        using var vm = new ResourceUsageReliefPageViewModel(new(transport, "https://example.invalid/api/v1/"), session);
        await vm.InitializeAsync();
        Assert.False(vm.IsAdministrator);
        Assert.Empty(transport.Requests);
        Assert.Empty(vm.Users);
        Assert.False(vm.RefreshCommand.CanExecute(null));
        Assert.False(vm.EraseCommand.CanExecute(null));
        var administration = new AdministrationPageViewModel(null, session: session);
        await administration.OpenResourceUsageReliefCommand.ExecuteAsync(null);
        Assert.Contains("Administrator", administration.StatusMessage);
    }

    [AvaloniaFact]
    public async Task Page_loads_only_exhausted_users_and_confirmed_remission_updates_the_result()
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new Transport();
        using var vm = new ResourceUsageReliefPageViewModel(new(transport, "https://example.invalid/api/v1/"), session);
        await vm.InitializeAsync("alice");
        Assert.Equal(2, vm.Users.Count);
        Assert.Equal("alice", vm.SelectedUsage?.UserId);
        vm.SearchText = "BOB";
        Assert.Equal("bob", Assert.Single(vm.FilteredUsers).UserId);
        vm.SearchText = "";
        vm.PointsText = "125,50";
        Assert.Equal(974.5m, vm.ConsumptionAfter);
        Assert.False(vm.EraseCommand.CanExecute(null));
        await vm.EraseCommand.ExecuteAsync(null);
        Assert.Null(transport.Erasure);
        vm.ConfirmErasure = true;
        Assert.True(vm.EraseCommand.CanExecute(null));
        ResourceUsageSummary? updated = null;
        vm.UsageUpdated += result => updated = result;
        await vm.EraseCommand.ExecuteAsync(null);
        Assert.Equal(125.5m, transport.Erasure?.Points);
        Assert.Equal(20m, transport.Erasure?.ExpectedCredits);
        Assert.Same(updated, vm.SelectedUsage);
        Assert.Equal(974.5m, vm.SelectedUsage!.Consumption);
        Assert.False(vm.SelectedUsage.IsQuotaReached);
        Assert.False(vm.ConfirmErasure);
        Assert.False(vm.EraseCommand.CanExecute(null));
        Assert.Contains("n’est plus atteinte", vm.StatusMessage);
        Assert.Contains("alice", vm.StatusMessage);
        Assert.Contains("GET https://example.invalid/api/v1/resource-usage/admin", transport.Requests);
        Assert.Contains("POST https://example.invalid/api/v1/resource-usage/admin/alice/erase", transport.Requests);
    }

    [AvaloniaTheory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1100.00000001")]
    [InlineData("0.000000001")]
    [InlineData("1,000.00")]
    [InlineData("NaN")]
    [InlineData("1e2")]
    public async Task Invalid_amounts_never_reach_the_API(string amount)
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new Transport();
        using var vm = new ResourceUsageReliefPageViewModel(new(transport, "https://example.invalid/api/v1/"), session);
        await vm.InitializeAsync("alice");
        vm.PointsText = amount;
        vm.ConfirmErasure = true;
        Assert.False(vm.EraseCommand.CanExecute(null));
        Assert.NotEmpty(vm.AmountError);
        await vm.EraseCommand.ExecuteAsync(null);
        Assert.Null(transport.Erasure);
    }

    [AvaloniaFact]
    public async Task Changing_target_or_amount_or_refreshing_invalidates_confirmation()
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new Transport();
        using var vm = new ResourceUsageReliefPageViewModel(new(transport, "https://example.invalid/api/v1/"), session);
        await vm.InitializeAsync("alice");
        vm.PointsText = "10";
        vm.ConfirmErasure = true;
        vm.PointsText = "20";
        Assert.False(vm.ConfirmErasure);
        vm.ConfirmErasure = true;
        vm.SelectedUsage = vm.Users.Single(u => u.UserId == "bob");
        Assert.False(vm.ConfirmErasure);
        Assert.Empty(vm.PointsText);
        vm.PointsText = "20";
        vm.ConfirmErasure = true;
        await vm.RefreshCommand.ExecuteAsync(null);
        Assert.False(vm.ConfirmErasure);
        Assert.Empty(vm.PointsText);
        Assert.Equal("bob", vm.SelectedUsage?.UserId);
    }

    [AvaloniaFact]
    public async Task Failure_requires_a_fresh_snapshot_before_retry()
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new Transport { FailErase = true };
        using var vm = new ResourceUsageReliefPageViewModel(new(transport, "https://example.invalid/api/v1/"), session);
        await vm.InitializeAsync("alice");
        vm.PointsText = "100";
        vm.ConfirmErasure = true;
        await vm.EraseCommand.ExecuteAsync(null);
        Assert.Contains("Actualisez", vm.StatusMessage);
        Assert.False(vm.ConfirmErasure);
        vm.ConfirmErasure = true;
        Assert.False(vm.EraseCommand.CanExecute(null));
        transport.FailErase = false;
        await vm.EraseCommand.ExecuteAsync(null);
        Assert.Single(transport.Requests, r => r.StartsWith("POST"));
        await vm.RefreshCommand.ExecuteAsync(null);
        vm.PointsText = "100";
        vm.ConfirmErasure = true;
        Assert.True(vm.EraseCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public async Task Logout_clears_private_data_and_a_late_response_cannot_restore_it()
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new Transport { Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        using var vm = new ResourceUsageReliefPageViewModel(new(transport, "https://example.invalid/api/v1/"), session);
        var loading = vm.InitializeAsync("alice");
        session.Api = null;
        session.Refresh();
        transport.Pending.SetResult([Usage("alice", 1100m)]);
        await loading;
        Assert.Empty(vm.Users);
        Assert.Null(vm.SelectedUsage);
        Assert.False(vm.IsAdministrator);
        Assert.False(vm.EraseCommand.CanExecute(null));
    }

    [AvaloniaTheory]
    [InlineData(400, 800)]
    [InlineData(800, 400)]
    public async Task Page_displays_target_preview_and_explicit_confirmation_and_disposes_on_exit(int width, int height)
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new Transport();
        using var vm = new ResourceUsageReliefPageViewModel(new(transport, "https://example.invalid/api/v1/"), session);
        await vm.InitializeAsync("alice");
        vm.PointsText = "100";
        var page = new ResourceUsageReliefPage { DataContext = vm };
        var window = new Window { Content = page, Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Compte cible : alice", page.FindControl<TextBlock>("ReliefTarget")!.Text);
            Assert.Contains("1000", page.FindControl<TextBlock>("ReliefPreview")!.Text);
            Assert.Same(vm.EraseCommand, page.FindControl<Button>("ErasePoints")!.Command);
            Assert.False(page.FindControl<Button>("ErasePoints")!.IsEffectivelyEnabled);
            page.FindControl<CheckBox>("ReliefConfirmation")!.IsChecked = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(page.FindControl<Button>("ErasePoints")!.IsEffectivelyEnabled);
            var admin = new AdministrationPage { DataContext = new AdministrationPageViewModel() };
            Assert.NotNull(admin.FindControl<Button>("OpenResourceUsageRelief")!.Command);
        }
        finally { window.Close(); }
        Assert.False(vm.IsAdministrator);
        Assert.Empty(vm.Users);
        Assert.False(vm.ConfirmErasure);
    }

    private sealed class Transport : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public List<string> Requests { get; } = [];
        public EraseResourceUsageRequest? Erasure { get; private set; }
        public bool FailErase { get; set; }
        public TaskCompletionSource<List<ResourceUsageSummary>>? Pending { get; init; }
        public async Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Requests.Add(method + " " + path);
            object result;
            if (method == HttpMethod.Get)
                result = Pending is null ? new List<ResourceUsageSummary>
                    { Usage("alice", 1100m, 20m), Usage("bob", 2000m), Usage("carol", 999m) } : await Pending.Task;
            else
            {
                Erasure = Assert.IsType<EraseResourceUsageRequest>(body);
                if (FailErase) throw new HttpRequestException("Les crédits ont changé.", null, HttpStatusCode.Conflict);
                result = Usage("alice", 1100m - Erasure.Points, 20m + Erasure.Points);
            }
            return (T)result;
        }
        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => throw new NotSupportedException();
        public Task<byte[]> DownloadAsync(HttpMethod method, string path, CancellationToken ct = default)
            => throw new NotSupportedException();
        public ValueTask DisposeAsync() { Http.Dispose(); return ValueTask.CompletedTask; }
    }
}
