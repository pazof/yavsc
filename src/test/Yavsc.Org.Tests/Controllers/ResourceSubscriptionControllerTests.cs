using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yavsc.Abstract.Resources;
using Yavsc.Tests.Shared;
using Yavsc.Models;
using Yavsc.Models.Payment;
using Yavsc.Server.Helpers;
using Yavsc.Settings;
using Yavsc.Web.Services;
using Xunit;

namespace Yavsc.Org.Tests.Controllers;

public sealed class ResourceSubscriptionControllerTests : IClassFixture<ResourceSubscriptionControllerTests.Factory>
{
    private readonly Factory _factory;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ResourceSubscriptionControllerTests(Factory factory)
    {
        _factory = factory;
        factory.PayPal.Payments.Clear();
        factory.PayPal.Verified = true;
        factory.PayPal.CancelCount = 0;
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.ResourceSubscriptionPayments.RemoveRange(db.ResourceSubscriptionPayments);
        db.ResourceSubscriptions.RemoveRange(db.ResourceSubscriptions);
        db.ResourceUsageRecords.RemoveRange(db.ResourceUsageRecords);
        if (!db.Users.Any(u => u.Id == "listener"))
            db.Users.Add(new ApplicationUser { Id = "listener", UserName = "Listener", Email = "listener@example.test" });
        if (!db.Users.Any(u => u.Id == "author"))
            db.Users.Add(new ApplicationUser { Id = "author", UserName = "Author", Email = "author@example.test" });
        db.SaveChanges();
    }

    [Fact]
    public async Task Withdrawal_requires_antiforgery_and_cannot_target_another_account()
    {
        using var client = _factory.SignedInClient();
        using var missingToken = await client.PostAsync("/Account/Delete", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["UserId"] = "listener" }), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, missingToken.StatusCode);
        using var tampered = await PostAsync(client, "/Account/Delete", new() { ["UserId"] = "author" });
        Assert.Equal(HttpStatusCode.Redirect, tampered.StatusCode);
        Assert.Contains("AccessDenied", tampered.Headers.Location?.ToString());
        using var scope = _factory.Services.CreateScope();
        Assert.Null((await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Users.SingleAsync(u => u.Id == "author", Ct)).WithdrawalRequestedAt);
    }

    [Fact]
    public async Task Subscription_management_requires_login_and_all_mutations_require_antiforgery()
    {
        using var anonymous = _factory.CreateClient(new() { AllowAutoRedirect = false });
        using var page = await anonymous.GetAsync("/ResourceSubscription", Ct);
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);
        using var client = _factory.SignedInClient();
        using var create = await client.PostAsync("/ResourceSubscription/Create", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["pack"] = "Discovery" }), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);
    }

    [Fact]
    public async Task Native_api_requires_bearer_and_does_not_accept_a_web_cookie()
    {
        using var anonymous = _factory.CreateClient(new() { AllowAutoRedirect = false });
        using var unsigned = await anonymous.GetAsync("/api/v1/resource-subscriptions", Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, unsigned.StatusCode);
        using var cookie = _factory.SignedInClient();
        using var response = await cookie.PostAsJsonAsync("/api/v1/resource-subscriptions",
            new { pack = "Premium" }, Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Native_checkout_reuses_subscription_ignores_client_prices_and_requires_verified_payment()
    {
        using (var scope = _factory.Services.CreateScope())
            await ResourceUsageTracker.RecordAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                "listener", "Listener", cpuSeconds: 12000m, cancellationToken: Ct);
        using var client = NativeClient();
        var catalogue = await client.GetFromJsonAsync<ResourceSubscriptionsResponse>("/api/v1/resource-subscriptions", Ct);
        Assert.Equal(1.2m, catalogue!.Offers[0].PriceIncludingTax);
        Assert.Equal(2.4m, catalogue.Offers[1].PriceIncludingTax);
        Assert.True(catalogue.Usage.IsQuotaReached);
        using var created = await client.PostAsJsonAsync("/api/v1/resource-subscriptions",
            new { pack = "Discovery", priceIncludingTax = 0.01m, userId = "author" }, Ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        Assert.True(created.Headers.CacheControl?.NoStore);
        var checkout = (await created.Content.ReadFromJsonAsync<ResourceSubscriptionCheckoutResponse>(Ct))!;
        Assert.Equal("https://www.sandbox.paypal.com/approve", checkout.ApprovalUrl);
        Assert.Equal(1.2m, checkout.Subscription.PriceIncludingTax);
        Assert.Equal(1200m, (await UsageAsync()).Consumption);
        using var resumed = await client.PostAsJsonAsync("/api/v1/resource-subscriptions", new { pack = "Discovery" }, Ct);
        Assert.Equal(checkout.Subscription.Id,
            (await resumed.Content.ReadFromJsonAsync<ResourceSubscriptionCheckoutResponse>(Ct))!.Subscription.Id);
        using var conflict = await client.PostAsJsonAsync("/api/v1/resource-subscriptions", new { pack = "Premium" }, Ct);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        using var anonymous = _factory.CreateClient();
        using var returned = await anonymous.GetAsync("/api/v1/resource-subscriptions/return?success=true", Ct);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
        Assert.Equal("text/html", returned.Content.Headers.ContentType?.MediaType);
        Assert.Equal(1200m, (await UsageAsync()).Consumption);
        using var other = NativeClient("author");
        using var forbidden = await other.PostAsync($"/api/v1/resource-subscriptions/{checkout.Subscription.Id}/cancel", null, Ct);
        Assert.Equal(HttpStatusCode.NotFound, forbidden.StatusCode);
        Assert.Empty((await other.GetFromJsonAsync<ResourceSubscriptionsResponse>("/api/v1/resource-subscriptions", Ct))!.Subscriptions);
        _factory.PayPal.Payments.Add(new("PAY-NATIVE", "COMPLETED", 1.2m, "EUR", DateTimeOffset.UtcNow));
        using var refreshed = await client.PostAsync("/api/v1/resource-subscriptions/refresh", null, Ct);
        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);
        Assert.Equal(200m, (await UsageAsync()).Consumption);
        using var repeated = await client.PostAsync("/api/v1/resource-subscriptions/refresh", null, Ct);
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        Assert.Equal(200m, (await UsageAsync()).Consumption);
        using var cancelled = await client.PostAsync($"/api/v1/resource-subscriptions/{checkout.Subscription.Id}/cancel", null, Ct);
        Assert.Equal(HttpStatusCode.OK, cancelled.StatusCode);
        Assert.Equal("CANCELLED", (await cancelled.Content.ReadFromJsonAsync<ResourceSubscriptionsResponse>(Ct))!.Subscriptions.Single().State);
        Assert.Equal(1, _factory.PayPal.CancelCount);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"pack":"Other"}""")]
    [InlineData("""{"pack":null}""")]
    public async Task Native_checkout_rejects_invalid_packs(string json)
    {
        using var client = NativeClient();
        using var response = await client.PostAsync("/api/v1/resource-subscriptions",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = _factory.Services.CreateScope();
        Assert.Empty(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().ResourceSubscriptions);
    }

    private HttpClient NativeClient(string userId = "listener")
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokenIssuer.Issue(userId, "api"));
        return client;
    }

    [Fact]
    public async Task Native_catalogue_uses_instance_prices_and_reports_unconfigured_offers_explicitly()
    {
        var options = _factory.Services.GetRequiredService<IOptions<ResourceSubscriptionOptions>>().Value;
        var originalPrice = options.DiscoveryPriceExcludingTax;
        var originalPlan = options.DiscoveryPlanId;
        try
        {
            options.DiscoveryPriceExcludingTax = 3m;
            options.DiscoveryPlanId = "";
            using var client = NativeClient();
            var catalogue = (await client.GetFromJsonAsync<ResourceSubscriptionsResponse>("/api/v1/resource-subscriptions", Ct))!;
            Assert.Equal(3.6m, catalogue.Offers[0].PriceIncludingTax);
            Assert.False(catalogue.Offers[0].Available);
            using var response = await client.PostAsJsonAsync("/api/v1/resource-subscriptions", new { pack = "Discovery" }, Ct);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }
        finally
        {
            options.DiscoveryPriceExcludingTax = originalPrice;
            options.DiscoveryPlanId = originalPlan;
        }
    }

    [Fact]
    public async Task Native_routes_are_described_in_the_existing_openapi_document()
    {
        using var client = NativeClient();
        using var document = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json", Ct));
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.GetProperty("/api/v1/resource-subscriptions").TryGetProperty("get", out _));
        Assert.True(paths.GetProperty("/api/v1/resource-subscriptions").TryGetProperty("post", out _));
        Assert.True(paths.GetProperty("/api/v1/resource-subscriptions/refresh").TryGetProperty("post", out _));
        Assert.True(paths.GetProperty("/api/v1/resource-subscriptions/{id}/cancel").TryGetProperty("post", out _));
    }

    [Fact]
    public async Task Exhausted_free_usage_can_still_subscribe_and_browser_return_does_not_award_unpaid_credit()
    {
        using (var scope = _factory.Services.CreateScope())
            await ResourceUsageTracker.RecordAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                "listener", "Listener", cpuSeconds: 12000m, cancellationToken: Ct);
        using var client = _factory.SignedInClient();
        var html = await client.GetStringAsync("/ResourceSubscription", Ct);
        Assert.Contains("1200", html);
        Assert.Contains("Premium", html);
        var subscription = await StartAsync(client);
        Assert.Equal("listener", subscription.UserId);
        Assert.Equal(1000m, subscription.MonthlyPoints);
        Assert.Equal(1.2m, subscription.PriceIncludingTax);
        using var returned = await client.GetAsync($"/ResourceSubscription/Return?id={subscription.Id}", Ct);
        Assert.Equal(HttpStatusCode.Redirect, returned.StatusCode);
        var usage = await UsageAsync();
        Assert.Equal(1200m, usage.Consumption);
        Assert.True(usage.IsQuotaReached);
    }

    [Fact]
    public async Task Verified_notification_reconciles_payment_once_and_unblocks_without_a_browser_return()
    {
        using (var scope = _factory.Services.CreateScope())
            await ResourceUsageTracker.RecordAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                "listener", "Listener", cpuSeconds: 12000m, cancellationToken: Ct);
        using var client = _factory.SignedInClient();
        var subscription = await StartAsync(client);
        _factory.PayPal.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", subscription.CreatedAt));
        using var first = await NotifyAsync(subscription.PayPalId!);
        using var repeated = await NotifyAsync(subscription.PayPalId!);
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
        var usage = await UsageAsync();
        Assert.Equal(200m, usage.Consumption);
        Assert.False(usage.IsQuotaReached);
        using var verificationScope = _factory.Services.CreateScope();
        Assert.Single(verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().ResourceSubscriptionPayments);
    }

    [Fact]
    public async Task Premium_checkout_uses_three_quotas_and_its_own_vat_inclusive_price()
    {
        using var client = _factory.SignedInClient();
        using var response = await PostAsync(client, "/ResourceSubscription/Create", new() { ["pack"] = "Premium" });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var subscription = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().ResourceSubscriptions.SingleAsync(Ct);
        Assert.Equal("P-PREMIUM", subscription.PlanId);
        Assert.Equal(3000m, subscription.MonthlyPoints);
        Assert.Equal(2.4m, subscription.PriceIncludingTax);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("""{"event_type":"PAYMENT.SALE.COMPLETED","resource":[]}""")]
    [InlineData("""{"event_type":"PAYMENT.SALE.REFUNDED","resource":{"sale_id":42}}""")]
    [InlineData("""{"event_type":"PAYMENT.SALE.REFUNDED","resource":{"links":{}}}""")]
    [InlineData("""{"event_type":"PAYMENT.SALE.REFUNDED","resource":{"links":[{"href":42}]}}""")]
    public async Task Malformed_notification_shapes_are_rejected_without_credit(string json)
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/ResourceSubscription/Notify",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0m, (await UsageAsync()).CreditBalance);
    }

    [Fact]
    public async Task Forged_notification_does_not_reconcile_or_grant_a_credit()
    {
        using var client = _factory.SignedInClient();
        var subscription = await StartAsync(client);
        _factory.PayPal.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", subscription.CreatedAt));
        _factory.PayPal.Verified = false;
        using var notified = await NotifyAsync(subscription.PayPalId!);
        Assert.Equal(HttpStatusCode.BadRequest, notified.StatusCode);
        Assert.Equal(0m, (await UsageAsync()).CreditBalance);
    }

    [Fact]
    public async Task Cancellation_is_owned_post_only_and_retains_acquired_credit()
    {
        using var client = _factory.SignedInClient();
        var subscription = await StartAsync(client);
        _factory.PayPal.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", subscription.CreatedAt));
        using var notified = await NotifyAsync(subscription.PayPalId!);
        Assert.Equal(HttpStatusCode.NoContent, notified.StatusCode);
        using var other = _factory.SignedInClient("author");
        using var refused = await PostAsync(other, "/ResourceSubscription/Cancel", new() { ["id"] = subscription.Id.ToString() });
        Assert.Equal(HttpStatusCode.NotFound, refused.StatusCode);
        Assert.Equal(0, _factory.PayPal.CancelCount);
        using var get = await client.GetAsync($"/ResourceSubscription/Cancel?id={subscription.Id}", Ct);
        Assert.NotEqual(HttpStatusCode.OK, get.StatusCode);
        using var cancelled = await PostAsync(client, "/ResourceSubscription/Cancel", new() { ["id"] = subscription.Id.ToString() });
        Assert.Equal(HttpStatusCode.Redirect, cancelled.StatusCode);
        Assert.Equal(1, _factory.PayPal.CancelCount);
        Assert.Equal(666.66666666m, (await UsageAsync()).CreditBalance);
    }

    [Fact]
    public async Task Refund_notification_finds_the_original_receipt_and_removes_its_credit()
    {
        using var client = _factory.SignedInClient();
        var subscription = await StartAsync(client);
        _factory.PayPal.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", subscription.CreatedAt));
        using var first = await NotifyAsync(subscription.PayPalId!);
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        _factory.PayPal.Payments[0] = _factory.PayPal.Payments[0] with { State = "REFUNDED" };
        using var notificationClient = _factory.CreateClient();
        using var refund = await notificationClient.PostAsJsonAsync("/ResourceSubscription/Notify",
            new { event_type = "PAYMENT.SALE.REFUNDED", resource = new { sale_id = "PAY-1", id = "REFUND-1" } }, Ct);
        Assert.Equal(HttpStatusCode.NoContent, refund.StatusCode);
        Assert.Equal(0m, (await UsageAsync()).CreditBalance);
    }

    private async Task<ResourceSubscription> StartAsync(HttpClient client)
    {
        using var response = await PostAsync(client, "/ResourceSubscription/Create", new()
        {
            ["pack"] = "Discovery", ["UserId"] = "author", ["Amount"] = "0.01", ["MonthlyPoints"] = "999999"
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("https://www.sandbox.paypal.com/", response.Headers.Location?.ToString());
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().ResourceSubscriptions.AsNoTracking().SingleAsync(Ct);
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string action, Dictionary<string, string> fields)
    {
        var html = await client.GetStringAsync("/ResourceSubscription", Ct);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success);
        fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value);
        return await client.PostAsync(action, new FormUrlEncodedContent(fields), Ct);
    }

    private async Task<Yavsc.Abstract.Resources.ResourceUsageSummary> UsageAsync()
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IResourceUsageService>().GetCurrentAsync("listener", "Listener", Ct);
    }

    private async Task<HttpResponseMessage> NotifyAsync(string paypalId)
    {
        using var client = _factory.CreateClient();
        return await client.PostAsJsonAsync("/ResourceSubscription/Notify",
            new { event_type = "PAYMENT.SALE.COMPLETED", resource = new { billing_agreement_id = paypalId } }, Ct);
    }

    public sealed class Factory : PodcastWebTests.Factory
    {
        public Gateway PayPal { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>("Bearer", o =>
                {
                    o.Authority = null;
                    o.MetadataAddress = "";
                    o.ConfigurationManager = null;
                    o.Configuration = new OpenIdConnectConfiguration { Issuer = TestTokenIssuer.Issuer };
                    o.MapInboundClaims = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, ValidIssuer = TestTokenIssuer.Issuer,
                        ValidateAudience = false, ValidateLifetime = true,
                        ValidateIssuerSigningKey = true, IssuerSigningKey = TestTokenIssuer.SigningKey,
                        NameClaimType = "name"
                    };
                });
                services.AddSingleton<IPayPalSubscriptionGateway>(PayPal);
                services.Configure<ResourceSubscriptionOptions>(o =>
                {
                    o.DiscoveryPlanId = "P-DISCOVERY";
                    o.PremiumPlanId = "P-PREMIUM";
                    o.WebhookId = "WH-TEST";
                });
            });
        }
    }

    public sealed class Gateway : IPayPalSubscriptionGateway
    {
        public bool Verified { get; set; } = true;
        public int CancelCount { get; set; }
        public List<PayPalSubscriptionTransaction> Payments { get; } = [];
        private PayPalSubscriptionDetails _remote = null!;
        public Task<PayPalSubscriptionDetails> CreateAsync(ResourceSubscription subscription, string returnUrl, string cancelUrl, CancellationToken ct)
        {
            _remote = new("I-TEST", subscription.Id.ToString("N"), subscription.PlanId, "APPROVAL_PENDING",
                subscription.CreatedAt, subscription.CreatedAt.AddMonths(1), "https://www.sandbox.paypal.com/approve");
            return Task.FromResult(_remote);
        }
        public Task<PayPalSubscriptionDetails> GetAsync(string id, CancellationToken ct) => Task.FromResult(_remote);
        public Task<IReadOnlyList<PayPalSubscriptionTransaction>> TransactionsAsync(string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<PayPalSubscriptionTransaction>>(Payments.ToArray());
        public Task CancelAsync(string id, CancellationToken ct)
        {
            CancelCount++;
            _remote = _remote with { State = "CANCELLED", NextBillingAt = null };
            return Task.CompletedTask;
        }
        public Task VerifyWebhookAsync(JsonElement notification, IReadOnlyDictionary<string, string> headers, CancellationToken ct)
            => Verified ? Task.CompletedTask : throw new SubscriptionOperationException(400, "Signature invalide.");
    }
}
