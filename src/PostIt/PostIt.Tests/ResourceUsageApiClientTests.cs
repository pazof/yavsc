using System.Net.Http;
using System.Text.Json;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Client;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
public class ResourceUsageApiClientTests
{
    [Fact]
    public async Task Erase_sends_amount_and_expected_credits_to_escaped_user_endpoint()
    {
        var fake = new FakeYavscApiClient();
        var client = new ResourceUsageApiClient(fake, "https://example.invalid/api/v1/");
        var request = new EraseResourceUsageRequest { Points = 125.5m, ExpectedCredits = 12m };
        await client.EraseAsync("user/name ?#", request, TestContext.Current.CancellationToken);
        Assert.Equal("POST", fake.LastMethod);
        Assert.Equal("https://example.invalid/api/v1/resource-usage/admin/user%2Fname%20%3F%23/erase", fake.LastPath);
        Assert.Same(request, fake.LastBody);
        Assert.Equal(TestContext.Current.CancellationToken, fake.LastToken);
    }

    [Fact]
    public async Task Usage_remains_visible_when_the_rest_of_the_profile_is_unavailable()
    {
        var usage = new ResourceUsageApiClient(new FakeYavscApiClient(), "https://example.invalid/api/v1/");
        var model = new PostIt.ViewModels.UserProfilePageViewModel(null, usageClient: usage);
        await model.RefreshAsync();
        Assert.NotNull(model.ResourceUsage);
        Assert.Equal(20.65m, model.ResourceUsage.Consumption);
        Assert.Null(model.UsageError);
        Assert.False(model.IsProfileLoaded);
        Assert.False(model.IsBusy);
    }
    [Fact]
    public async Task GetCurrentAsync_calls_user_usage_endpoint()
    {
        var fake = new FakeYavscApiClient();
        var client = new ResourceUsageApiClient(fake, "https://example.invalid/api/v1/");

        var usage = await client.GetCurrentAsync();

        Assert.Equal("https://example.invalid/api/v1/resource-usage/me", fake.LastPath);
        Assert.Equal("GET", fake.LastMethod);
        Assert.Equal(12.5m, usage.ApiCalls);
        Assert.Equal(3.2m, usage.CpuSeconds);
        Assert.Equal(20.65m, usage.Consumption);
        Assert.Equal(10000m, usage.ConsumptionLimit);
        Assert.Equal(9979.35m, usage.RemainingConsumption);
        Assert.False(usage.IsQuotaReached);
    }

    [Fact]
    public async Task GetAdminOverviewAsync_calls_admin_usage_endpoint()
    {
        var fake = new FakeYavscApiClient();
        var client = new ResourceUsageApiClient(fake, "https://example.invalid/api/v1/");

        var usage = await client.GetAdminOverviewAsync();

        Assert.Equal("https://example.invalid/api/v1/resource-usage/admin", fake.LastPath);
        Assert.Equal("GET", fake.LastMethod);
        Assert.Equal(2, usage.Count);
        Assert.Equal("administrator", usage[0].UserName);
    }

    private sealed class FakeYavscApiClient : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public string? LastPath { get; private set; }
        public string? LastMethod { get; private set; }
        public object? LastBody { get; private set; }
        public CancellationToken LastToken { get; private set; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastMethod = method.Method;
            LastPath = path;
            LastBody = body;
            LastToken = ct;

            if (typeof(T) == typeof(ResourceUsageSummary))
            {
                var summary = new ResourceUsageSummary
                {
                    UserId = "user-42",
                    UserName = "alice",
                    ApiCalls = 12.5m,
                    CpuSeconds = 3.2m,
                    BandwidthMb = 18.4m,
                    StorageMb = 51.2m,
                    Consumption = 20.65m,
                    ConsumptionLimit = 10000m,
                    RemainingConsumption = 9979.35m,
                    EstimatedCompensation = 4.9m,
                    Currency = "EUR",
                    RecordedAtUtc = DateTimeOffset.UtcNow
                };

                return Task.FromResult((T)(object)summary);
            }

            if (typeof(T) == typeof(List<ResourceUsageSummary>))
            {
                var list = new List<ResourceUsageSummary>
                {
                    new()
                    {
                        UserId = "admin-user",
                        UserName = "administrator",
                        ApiCalls = 90m,
                        CpuSeconds = 14m,
                        BandwidthMb = 500m,
                        StorageMb = 2048m,
                        EstimatedCompensation = 32m,
                        Currency = "EUR",
                        RecordedAtUtc = DateTimeOffset.UtcNow
                    },
                    new()
                    {
                        UserId = "user-99",
                        UserName = "bob",
                        ApiCalls = 40m,
                        CpuSeconds = 7m,
                        BandwidthMb = 120m,
                        StorageMb = 600m,
                        EstimatedCompensation = 15m,
                        Currency = "EUR",
                        RecordedAtUtc = DateTimeOffset.UtcNow
                    }
                };

                return Task.FromResult((T)(object)list);
            }

            throw new NotSupportedException($"Unsupported type: {typeof(T).FullName}");
        }

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<byte[]> DownloadAsync(HttpMethod method, string path, CancellationToken ct = default)
            => Task.FromResult(Array.Empty<byte>());

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
