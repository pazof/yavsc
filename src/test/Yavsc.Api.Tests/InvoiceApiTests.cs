using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Billing;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Workflow;
using Yavsc.Services;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class InvoiceApiTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static readonly DateTimeOffset Received = new(2026, 1, 2, 12, 0, 0, TimeSpan.Zero);

    private HttpClient Client(string? subject = "alice")
    {
        var http = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        }) { BaseAddress = new Uri(fixture.BaseAddress) };
        if (subject is not null)
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestTokenIssuer.Issue(subject, "api"));
        return http;
    }

    private (long Estimate, long Command) Seed(bool providerSignature = true, bool clientSignature = true, bool reset = true)
    {
        if (reset) fixture.ResetAndSeedActivityGraph();
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var command = new RdvQuery
        {
            ClientId = "bob", PerformerId = "alice", ActivityCode = "dev",
            Consent = true, Reason = "Invoice test", Location = db.Locations.Single(),
            EventDate = DateTime.UtcNow.AddDays(1), UserCreated = "alice", UserModified = "alice"
        };
        db.Add(command);
        db.SaveChanges();
        var estimate = new Estimate
        {
            CommandId = command.Id, ClientId = "bob", OwnerId = "alice", CommandType = BillingCodes.Rdv,
            Title = "Signed estimate", Description = "Invoice test",
            AttachedFiles = [], AttachedGraphics = [],
            ProviderValidationDate = DateTime.UtcNow, ClientValidationDate = DateTime.UtcNow,
            Bill =
            [
                new CommandLine { Name = "Service", Description = "Service", Count = 2, UnitaryCost = 60m },
                new CommandLine { Name = "Discount", Description = "Discount", Count = 1, UnitaryCost = -20m }
            ]
        };
        db.Add(estimate);
        db.SaveChanges();
        if (providerSignature) db.Signatures.Add(Signature(estimate.Id, SignatureType.Pro, "alice"));
        if (clientSignature) db.Signatures.Add(Signature(estimate.Id, SignatureType.Client, "bob"));
        db.SaveChanges();
        return (estimate.Id, command.Id);
    }

    private static Signature Signature(long estimateId, SignatureType type, string signer)
        => new()
        {
            EstimateId = estimateId, Type = type, SignerId = signer, CoordinateMax = 10_000,
            CapturedAtUtc = DateTime.UtcNow, Strokes = [2, 0, 0, 100, 100]
        };

    private static RecordInvoicePaymentRequest Receipt(string reference, decimal amount = 30m)
        => new() { Reference = reference, Amount = amount, Currency = "EUR", ReceivedAtUtc = Received };

    private static async Task<InvoiceResponse> InvoiceAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        var body = await response.Content.ReadAsStringAsync(Ct);
        Assert.True(response.StatusCode == expected, $"{response.StatusCode}: {body}");
        return (await response.Content.ReadFromJsonAsync<InvoiceResponse>(Ct))!;
    }

    [Fact]
    public async Task Signed_estimate_creates_a_frozen_unpaid_invoice_and_generation_is_idempotent()
    {
        var (estimateId, commandId) = Seed();
        using var http = Client();
        using var created = await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct);
        var invoice = await InvoiceAsync(created, HttpStatusCode.Created);
        Assert.EndsWith($"/api/v1/invoices/{commandId}", created.Headers.Location!.ToString());
        Assert.Equal(commandId, invoice.CommandId);
        Assert.Equal(estimateId, invoice.EstimateId);
        Assert.Equal("bob", invoice.ClientId);
        Assert.Equal("alice", invoice.PerformerId);
        Assert.Equal(100m, invoice.AmountDue);
        Assert.Equal(0m, invoice.AmountPaid);
        Assert.Equal(100m, invoice.AmountRemaining);
        Assert.Equal("Unpaid", invoice.PaymentStatus);
        Assert.False(invoice.IsAcquitted);
        Assert.Empty(invoice.Payments);
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var line = db.Estimates.Include(e => e.Bill).Single(e => e.Id == estimateId).Bill.First();
            line.UnitaryCost = 999m;
            db.SaveChanges();
            var command = db.RdvQueries.Single(q => q.Id == commandId);
            Assert.Equal(100m, command.GetBillItems().Sum(item => item.Count * item.UnitaryCost));
        }
        var repeated = await InvoiceAsync(
            await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct), HttpStatusCode.Created);
        Assert.Equal(invoice.CreatedAtUtc, repeated.CreatedAtUtc);
        Assert.Equal(100m, repeated.AmountDue);
        Assert.Equal(invoice.Lines.ToArray(), repeated.Lines.ToArray());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Dates_without_both_persisted_signatures_cannot_generate_an_invoice(bool provider, bool client)
    {
        var (estimateId, commandId) = Seed(provider, client);
        using var http = Client();
        var response = await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Null(db.RdvQueries.Single(q => q.Id == commandId).AmountDue);
    }

    [Theory]
    [InlineData("wrong-signer")]
    [InlineData("missing-date")]
    [InlineData("wrong-client")]
    [InlineData("wrong-performer")]
    [InlineData("standalone")]
    [InlineData("negative-total")]
    [InlineData("wrong-currency")]
    [InlineData("invalid-quantity")]
    [InlineData("fractional-cent")]
    [InlineData("empty-signature")]
    public async Task Inconsistent_estimates_are_rejected(string fault)
    {
        var (estimateId, _) = Seed();
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var estimate = db.Estimates.Include(e => e.Bill).Include(e => e.Signatures).Single(e => e.Id == estimateId);
            switch (fault)
            {
                case "wrong-signer": estimate.SignatureClient!.SignerId = "alice"; break;
                case "missing-date": estimate.ClientValidationDate = default; break;
                case "wrong-client": estimate.ClientId = "alice"; break;
                case "wrong-performer": db.RdvQueries.Single().PerformerId = "bob"; break;
                case "standalone": estimate.CommandId = null; break;
                case "negative-total": estimate.Bill.ForEach(line => line.UnitaryCost = -1m); break;
                case "wrong-currency": estimate.Bill[0].Currency = "USD"; break;
                case "invalid-quantity": estimate.Bill[0].Count = 0; break;
                case "fractional-cent": estimate.Bill[0].UnitaryCost = 0.001m; break;
                case "empty-signature": estimate.SignatureClient!.Strokes = []; break;
            }
            db.SaveChanges();
        }
        using var http = Client();
        Assert.Equal(HttpStatusCode.Conflict,
            (await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct)).StatusCode);
    }

    [Fact]
    public async Task Receipts_accumulate_idempotently_until_the_invoice_is_fully_paid()
    {
        var (estimateId, commandId) = Seed();
        using var http = Client();
        await InvoiceAsync(await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct), HttpStatusCode.Created);
        var path = $"/api/v1/invoices/{commandId}/payments/offline";
        var partial = await InvoiceAsync(await http.PostAsJsonAsync(path, Receipt("bank-1"), Ct), HttpStatusCode.OK);
        Assert.Equal(30m, partial.AmountPaid);
        Assert.Equal(70m, partial.AmountRemaining);
        Assert.Equal("PartiallyPaid", partial.PaymentStatus);
        Assert.False(partial.IsAcquitted);
        Assert.Single(partial.Payments);
        using (var partialScope = fixture.Services.CreateScope())
        {
            var partialDb = partialScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var partialCommand = partialDb.RdvQueries.Single(q => q.Id == commandId);
            Assert.Contains("Status: PartiallyPaid", partialCommand.GetBillText());
            Assert.DoesNotContain("Status: Paid", partialCommand.GetBillText());
        }
        var replay = await InvoiceAsync(await http.PostAsJsonAsync(path, Receipt("bank-1"), Ct), HttpStatusCode.OK);
        Assert.Equal(30m, replay.AmountPaid);
        Assert.Single(replay.Payments);
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync(path, Receipt("bank-1", 31m), Ct)).StatusCode);
        var total = await InvoiceAsync(await http.PostAsJsonAsync(path, Receipt("cash-2", 70m), Ct), HttpStatusCode.OK);
        Assert.Equal(100m, total.AmountPaid);
        Assert.Equal(0m, total.AmountRemaining);
        Assert.True(total.IsAcquitted);
        Assert.Equal("Paid", total.PaymentStatus);
        Assert.Equal(2, total.Payments.Count);
        using var client = Client("bob");
        var read = await InvoiceAsync(await client.GetAsync($"/api/v1/invoices/{commandId}", Ct), HttpStatusCode.OK);
        Assert.Equal(100m, read.AmountPaid);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var command = db.RdvQueries.Single(q => q.Id == commandId);
        Assert.Equal(100m, command.AmountPaid);
        Assert.True(command.GetIsAcquitted());
    }

    [Theory]
    [InlineData("negative")]
    [InlineData("zero")]
    [InlineData("currency")]
    [InlineData("date")]
    [InlineData("future")]
    [InlineData("fractional-cent")]
    public async Task Invalid_receipts_do_not_change_the_invoice(string fault)
    {
        var (estimateId, commandId) = Seed();
        using var http = Client();
        await InvoiceAsync(await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct), HttpStatusCode.Created);
        var request = fault switch
        {
            "negative" => Receipt("bad") with { Amount = -1m },
            "zero" => Receipt("bad") with { Amount = 0m },
            "currency" => Receipt("bad") with { Currency = "USD" },
            "date" => Receipt("bad") with { ReceivedAtUtc = default },
            "future" => Receipt("bad") with { ReceivedAtUtc = DateTimeOffset.UtcNow.AddDays(1) },
            _ => Receipt("bad") with { Amount = 0.001m }
        };
        Assert.Equal(HttpStatusCode.BadRequest,
            (await http.PostAsJsonAsync($"/api/v1/invoices/{commandId}/payments/offline", request, Ct)).StatusCode);
        var invoice = await InvoiceAsync(await http.GetAsync($"/api/v1/invoices/{commandId}", Ct), HttpStatusCode.OK);
        Assert.Equal(0m, invoice.AmountPaid);
        Assert.Empty(invoice.Payments);
    }

    [Fact]
    public async Task Only_performer_can_generate_or_attest_and_only_parties_can_read_or_verify()
    {
        var (estimateId, commandId) = Seed();
        using var provider = Client();
        using var client = Client("bob");
        using var outsider = Client("mallory");
        using var anonymous = Client(null);
        var generate = $"/api/v1/invoices/from-estimate/{estimateId}";
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync(generate, null, Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync(generate, null, Ct)).StatusCode);
        await InvoiceAsync(await provider.PostAsync(generate, null, Ct), HttpStatusCode.Created);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.PostAsJsonAsync($"/api/v1/invoices/{commandId}/payments/offline", Receipt("bank-1"), Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await outsider.GetAsync($"/api/v1/invoices/{commandId}", Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await outsider.PostAsJsonAsync(
            $"/api/v1/invoices/{commandId}/payments/paypal", new { transactionId = "TX-1" }, Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await provider.GetAsync("/api/v1/invoices/999999999", Ct)).StatusCode);
    }

    [Fact]
    public async Task Unconfigured_paypal_never_creates_a_receipt()
    {
        var (estimateId, commandId) = Seed();
        using var http = Client();
        await InvoiceAsync(await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct), HttpStatusCode.Created);
        var response = await http.PostAsJsonAsync($"/api/v1/invoices/{commandId}/payments/paypal", new { transactionId = "TX-1" }, Ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var invoice = await InvoiceAsync(await http.GetAsync($"/api/v1/invoices/{commandId}", Ct), HttpStatusCode.OK);
        Assert.Empty(invoice.Payments);
        Assert.Equal(0m, invoice.AmountPaid);
    }

    [Fact]
    public async Task Verified_paypal_receipts_combine_with_offline_payments_without_double_counting()
    {
        var (estimateId, commandId) = Seed();
        using var http = Client();
        await InvoiceAsync(await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct), HttpStatusCode.Created);
        await InvoiceAsync(await http.PostAsJsonAsync(
            $"/api/v1/invoices/{commandId}/payments/offline", Receipt("bank-1"), Ct), HttpStatusCode.OK);
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var service = new InvoicingService(db, new VerifiedPayPalStub());
            var invoice = await service.VerifyPayPalAsync(commandId, "TX-1", "bob", false, Ct);
            Assert.True(invoice.IsAcquitted);
            Assert.Equal(100m, invoice.AmountPaid);
            Assert.Equal(2, invoice.Payments.Count);
        }
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var service = new InvoicingService(db, new VerifiedPayPalStub());
            var invoice = await service.VerifyPayPalAsync(commandId, "TX-1", "bob", false, Ct);
            Assert.Equal(100m, invoice.AmountPaid);
            Assert.Equal(2, invoice.Payments.Count);
        }
    }

    [Fact]
    public async Task A_receipt_reference_cannot_be_reused_for_another_invoice()
    {
        var first = Seed();
        var second = Seed(reset: false);
        using var http = Client();
        await InvoiceAsync(await http.PostAsync($"/api/v1/invoices/from-estimate/{first.Estimate}", null, Ct), HttpStatusCode.Created);
        await InvoiceAsync(await http.PostAsync($"/api/v1/invoices/from-estimate/{second.Estimate}", null, Ct), HttpStatusCode.Created);
        await InvoiceAsync(await http.PostAsJsonAsync(
            $"/api/v1/invoices/{first.Command}/payments/offline", Receipt("bank-1"), Ct), HttpStatusCode.OK);
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync(
            $"/api/v1/invoices/{second.Command}/payments/offline", Receipt("bank-1"), Ct)).StatusCode);
        using (var scope = fixture.Services.CreateScope())
        {
            var service = new InvoicingService(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(), new VerifiedPayPalStub());
            await service.VerifyPayPalAsync(first.Command, "TX-1", "bob", false, Ct);
        }
        using (var scope = fixture.Services.CreateScope())
        {
            var service = new InvoicingService(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(), new VerifiedPayPalStub());
            var error = await Assert.ThrowsAsync<InvoiceOperationException>(
                () => service.VerifyPayPalAsync(second.Command, "TX-1", "bob", false, Ct));
            Assert.Equal(409, error.StatusCode);
        }
    }

    [Fact]
    public async Task Concurrent_receipts_are_atomic_and_the_losing_request_can_be_retried()
    {
        var (estimateId, commandId) = Seed();
        using var http = Client();
        await InvoiceAsync(await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct), HttpStatusCode.Created);
        using var scope = fixture.Services.CreateScope();
        var pause = new SavePause();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>(
            scope.ServiceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>())
            .AddInterceptors(pause).Options;
        await using var stale = new ApplicationDbContext(options);
        var staleService = new InvoicingService(stale, new VerifiedPayPalStub());
        var first = staleService.RecordOfflineAsync(commandId, Receipt("bank-1"), "alice", false, Ct);
        try
        {
            await pause.Started.Task.WaitAsync(TimeSpan.FromSeconds(10), Ct);
            using var otherScope = fixture.Services.CreateScope();
            var otherService = new InvoicingService(otherScope.ServiceProvider.GetRequiredService<ApplicationDbContext>(), new VerifiedPayPalStub());
            var other = await otherService.RecordOfflineAsync(commandId, Receipt("bank-2", 40m), "alice", false, Ct);
            Assert.Equal(40m, other.AmountPaid);
        }
        finally
        {
            pause.Resume.TrySetResult();
        }
        var conflict = await Assert.ThrowsAsync<InvoiceOperationException>(() => first);
        Assert.Equal(409, conflict.StatusCode);
        using (var checkScope = fixture.Services.CreateScope())
        {
            var db = checkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Single(db.InvoicePayments);
            Assert.Equal(40m, db.RdvQueries.Single(q => q.Id == commandId).AmountPaid);
        }
        var retry = await InvoiceAsync(await http.PostAsJsonAsync(
            $"/api/v1/invoices/{commandId}/payments/offline", Receipt("bank-1"), Ct), HttpStatusCode.OK);
        Assert.Equal(70m, retry.AmountPaid);
        Assert.Equal(2, retry.Payments.Count);
    }

    [Fact]
    public async Task Invoiced_commands_and_estimates_cannot_be_deleted()
    {
        var (estimateId, commandId) = Seed();
        using var provider = Client();
        using var client = Client("bob");
        await InvoiceAsync(await provider.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct), HttpStatusCode.Created);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/v1/billing/Rdv/{commandId}", Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await provider.DeleteAsync($"/api/v1/estimate/{estimateId}", Ct)).StatusCode);
    }

    [Fact]
    public async Task Legacy_document_generation_cannot_bypass_invoice_establishment()
    {
        var (_, commandId) = Seed();
        using var provider = Client();
        Assert.Equal(HttpStatusCode.Conflict,
            (await provider.PostAsync($"/api/v1/bill/genpdf/Rdv/{commandId}", null, Ct)).StatusCode);
    }

    [Fact]
    public async Task Invoice_total_cannot_be_overwritten_and_overpayments_keep_the_actual_amount()
    {
        var (estimateId, commandId) = Seed();
        using var http = Client();
        await InvoiceAsync(await http.PostAsync($"/api/v1/invoices/from-estimate/{estimateId}", null, Ct), HttpStatusCode.Created);
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var command = db.RdvQueries.Single(q => q.Id == commandId);
            Assert.Throws<InvalidOperationException>(() => command.SetPaymentAmounts(10m, 0m));
            Assert.Equal(100m, command.AmountDue);
        }
        var request = Receipt("bank-overpaid", 120m) with { ReceivedAtUtc = Received.AddTicks(1234567) };
        var invoice = await InvoiceAsync(await http.PostAsJsonAsync(
            $"/api/v1/invoices/{commandId}/payments/offline", request, Ct), HttpStatusCode.OK);
        Assert.Equal(120m, invoice.AmountPaid);
        Assert.Equal(0m, invoice.AmountRemaining);
        Assert.True(invoice.IsAcquitted);
        var replay = await InvoiceAsync(await http.PostAsJsonAsync(
            $"/api/v1/invoices/{commandId}/payments/offline", request, Ct), HttpStatusCode.OK);
        Assert.Equal(120m, replay.AmountPaid);
        Assert.Single(replay.Payments);
    }

    [Fact]
    public async Task Openapi_documents_the_invoice_routes()
    {
        using var admin = Client();
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            TestTokenIssuer.Issue("alice", "api", extraClaims:
            [new System.Security.Claims.Claim(Constants.RoleClaimType, Constants.AdminGroupName)]));
        var response = await admin.GetAsync("/openapi/invoicing.json", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("/api/v1/invoices/from-estimate/{estimateId}", json);
        Assert.Contains("/api/v1/invoices/{commandId}/payments/paypal", json);
        Assert.Contains("/api/v1/invoices/{commandId}/payments/offline", json);
    }

    private sealed class SavePause : SaveChangesInterceptor
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            await Resume.Task.WaitAsync(cancellationToken);
            return result;
        }
    }

    private sealed class VerifiedPayPalStub : IPayPalTransactionVerifier
    {
        public Task<VerifiedPayPalReceipt> VerifyAsync(string transactionId, string invoiceReference, string currency, CancellationToken ct)
        {
            Assert.StartsWith("RdvQuery/", invoiceReference);
            Assert.Equal("EUR", currency);
            return Task.FromResult(new VerifiedPayPalReceipt(transactionId, 70m, currency, Received));
        }
    }
}
