using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using Avalonia.Controls;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Client;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
[Collection(AvaloniaUiCollection.Name)]
public sealed class SubscriptionsPageTests(AvaloniaUiFixture ui)
{
    [Fact]
    public async Task Page_binds_server_offers_prices_and_balance()
    {
        await using var api = new FakeApi();
        var model = Model(api);
        await ui.WithWindowAsync(() => new SubscriptionsPage { DataContext = model }, async window =>
        {
            await model.InitializeAsync();
            var page = Assert.IsType<SubscriptionsPage>(window.Content);
            Assert.IsType<SubscriptionsPage>(new ViewLocator().Build(model));
            var selector = page.FindControl<ComboBox>("OfferSelector")!;
            Assert.Equal(2, selector.ItemCount);
            Assert.Equal(1.2m, model.SelectedOffer?.PriceIncludingTax);
            Assert.Equal($"Prix TTC : {1.2m.ToString("F2", CultureInfo.CurrentCulture)} EUR / mois",
                page.FindControl<TextBlock>("OfferPrice")!.Text);
            selector.SelectedIndex = 1;
            Assert.Equal("Premium", model.SelectedOffer?.Pack);
            Assert.Equal($"Prix TTC : {2.4m.ToString("F2", CultureInfo.CurrentCulture)} EUR / mois",
                page.FindControl<TextBlock>("OfferPrice")!.Text);
            Assert.Equal(45m, model.Usage?.Consumption);
            Assert.Null(model.StatusMessage);
            Assert.True(model.SubscribeCommand.CanExecute(null));
            Assert.False(model.CancelSubscriptionCommand.CanExecute(null));
        }, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Checkout_uses_authority_and_selected_pack_without_granting_local_credit()
    {
        await using var api = new FakeApi();
        Uri? opened = null;
        var authority = "https://first.example/";
        var client = new ResourceSubscriptionsApiClient(api, () => authority);
        var model = new SubscriptionsPageViewModel(client, uri => { opened = uri; return Task.FromResult(true); });
        await model.InitializeAsync();
        authority = "https://second.example/instance";
        model.SelectedOffer = model.Offers[1];
        await model.SubscribeCommand.ExecuteAsync(null);
        Assert.Equal("https://second.example/instance/api/v1/resource-subscriptions", api.LastPath);
        Assert.Equal("Premium", Assert.IsType<CreateResourceSubscriptionRequest>(api.LastBody).Pack);
        Assert.Equal("www.sandbox.paypal.com", opened?.Host);
        Assert.Equal("APPROVAL_PENDING", model.CurrentSubscription?.State);
        Assert.Equal(45m, model.Usage?.Consumption);
        Assert.False(model.IsBusy);
        Assert.Contains("Actualiser", model.StatusMessage);
    }

    [Theory]
    [InlineData("https://attacker.example/approve", true)]
    [InlineData("http://www.paypal.com/approve", true)]
    [InlineData("https://www.paypal.com@attacker.example/approve", true)]
    [InlineData("https://www.paypal.com:444/approve", true)]
    [InlineData("https://www.sandbox.paypal.com/approve", false)]
    public async Task Invalid_link_or_browser_failure_is_explicit_and_checkout_can_be_resumed(string link, bool launchResult)
    {
        await using var api = new FakeApi { ApprovalUrl = link };
        var launches = 0;
        var model = new SubscriptionsPageViewModel(new(api, () => "https://org.example"), _ =>
        {
            launches++;
            return Task.FromResult(launchResult);
        });
        await model.InitializeAsync();
        await model.SubscribeCommand.ExecuteAsync(null);
        Assert.Contains("impossible", model.StatusMessage);
        Assert.Equal(launchResult ? 0 : 1, launches);
        Assert.True(model.SubscribeCommand.CanExecute(null));
        Assert.Equal(45m, model.Usage?.Consumption);
    }

    [Fact]
    public async Task Active_subscription_blocks_duplicate_checkout_and_cancel_requires_confirmation()
    {
        await using var api = new FakeApi { State = "ACTIVE" };
        var model = Model(api);
        await model.InitializeAsync();
        Assert.False(model.SubscribeCommand.CanExecute(null));
        await model.CancelSubscriptionCommand.ExecuteAsync(null);
        Assert.Contains("confirmation", model.StatusMessage);
        Assert.Equal(HttpMethod.Get, api.LastMethod);
        model.ConfirmCancellation = true;
        await model.CancelSubscriptionCommand.ExecuteAsync(null);
        Assert.EndsWith($"/{api.Id}/cancel", api.LastPath);
        Assert.Null(model.CurrentSubscription);
        Assert.False(model.ConfirmCancellation);
        Assert.True(model.SubscribeCommand.CanExecute(null));
    }

    [Fact]
    public async Task Refresh_uses_server_reconciliation_and_reports_failures_without_fake_success()
    {
        await using var api = new FakeApi();
        var model = Model(api);
        await model.InitializeAsync();
        api.Fail = true;
        await model.RefreshCommand.ExecuteAsync(null);
        Assert.Contains("offline", model.StatusMessage);
        Assert.False(model.IsBusy);
        Assert.Equal(45m, model.Usage?.Consumption);
        api.Fail = false;
        await model.RefreshCommand.ExecuteAsync(null);
        Assert.EndsWith("/refresh", api.LastPath);
        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Equal(0m, model.Usage?.Consumption);
    }

    [Fact]
    public async Task Unavailable_offers_and_changing_an_existing_pack_cannot_start_checkout()
    {
        await using var api = new FakeApi { State = "APPROVAL_PENDING" };
        var model = Model(api);
        await model.InitializeAsync();
        Assert.False(model.SubscribeCommand.CanExecute(null));
        model.SelectedOffer = model.Offers[1];
        Assert.True(model.SubscribeCommand.CanExecute(null));
        model.SelectedOffer = model.SelectedOffer with { Available = false };
        Assert.False(model.SubscribeCommand.CanExecute(null));
    }

    private static SubscriptionsPageViewModel Model(FakeApi api)
        => new(new(api, () => "https://org.example"), _ => Task.FromResult(true));

    private sealed class FakeApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public Guid Id { get; } = Guid.NewGuid();
        public string? State { get; set; }
        public string ApprovalUrl { get; set; } = "https://www.sandbox.paypal.com/approve";
        public bool Fail { get; set; }
        public HttpMethod? LastMethod { get; private set; }
        public string? LastPath { get; private set; }
        public object? LastBody { get; private set; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastMethod = method;
            LastPath = path;
            LastBody = body;
            if (Fail) throw new HttpRequestException("offline");
            var pack = (body as CreateResourceSubscriptionRequest)?.Pack ?? "Premium";
            var subscription = new ResourceSubscriptionResponse(Id, pack, State ?? "APPROVAL_PENDING", 2.4m, 3000m, null);
            object result;
            if (body is CreateResourceSubscriptionRequest)
                result = new ResourceSubscriptionCheckoutResponse(subscription, ApprovalUrl);
            else
            {
                if (path.EndsWith("/cancel")) State = null;
                result = new ResourceSubscriptionsResponse(
                    [new("Discovery", "Découverte", 1m, 20m, 1.2m, 1000m, 2, true),
                     new("Premium", "Premium", 2m, 20m, 2.4m, 3000m, 5, true)],
                    State is null ? [] : [subscription],
                    new ResourceUsageSummary { Consumption = path.EndsWith("/refresh") ? 0m : 45m });
            }
            return Task.FromResult(JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(result))!);
        }

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> factory, CancellationToken ct = default) => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> factory, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<byte[]> DownloadAsync(HttpMethod method, string path, CancellationToken ct = default) => throw new NotSupportedException();
        public ValueTask DisposeAsync() { Http.Dispose(); return ValueTask.CompletedTask; }
    }
}
