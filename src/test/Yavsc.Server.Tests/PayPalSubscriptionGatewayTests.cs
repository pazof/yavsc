using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Yavsc.Models.Payment;
using Yavsc.Settings;
using Yavsc.Web.Services;

namespace Yavsc.Server.Tests;

public sealed class PayPalSubscriptionGatewayTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private sealed class Handler : HttpMessageHandler
    {
        public string PlanCurrency { get; set; } = "EUR";
        public string PlanPrice { get; set; } = "1.20";
        public string Frequency { get; set; } = "MONTH";
        public bool AutoBillOutstanding { get; set; }
        public string? RawResponse { get; set; }
        public string Verification { get; set; } = "SUCCESS";
        public string ApprovalUrl { get; set; } = "https://www.sandbox.paypal.com/webapps/billing/subscriptions?ba_token=test";
        public string CustomId { get; set; } = "";
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public List<(string Path, string Body, string? RequestId, string Host)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var path = request.RequestUri!.AbsolutePath;
            var body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct);
            Requests.Add((path, body, request.Headers.TryGetValues("PayPal-Request-Id", out var ids) ? ids.Single() : null, request.RequestUri.Host));
            if (Status != HttpStatusCode.OK) return new(Status) { Content = new StringContent("unavailable") };
            if (RawResponse is not null && path != "/v1/oauth2/token")
                return new(HttpStatusCode.OK) { Content = new StringContent(RawResponse) };
            object response = path switch
            {
                "/v1/oauth2/token" => new { access_token = "synthetic-token" },
                "/v1/billing/plans/P-DISCOVERY" => new
                {
                    id = "P-DISCOVERY", status = "ACTIVE",
                    billing_cycles = new[] { new
                    {
                        tenure_type = "REGULAR", total_cycles = 0,
                        frequency = new { interval_unit = Frequency, interval_count = 1 },
                        pricing_scheme = new { fixed_price = new { currency_code = PlanCurrency, value = PlanPrice } }
                    }},
                    payment_preferences = new { auto_bill_outstanding = AutoBillOutstanding, setup_fee = new { value = "0" } }
                },
                "/v1/notifications/verify-webhook-signature" => new { verification_status = Verification },
                "/v1/billing/subscriptions/I-TEST/transactions" => new
                {
                    transactions = new[] { new
                    {
                        id = "PAY-1", status = "COMPLETED", time = "2026-01-31T12:00:00Z",
                        amount_with_breakdown = new { gross_amount = new { value = "1.20", currency_code = "EUR" } }
                    }}
                },
                _ => new
                {
                    id = "I-TEST", custom_id = CustomId, plan_id = "P-DISCOVERY", status = "APPROVAL_PENDING",
                    start_time = "2026-01-31T12:00:00Z",
                    links = new[] { new { rel = "approve", href = ApprovalUrl } }
                }
            };
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(response) };
        }
    }

    private static PayPalSubscriptionGateway Gateway(HttpClient http, string mode = "sandbox")
        => new(http, Options.Create(new PayPalSettings { Mode = mode, ClientId = "test-client", ClientSecret = "test-secret" }),
            Options.Create(new ResourceSubscriptionOptions { WebhookId = "test-webhook" }));

    private static ResourceSubscription Subscription() => new()
    {
        Id = Guid.NewGuid(), UserId = "alice", Pack = "Discovery", PlanId = "P-DISCOVERY",
        PriceExcludingTax = 1m, PriceIncludingTax = 1.2m
    };

    [Theory]
    [InlineData("sandbox", "api-m.sandbox.paypal.com")]
    [InlineData("production", "api-m.paypal.com")]
    public async Task Creation_checks_monthly_ttc_plan_and_uses_local_idempotency_reference(string mode, string host)
    {
        var subscription = Subscription();
        using var handler = new Handler { CustomId = subscription.Id.ToString("N") };
        using var http = new HttpClient(handler);
        var result = await Gateway(http, mode).CreateAsync(subscription, "https://site.example/return", "https://site.example/cancel", Ct);
        Assert.Equal("I-TEST", result.Id);
        var creation = Assert.Single(handler.Requests.Where(r => r.Path == "/v1/billing/subscriptions"));
        Assert.Equal(subscription.Id.ToString("N"), creation.RequestId);
        Assert.Contains(subscription.Id.ToString("N"), creation.Body);
        Assert.Contains("P-DISCOVERY", creation.Body);
        Assert.Contains("SUBSCRIBE_NOW", creation.Body);
        Assert.All(handler.Requests, r => Assert.Equal(host, r.Host));
    }

    [Theory]
    [InlineData("EUR", "1.00", "MONTH")]
    [InlineData("USD", "1.20", "MONTH")]
    [InlineData("EUR", "1.20", "YEAR")]
    public async Task Wrong_plan_price_currency_or_period_is_refused_before_checkout(string currency, string price, string period)
    {
        using var handler = new Handler { PlanCurrency = currency, PlanPrice = price, Frequency = period };
        using var http = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<SubscriptionOperationException>(() =>
            Gateway(http).CreateAsync(Subscription(), "https://site.example/return", "https://site.example/cancel", Ct));
        Assert.Equal(503, error.StatusCode);
        Assert.DoesNotContain(handler.Requests, r => r.Path == "/v1/billing/subscriptions");
    }

    [Fact]
    public async Task Automatic_collection_of_outstanding_payments_is_refused_before_checkout()
    {
        using var handler = new Handler { AutoBillOutstanding = true };
        using var http = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<SubscriptionOperationException>(() =>
            Gateway(http).CreateAsync(Subscription(), "https://site.example/return", "https://site.example/cancel", Ct));
        Assert.Equal(503, error.StatusCode);
        Assert.DoesNotContain(handler.Requests, r => r.Path == "/v1/billing/subscriptions");
    }

    [Theory]
    [InlineData("""[]""")]
    [InlineData("""{"id":"I-TEST","links":{}}""")]
    [InlineData("""{"id":"I-TEST","plan_overridden":"false"}""")]
    [InlineData("""{"id":"I-TEST","billing_info":[]}""")]
    [InlineData("""{"id":"I-TEST","links":[{"rel":"approve","href":"https://www.paypal.com/a"},{"rel":"approve","href":"https://www.paypal.com/b"}]}""")]
    public async Task Malformed_subscription_shapes_are_explicit_upstream_errors(string response)
    {
        using var handler = new Handler { RawResponse = response };
        using var http = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<SubscriptionOperationException>(() => Gateway(http).GetAsync("I-TEST", Ct));
        Assert.Equal(502, error.StatusCode);
    }

    [Theory]
    [InlineData("""{"billing_cycles":{}}""")]
    [InlineData("""{"id":"P-DISCOVERY","status":"ACTIVE","billing_cycles":[{"tenure_type":"REGULAR","total_cycles":"0","frequency":{"interval_unit":"MONTH","interval_count":1},"pricing_scheme":{"fixed_price":{"currency_code":"EUR","value":"1.20"}}}],"payment_preferences":{"auto_bill_outstanding":false}}""")]
    public async Task Malformed_plan_shapes_are_explicit_upstream_errors(string response)
    {
        using var handler = new Handler { RawResponse = response };
        using var http = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<SubscriptionOperationException>(() =>
            Gateway(http).CreateAsync(Subscription(), "https://site.example/return", "https://site.example/cancel", Ct));
        Assert.Equal(502, error.StatusCode);
    }

    [Fact]
    public async Task Approval_redirect_cannot_point_to_an_untrusted_site()
    {
        using var handler = new Handler { ApprovalUrl = "https://attacker.example/" };
        using var http = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<SubscriptionOperationException>(() => Gateway(http).GetAsync("I-TEST", Ct));
        Assert.Equal(502, error.StatusCode);
    }

    [Fact]
    public async Task Webhook_verification_uses_the_configured_id_and_requires_paypal_success()
    {
        using var handler = new Handler { Verification = "FAILURE" };
        using var http = new HttpClient(handler);
        using var notification = JsonDocument.Parse("""{"id":"WH-TEST","event_type":"PAYMENT.SALE.COMPLETED"}""");
        var headers = new Dictionary<string, string>
        {
            ["PAYPAL-AUTH-ALGO"] = "SHA256withRSA", ["PAYPAL-CERT-URL"] = "https://api.paypal.com/cert",
            ["PAYPAL-TRANSMISSION-ID"] = "test-transmission", ["PAYPAL-TRANSMISSION-SIG"] = "test-signature",
            ["PAYPAL-TRANSMISSION-TIME"] = "2026-01-31T12:00:00Z"
        };
        var error = await Assert.ThrowsAsync<SubscriptionOperationException>(() =>
            Gateway(http).VerifyWebhookAsync(notification.RootElement, headers, Ct));
        Assert.Equal(400, error.StatusCode);
        var verification = Assert.Single(handler.Requests.Where(r => r.Path.EndsWith("verify-webhook-signature")));
        Assert.Contains("test-webhook", verification.Body);
        Assert.Contains("test-signature", verification.Body);
        handler.Verification = "SUCCESS";
        await Gateway(http).VerifyWebhookAsync(notification.RootElement, headers, Ct);
    }

    [Fact]
    public async Task Transaction_history_is_read_with_bounded_ranges_and_invariant_euro_amounts()
    {
        using var handler = new Handler();
        using var http = new HttpClient(handler);
        var from = new DateTimeOffset(2026, 1, 31, 12, 0, 0, TimeSpan.Zero);
        var payments = await Gateway(http).TransactionsAsync("I-TEST", from, from.AddDays(61), Ct);
        Assert.Equal(3, handler.Requests.Count(r => r.Path.EndsWith("/transactions")));
        Assert.All(payments, p => Assert.Equal(1.2m, p.Amount));
        Assert.All(payments, p => Assert.Equal("EUR", p.Currency));
    }

    [Fact]
    public async Task Http_failure_is_an_explicit_payment_failure_not_a_successful_subscription()
    {
        using var handler = new Handler { Status = HttpStatusCode.ServiceUnavailable };
        using var http = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<SubscriptionOperationException>(() => Gateway(http).GetAsync("I-TEST", Ct));
        Assert.Equal(502, error.StatusCode);
    }

    [Fact]
    public async Task Invalid_paypal_id_is_refused_without_network_access()
    {
        using var handler = new Handler();
        using var http = new HttpClient(handler);
        await Assert.ThrowsAsync<SubscriptionOperationException>(() => Gateway(http).GetAsync("../other", Ct));
        Assert.Empty(handler.Requests);
    }
}
