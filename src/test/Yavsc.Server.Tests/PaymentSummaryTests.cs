using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Workflow;
using Yavsc.Billing;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Payment;
using Yavsc.Models.Workflow;
using Yavsc.Services;

namespace Yavsc.Server.Tests;

public class PaymentSummaryTests
{
    [Theory]
    [InlineData(100, 0, 100, PaymentStatus.Unpaid, false)]
    [InlineData(100, 30, 70, PaymentStatus.PartiallyPaid, false)]
    [InlineData(100, 100, 0, PaymentStatus.Paid, true)]
    [InlineData(100, 120, 0, PaymentStatus.Paid, true)]
    [InlineData(0, 0, 0, PaymentStatus.Paid, true)]
    public void Summary_distinguishes_partial_and_total_payment(
        decimal due, decimal paid, decimal remaining, PaymentStatus status, bool acquitted)
    {
        var summary = new PaymentSummary(due, paid);

        Assert.Equal(due, summary.AmountDue);
        Assert.Equal(paid, summary.AmountPaid);
        Assert.Equal(remaining, summary.AmountRemaining);
        Assert.Equal(status, summary.Status);
        Assert.Equal(acquitted, summary.IsAcquitted);
    }

    [Fact]
    public void Summary_uses_exact_decimal_amounts()
    {
        var summary = new PaymentSummary(0.3m, 0.1m + 0.2m);

        Assert.Equal(0m, summary.AmountRemaining);
        Assert.True(summary.IsAcquitted);
        Assert.False(new PaymentSummary(100m, 99.99m).IsAcquitted);
    }

    [Theory]
    [InlineData(-1, 0, "amountDue")]
    [InlineData(100, -1, "amountPaid")]
    public void Negative_amounts_are_rejected(decimal due, decimal paid, string parameter)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PaymentSummary(due, paid));

        Assert.Equal(parameter, exception.ParamName);
    }

    [Fact]
    public void Gateway_success_without_known_amounts_does_not_acquit_the_bill()
    {
        IBillable command = new TestCommand
        {
            Regularization = new PayPalPayment { State = "SUCCESS" }
        };

        Assert.Null(command.PaymentSummary);
        Assert.False(command.GetIsAcquitted());
    }

    [Fact]
    public void Acquittal_and_document_name_follow_the_balance_not_gateway_success()
    {
        var command = new TestCommand
        {
            Id = 42,
            Regularization = new PayPalPayment { State = "SUCCESS" }
        };
        var service = new TestBillingService();
        command.SetPaymentAmounts(100m, 30m);
        Assert.False(command.GetIsAcquitted());
        Assert.Equal(PaymentStatus.PartiallyPaid, command.PaymentSummary!.Status);
        Assert.Equal("facture-payment-summary-test-42", command.GetFileBaseName(service));

        command.Regularization = null!;
        command.SetPaymentAmounts(100m, 100m);
        Assert.True(command.GetIsAcquitted());
        Assert.Equal("facture-payment-summary-test-42-ack", command.GetFileBaseName(service));
    }

    [Fact]
    public void Invalid_amount_update_leaves_previous_balance_unchanged()
    {
        var command = new TestCommand();
        command.SetPaymentAmounts(100m, 30m);

        Assert.Throws<ArgumentOutOfRangeException>(() => command.SetPaymentAmounts(50m, -1m));

        Assert.Equal(100m, command.AmountDue);
        Assert.Equal(30m, command.AmountPaid);
    }

    [Fact]
    public async Task Amounts_and_derived_status_survive_reloading_the_command()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        long id;
        using (var context = new ApplicationDbContext(options))
        {
            var command = new RdvQuery
            {
                ClientId = "client",
                PerformerId = "provider",
                ActivityCode = "test",
                Reason = "Payment test",
                UserCreated = "client",
                UserModified = "client"
            };
            context.Add(command);
            await context.SaveChangesAsync();
            id = command.Id;
        }

        using (var context = new ApplicationDbContext(options))
        {
            var command = await context.RdvQueries.SingleAsync(q => q.Id == id, TestContext.Current.CancellationToken);
            Assert.Null(command.AmountDue);
            Assert.Equal(0m, command.AmountPaid);
            Assert.False(command.GetIsAcquitted());
            command.SetPaymentAmounts(100.50m, 30.25m);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var command = await context.RdvQueries.SingleAsync(q => q.Id == id, TestContext.Current.CancellationToken);
            Assert.Equal(100.50m, command.AmountDue);
            Assert.Equal(30.25m, command.AmountPaid);
            Assert.Equal(70.25m, command.PaymentSummary!.AmountRemaining);
            Assert.Equal(PaymentStatus.PartiallyPaid, command.PaymentSummary.Status);
            Assert.False(command.GetIsAcquitted());
            command.SetPaymentAmounts(100.50m, 100.50m);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var command = await context.RdvQueries.SingleAsync(q => q.Id == id, TestContext.Current.CancellationToken);
            Assert.True(command.GetIsAcquitted());
            Assert.Equal(0m, command.PaymentSummary!.AmountRemaining);
        }
    }

    [Fact]
    public void Command_amounts_cannot_be_set_by_request_json()
    {
        const string json = """{"AmountDue":100,"AmountPaid":100}""";
        var newtonsoftCommand = Newtonsoft.Json.JsonConvert.DeserializeObject<RdvQuery>(json)!;
        var systemTextCommand = System.Text.Json.JsonSerializer.Deserialize<RdvQuery>(json)!;

        Assert.Null(newtonsoftCommand.AmountDue);
        Assert.Equal(0m, newtonsoftCommand.AmountPaid);
        Assert.Null(systemTextCommand.AmountDue);
        Assert.Equal(0m, systemTextCommand.AmountPaid);
    }

    [Fact]
    public void Payment_migration_adds_only_amounts_without_inventing_historical_payments()
    {
        var migration = new Yavsc.Migrations.InvoicePaymentAmounts();
        Assert.Equal(2, migration.UpOperations.Count);
        var columns = migration.UpOperations
            .Cast<Microsoft.EntityFrameworkCore.Migrations.Operations.AddColumnOperation>()
            .ToList();
        Assert.All(columns, column => Assert.Equal("NominativeServiceCommands", column.Table));
        var due = Assert.Single(columns, column => column.Name == "AmountDue");
        Assert.True(due.IsNullable);
        Assert.Null(due.DefaultValue);
        var paid = Assert.Single(columns, column => column.Name == "AmountPaid");
        Assert.False(paid.IsNullable);
        Assert.Equal(0m, paid.DefaultValue);
        Assert.Equal(2, migration.DownOperations.Count);
    }

    [Fact]
    public void Payment_model_matches_the_postgresql_migration_snapshot()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=model_comparison_only",
                provider => provider.MigrationsAssembly("Yavsc.Org"))
            .Options;
        using var context = new ApplicationDbContext(options);

        Assert.False(context.Database.HasPendingModelChanges());
    }

    private sealed class TestCommand : NominativeServiceCommand
    {
        public override long Id { get; set; }
        public override string Description { get; set; } = "Test";
        public override List<IBillItem> GetBillItems() => throw new NotSupportedException();
    }

    private sealed class TestBillingService : IBillingService
    {
        public Dictionary<string, string> BillingMap { get; } = new()
        {
            [nameof(TestCommand)] = "payment-summary-test"
        };

        public Task<IQuery> GetBillAsync(string billingCode, long queryId) => throw new NotSupportedException();
        public Task<IUserSettings> GetPerformersSettingsAsync(string activityCode, string userId) => throw new NotSupportedException();
    }
}
