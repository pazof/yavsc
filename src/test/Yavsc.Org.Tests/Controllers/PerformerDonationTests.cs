using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Models.Payment;
using Yavsc.Models.Relationship;
using Yavsc.Models.Workflow;
using Yavsc.Web.Services;
using Xunit;

namespace Yavsc.Org.Tests.Controllers;

public sealed class PerformerDonationTests : IClassFixture<PerformerDonationTests.Factory>
{
    private readonly Factory _factory;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public PerformerDonationTests(Factory factory)
    {
        _factory = factory;
        factory.PayPal.Reset();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.PerformerDonations.RemoveRange(db.PerformerDonations);
        if (!db.Users.Any(u => u.Id == "author"))
        {
            db.Users.AddRange(
                new ApplicationUser { Id = "author", UserName = "Author", Email = "author@example.test" },
                new ApplicationUser { Id = "listener", UserName = "Listener", Email = "listener@example.test" });
        }
        if (!db.Performers.Any(p => p.PerformerId == "author"))
        {
            db.Performers.Add(new PerformerProfile
            {
                PerformerId = "author", Active = true, SIREN = "123456789", WebSite = "",
                OrganizationAddress = new Location { Address = "Test" }
            });
        }
        db.SaveChanges();
        var performer = db.Performers.Single(p => p.PerformerId == "author");
        performer.DonationPayPalEmail = "author@payments.example";
        performer.Active = true;
        db.SaveChanges();
    }

    [Theory]
    [InlineData("/Donation/Create/author")]
    [InlineData("/Donation/Settings")]
    public async Task Donation_pages_require_a_web_session(string path)
    {
        using var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        using var response = await client.GetAsync(path, Ct);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/signin", response.Headers.Location?.ToString());
        Assert.Empty(_factory.PayPal.Requests);
    }

    [Fact]
    public async Task Performer_configures_only_their_own_recipient_and_can_disable_new_donations()
    {
        using var client = _factory.SignedInClient("author");
        using var response = await PostFormAsync(client, "/Donation/Settings", "/Donation/Settings", new()
        {
            ["PayPalEmail"] = "new-recipient@payments.example", ["PerformerId"] = "listener"
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("new-recipient@payments.example", await RecipientEmailAsync());
        using var disabled = await PostFormAsync(client, "/Donation/Settings", "/Donation/Settings", new() { ["PayPalEmail"] = "" });
        Assert.Equal(HttpStatusCode.Redirect, disabled.StatusCode);
        Assert.Null(await RecipientEmailAsync());
        using var listener = _factory.SignedInClient();
        using var create = await listener.GetAsync("/Donation/Create/author", Ct);
        Assert.Equal(HttpStatusCode.NotFound, create.StatusCode);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("https://external.example/donate")]
    public async Task Invalid_recipient_setting_is_rejected(string value)
    {
        using var client = _factory.SignedInClient("author");
        using var response = await PostFormAsync(client, "/Donation/Settings", "/Donation/Settings", new() { ["PayPalEmail"] = value });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("author@payments.example", await RecipientEmailAsync());
    }

    [Fact]
    public async Task Non_performer_cannot_configure_a_recipient()
    {
        using var client = _factory.SignedInClient();
        using var response = await client.GetAsync("/Donation/Settings", Ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("/Manage/SetActivity", await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task Checkout_uses_the_stored_recipient_and_existing_paypal_configuration()
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        Assert.Equal("listener", donation.DonorId);
        Assert.Equal("author@payments.example", donation.RecipientEmail);
        Assert.Equal(5m, donation.Amount);
        Assert.Equal(PerformerDonationState.AwaitingApproval, donation.State);
        var request = Assert.Single(_factory.PayPal.Requests);
        Assert.Equal("SetExpressCheckout", request["METHOD"]);
        Assert.Equal("author@payments.example", request["PAYMENTREQUEST_0_SELLERPAYPALACCOUNTID"]);
        Assert.Equal("5.00", request["PAYMENTREQUEST_0_AMT"]);
        Assert.Equal("EUR", request["PAYMENTREQUEST_0_CURRENCYCODE"]);
        Assert.Equal("https://org.example/Donation/Notify", request["PAYMENTREQUEST_0_NOTIFYURL"]);
        Assert.StartsWith("https://org.example/Donation/Return?id=", request["RETURNURL"]);
        Assert.Equal(donation.Id.ToString("N"), request["PAYMENTREQUEST_0_INVNUM"]);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("5.001")]
    public async Task Invalid_amount_does_not_initiate_paypal(string amount)
    {
        using var client = _factory.SignedInClient();
        using var response = await PostFormAsync(client, "/Donation/Create/author", "/Donation/Create", new()
        {
            ["PerformerId"] = "author", ["Amount"] = amount
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(_factory.PayPal.Requests);
        using var scope = _factory.Services.CreateScope();
        Assert.Empty(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().PerformerDonations);
    }

    [Fact]
    public async Task Checkout_is_not_a_payment_and_confirmation_is_post_only_and_idempotent()
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        using var returned = await client.GetAsync($"/Donation/Return/{donation.Id}?token={donation.CheckoutToken}&PayerID=untrusted", Ct);
        Assert.Equal(HttpStatusCode.Redirect, returned.StatusCode);
        Assert.Equal(PerformerDonationState.AwaitingApproval, (await FindAsync(donation.Id)).State);
        using var get = await client.GetAsync($"/Donation/Confirm/{donation.Id}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        using var confirmed = await ConfirmAsync(client, donation.Id);
        Assert.Equal(HttpStatusCode.Redirect, confirmed.StatusCode);
        var completed = await FindAsync(donation.Id);
        Assert.Equal(PerformerDonationState.Completed, completed.State);
        Assert.NotNull(completed.TransactionId);
        Assert.NotNull(completed.CompletedAt);
        var payment = _factory.PayPal.Requests.Single(r => r["METHOD"] == "DoExpressCheckoutPayment");
        Assert.Equal("approved-payer", payment["PAYERID"]);
        Assert.Equal(donation.Id.ToString("N"), payment["MSGSUBID"]);
        using var again = await ConfirmAsync(client, donation.Id);
        Assert.Equal(HttpStatusCode.Redirect, again.StatusCode);
        Assert.Single(_factory.PayPal.Requests, r => r["METHOD"] == "DoExpressCheckoutPayment");
    }

    [Fact]
    public async Task Another_user_and_a_substituted_checkout_token_cannot_confirm_the_donation()
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        using var author = _factory.SignedInClient("author");
        using var details = await author.GetAsync($"/Donation/Details/{donation.Id}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, details.StatusCode);
        using var wrong = await client.GetAsync($"/Donation/Return/{donation.Id}?token=another-token", Ct);
        Assert.Equal(HttpStatusCode.NotFound, wrong.StatusCode);
        Assert.DoesNotContain(_factory.PayPal.Requests, r => r["METHOD"] == "DoExpressCheckoutPayment");
    }

    [Fact]
    public async Task Payment_posts_require_antiforgery()
    {
        using var client = _factory.SignedInClient();
        using var response = await client.PostAsync("/Donation/Create",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["PerformerId"] = "author", ["Amount"] = "5" }), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.PayPal.Requests);
    }

    [Fact]
    public async Task Changed_recipient_or_amount_in_paypal_approval_prevents_capture()
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        _factory.PayPal.MismatchApproval = true;
        using var response = await ConfirmAsync(client, donation.Id);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.DoesNotContain(_factory.PayPal.Requests, r => r["METHOD"] == "DoExpressCheckoutPayment");
        Assert.Equal(PerformerDonationState.AwaitingApproval, (await FindAsync(donation.Id)).State);
    }

    [Fact]
    public async Task Pending_capture_is_not_displayed_as_paid_and_verified_ipn_completes_it_once()
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        _factory.PayPal.Pending = true;
        using var captured = await ConfirmAsync(client, donation.Id);
        Assert.Equal(HttpStatusCode.Redirect, captured.StatusCode);
        var pending = await FindAsync(donation.Id);
        Assert.Equal(PerformerDonationState.Pending, pending.State);
        Assert.Null(pending.CompletedAt);
        using var ipn = await NotifyAsync(donation.Id);
        Assert.Equal(HttpStatusCode.OK, ipn.StatusCode);
        using var duplicate = await NotifyAsync(donation.Id);
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        var completed = await FindAsync(donation.Id);
        Assert.Equal(PerformerDonationState.Completed, completed.State);
        Assert.NotNull(completed.CompletedAt);
        Assert.Single(_factory.PayPal.Requests, r => r["METHOD"] == "DoExpressCheckoutPayment");
    }

    [Fact]
    public async Task Capture_timeout_keeps_a_reconcilable_processing_state_without_charging_again()
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        _factory.PayPal.CaptureTimeout = true;
        using var response = await ConfirmAsync(client, donation.Id);
        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
        Assert.Equal(PerformerDonationState.Processing, (await FindAsync(donation.Id)).State);
        using var retry = await ConfirmAsync(client, donation.Id);
        Assert.Equal(HttpStatusCode.Conflict, retry.StatusCode);
        using var ipn = await NotifyAsync(donation.Id);
        Assert.Equal(HttpStatusCode.OK, ipn.StatusCode);
        Assert.Equal(PerformerDonationState.Completed, (await FindAsync(donation.Id)).State);
        Assert.Single(_factory.PayPal.Requests, r => r["METHOD"] == "DoExpressCheckoutPayment");
    }

    [Theory]
    [InlineData(false, "author@payments.example", "5.00", "EUR", 400)]
    [InlineData(true, "someone-else@payments.example", "5.00", "EUR", 409)]
    [InlineData(true, "author@payments.example", "6.00", "EUR", 409)]
    [InlineData(true, "author@payments.example", "5.00", "USD", 409)]
    public async Task Forged_or_mismatched_ipn_cannot_mark_a_donation_paid(bool verified, string recipient, string amount, string currency, int status)
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        _factory.PayPal.Pending = true;
        using var captured = await ConfirmAsync(client, donation.Id);
        _factory.PayPal.ValidNotification = verified;
        using var ipn = await NotifyAsync(donation.Id, recipient, amount, currency);
        Assert.Equal(status, (int)ipn.StatusCode);
        Assert.Equal(PerformerDonationState.Pending, (await FindAsync(donation.Id)).State);
    }

    [Fact]
    public async Task Concurrent_confirmation_sends_only_one_capture()
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        _factory.PayPal.HoldCapture = true;
        var first = ConfirmAsync(client, donation.Id);
        await _factory.PayPal.CaptureStarted.Task.WaitAsync(Ct);
        try
        {
            using var second = await ConfirmAsync(client, donation.Id);
            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }
        finally { _factory.PayPal.ReleaseCapture.TrySetResult(); }
        using var response = await first;
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Single(_factory.PayPal.Requests, r => r["METHOD"] == "DoExpressCheckoutPayment");
    }

    [Theory]
    [InlineData("Completed", PerformerDonationState.Completed)]
    [InlineData("Denied", PerformerDonationState.Failed)]
    public async Task Notification_received_during_capture_is_not_overwritten_by_a_stale_pending_response(
        string paymentStatus, PerformerDonationState expected)
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        _factory.PayPal.Pending = true;
        _factory.PayPal.HoldCapture = true;
        var capturing = ConfirmAsync(client, donation.Id);
        await _factory.PayPal.CaptureStarted.Task.WaitAsync(Ct);
        try
        {
            using var notification = await NotifyAsync(donation.Id, paymentStatus: paymentStatus);
            Assert.Equal(HttpStatusCode.OK, notification.StatusCode);
        }
        finally { _factory.PayPal.ReleaseCapture.TrySetResult(); }
        using var response = await capturing;
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expected, (await FindAsync(donation.Id)).State);
    }

    [Fact]
    public async Task Donation_keeps_its_original_recipient_when_the_performer_updates_their_settings()
    {
        using var donor = _factory.SignedInClient();
        var donation = await StartAsync(donor);
        using var author = _factory.SignedInClient("author");
        using var settings = await PostFormAsync(author, "/Donation/Settings", "/Donation/Settings", new()
        {
            ["PayPalEmail"] = "new-recipient@payments.example"
        });
        Assert.Equal(HttpStatusCode.Redirect, settings.StatusCode);
        using var confirmation = await ConfirmAsync(donor, donation.Id);
        Assert.Equal(HttpStatusCode.Redirect, confirmation.StatusCode);
        var payment = _factory.PayPal.Requests.Single(r => r["METHOD"] == "DoExpressCheckoutPayment");
        Assert.Equal("author@payments.example", payment["PAYMENTREQUEST_0_SELLERPAYPALACCOUNTID"]);
        Assert.Equal("author@payments.example", (await FindAsync(donation.Id)).RecipientEmail);
    }

    [Fact]
    public async Task History_exposes_only_the_current_donors_payments()
    {
        using var donor = _factory.SignedInClient();
        var donation = await StartAsync(donor);
        var ownHistory = await donor.GetStringAsync("/Donation", Ct);
        Assert.Contains(donation.Id.ToString(), ownHistory);
        using var other = _factory.SignedInClient("author");
        var otherHistory = await other.GetStringAsync("/Donation", Ct);
        Assert.DoesNotContain(donation.Id.ToString(), otherHistory);
    }

    [Fact]
    public async Task Recipient_email_is_not_exposed_in_performer_json()
    {
        using var scope = _factory.Services.CreateScope();
        var profile = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Performers.AsNoTracking().SingleAsync(p => p.PerformerId == "author", Ct);
        Assert.DoesNotContain("author@payments.example", System.Text.Json.JsonSerializer.Serialize(profile));
        Assert.DoesNotContain("author@payments.example", Newtonsoft.Json.JsonConvert.SerializeObject(profile));
    }

    [Fact]
    public async Task General_profile_edit_preserves_the_separately_configured_paypal_recipient()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var profile = await db.Performers.AsNoTracking().Include(p => p.OrganizationAddress)
            .SingleAsync(p => p.PerformerId == "author", Ct);
        profile.DonationPayPalEmail = null;
        profile.ExerciseCountryCode = "en";
        profile.SIREN = "AB12CD34";
        var controller = ManageController(scope.ServiceProvider);
        Assert.IsType<RedirectToActionResult>(await controller.SetActivity(profile));
        Assert.Equal("author@payments.example", await RecipientEmailAsync());
    }

    [Fact]
    public async Task Performer_with_donation_history_cannot_be_deleted_by_unsetting_their_activity()
    {
        using var client = _factory.SignedInClient();
        await StartAsync(client);
        using var scope = _factory.Services.CreateScope();
        var controller = ManageController(scope.ServiceProvider);
        Assert.IsType<ConflictObjectResult>(await controller.UnsetActivity());
        Assert.Equal("author@payments.example", await RecipientEmailAsync());
    }

    private static Yavsc.Controllers.ManageController ManageController(IServiceProvider services)
    {
        var controller = ActivatorUtilities.CreateInstance<Yavsc.Controllers.ManageController>(services);
        controller.ControllerContext = new ControllerContext
        {
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor(),
            HttpContext = new DefaultHttpContext
            {
                RequestServices = services,
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim("sub", "author"),
                    new Claim("name", "Author"),
                    new Claim(Yavsc.Constants.RoleClaimType, "Performer")
                ], "test"))
            }
        };
        return controller;
    }

    [Fact]
    public async Task Cancel_return_has_no_side_effect_and_post_cancellation_prevents_capture()
    {
        using var client = _factory.SignedInClient();
        var donation = await StartAsync(client);
        using var get = await client.GetAsync($"/Donation/Cancel/{donation.Id}", Ct);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal(PerformerDonationState.AwaitingApproval, (await FindAsync(donation.Id)).State);
        using var cancelled = await PostFormAsync(client, $"/Donation/Cancel/{donation.Id}", $"/Donation/CancelConfirm/{donation.Id}", new());
        Assert.Equal(HttpStatusCode.Redirect, cancelled.StatusCode);
        Assert.Equal(PerformerDonationState.Cancelled, (await FindAsync(donation.Id)).State);
        using var confirm = await ConfirmAsync(client, donation.Id);
        Assert.Equal(HttpStatusCode.Conflict, confirm.StatusCode);
        Assert.DoesNotContain(_factory.PayPal.Requests, r => r["METHOD"] == "DoExpressCheckoutPayment");
    }

    private async Task<PerformerDonation> StartAsync(HttpClient client)
    {
        using var response = await PostFormAsync(client, "/Donation/Create/author", "/Donation/Create", new()
        {
            ["PerformerId"] = "author", ["Amount"] = "5",
            ["RecipientEmail"] = "attacker@payments.example", ["Currency"] = "USD", ["DonorId"] = "author"
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("https://www.sandbox.paypal.com/", response.Headers.Location?.ToString());
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().PerformerDonations.AsNoTracking().SingleAsync(Ct);
    }

    private Task<HttpResponseMessage> ConfirmAsync(HttpClient client, Guid id)
        => PostFormAsync(client, "/Donation/Create/author", $"/Donation/Confirm/{id}", new());

    private static async Task<HttpResponseMessage> PostFormAsync(HttpClient client, string page, string action, Dictionary<string, string> fields)
    {
        var html = await client.GetStringAsync(page, Ct);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, "The page must render an antiforgery-protected form.");
        fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value);
        return await client.PostAsync(action, new FormUrlEncodedContent(fields), Ct);
    }

    private async Task<PerformerDonation> FindAsync(Guid id)
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().PerformerDonations.AsNoTracking().SingleAsync(d => d.Id == id, Ct);
    }

    private async Task<string?> RecipientEmailAsync()
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Performers
            .Where(p => p.PerformerId == "author").Select(p => p.DonationPayPalEmail).SingleAsync(Ct);
    }

    private async Task<HttpResponseMessage> NotifyAsync(Guid id, string recipient = "author@payments.example",
        string amount = "5.00", string currency = "EUR", string paymentStatus = "Completed")
    {
        using var client = _factory.CreateClient();
        return await client.PostAsync("/Donation/Notify", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["invoice"] = id.ToString("N"), ["receiver_email"] = recipient, ["mc_gross"] = amount,
            ["mc_currency"] = currency, ["txn_id"] = "payment-" + id.ToString("N")[..16],
            ["payment_status"] = paymentStatus, ["txn_type"] = "express_checkout"
        }), Ct);
    }

    public sealed class Factory : PodcastWebTests.Factory
    {
        public FakePayPal PayPal { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureTestServices(services =>
            {
                services.Configure<PayPalSettings>(settings =>
                {
                    settings.Mode = "sandbox";
                    settings.MerchantAccountUserName = "test-merchant";
                    settings.Accounts = [new()
                    {
                        ApiUsername = "test-merchant", ApiPassword = "test-password", Signature = "test-signature"
                    }];
                });
                services.AddTransient<IPayPalDonationGateway>(provider => new PayPalDonationGateway(
                    new HttpClient(PayPal, disposeHandler: false), provider.GetRequiredService<IOptions<PayPalSettings>>()));
            });
        }
    }

    public sealed class FakePayPal : HttpMessageHandler
    {
        public ConcurrentQueue<Dictionary<string, string>> Requests { get; } = new();
        private readonly Dictionary<string, Dictionary<string, string>> _checkouts = new();
        public bool Pending { get; set; }
        public bool MismatchApproval { get; set; }
        public bool CaptureTimeout { get; set; }
        public bool ValidNotification { get; set; }
        public bool HoldCapture { get; set; }
        public TaskCompletionSource CaptureStarted { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseCapture { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Reset()
        {
            Requests.Clear();
            _checkouts.Clear();
            Pending = MismatchApproval = CaptureTimeout = HoldCapture = false;
            ValidNotification = true;
            CaptureStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            ReleaseCapture = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var body = await request.Content!.ReadAsStringAsync(ct);
            if (request.RequestUri!.Host.StartsWith("ipnpb.", StringComparison.Ordinal))
            {
                Assert.StartsWith("cmd=_notify-validate&", body);
                return new(HttpStatusCode.OK) { Content = new StringContent(ValidNotification ? "VERIFIED" : "INVALID") };
            }
            Assert.Equal("https://api-3t.sandbox.paypal.com/nvp", request.RequestUri.AbsoluteUri);
            var fields = body.Split('&').Select(pair => pair.Split('=', 2))
                .ToDictionary(p => WebUtility.UrlDecode(p[0]), p => WebUtility.UrlDecode(p[1]));
            Requests.Enqueue(fields);
            Dictionary<string, string> result = new() { ["ACK"] = "Success" };
            switch (fields["METHOD"])
            {
                case "SetExpressCheckout":
                    var token = "checkout-" + fields["PAYMENTREQUEST_0_INVNUM"];
                    _checkouts[token] = fields;
                    result["TOKEN"] = token;
                    break;
                case "GetExpressCheckoutDetails":
                    var checkout = _checkouts[fields["TOKEN"]];
                    result["TOKEN"] = fields["TOKEN"];
                    result["PAYERID"] = "approved-payer";
                    foreach (var name in new[] { "INVNUM", "AMT", "CURRENCYCODE", "SELLERPAYPALACCOUNTID" })
                        result["PAYMENTREQUEST_0_" + name] = checkout["PAYMENTREQUEST_0_" + name];
                    if (MismatchApproval) result["PAYMENTREQUEST_0_AMT"] = "1000.00";
                    break;
                case "DoExpressCheckoutPayment":
                    CaptureStarted.TrySetResult();
                    if (HoldCapture) await ReleaseCapture.Task.WaitAsync(ct);
                    if (CaptureTimeout) throw new TaskCanceledException("Simulated PayPal timeout.");
                    result["PAYMENTINFO_0_AMT"] = fields["PAYMENTREQUEST_0_AMT"];
                    result["PAYMENTINFO_0_CURRENCYCODE"] = fields["PAYMENTREQUEST_0_CURRENCYCODE"];
                    result["PAYMENTINFO_0_SELLERPAYPALACCOUNTID"] = fields["PAYMENTREQUEST_0_SELLERPAYPALACCOUNTID"];
                    result["PAYMENTINFO_0_PAYMENTSTATUS"] = Pending ? "Pending" : "Completed";
                    result["PAYMENTINFO_0_TRANSACTIONID"] = "payment-" + fields["PAYMENTREQUEST_0_INVNUM"][..16];
                    break;
                default: throw new InvalidOperationException("Unexpected PayPal operation.");
            }
            using var content = new FormUrlEncodedContent(result);
            return new(HttpStatusCode.OK) { Content = new StringContent(await content.ReadAsStringAsync(ct), Encoding.UTF8, "application/x-www-form-urlencoded") };
        }
    }
}
