using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Yavsc.Models;
using Yavsc.Models.Payment;
using Yavsc.Server.Helpers;
using Yavsc.Settings;
using Yavsc.Web.Services;

namespace Yavsc.Server.Tests;

[Trait("Category", "non-reg")]
public sealed class ResourceSubscriptionTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 31, 12, 0, 0, TimeSpan.Zero);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = Start;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Gateway : IPayPalSubscriptionGateway
    {
        public PayPalSubscriptionDetails Remote { get; set; } = null!;
        public List<PayPalSubscriptionTransaction> Payments { get; } = [];
        public Task<PayPalSubscriptionDetails> GetAsync(string id, CancellationToken ct) => Task.FromResult(Remote);
        public Task<IReadOnlyList<PayPalSubscriptionTransaction>> TransactionsAsync(string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<PayPalSubscriptionTransaction>>(Payments.ToArray());
        public Task<PayPalSubscriptionDetails> CreateAsync(ResourceSubscription subscription, string returnUrl, string cancelUrl, CancellationToken ct)
            => throw new NotSupportedException();
        public Task CancelAsync(string id, CancellationToken ct) => throw new NotSupportedException();
        public Task VerifyWebhookAsync(JsonElement notification, IReadOnlyDictionary<string, string> headers, CancellationToken ct)
            => throw new NotSupportedException();
    }

    private static ServiceProvider Provider(Clock clock, Gateway gateway, bool useBackendDefaults = false)
    {
        var name = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(name));
        services.AddResourceUsage(new ConfigurationBuilder().Build());
        if (!useBackendDefaults)
            services.Configure<ResourceUsageOptions>(o =>
            {
                o.ApiCallWeight = 0.02m;
                o.CpuSecondWeight = 1m;
                o.BandwidthMbWeight = 0.1m;
                o.StorageMbWeight = 0.3m;
                o.DefaultLimit = 10000m;
            });
        services.Configure<ResourceSubscriptionOptions>(o =>
        {
            o.DiscoveryPlanId = "P-DISCOVERY";
            o.PremiumPlanId = "P-PREMIUM";
        });
        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton<IPayPalSubscriptionGateway>(gateway);
        services.AddScoped<IResourceSubscriptionService, ResourceSubscriptionService>();
        return services.BuildServiceProvider();
    }

    private static async Task<ResourceSubscription> BeginAsync(IServiceProvider services, Gateway gateway, string pack = "Discovery", decimal gross = 0m)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new ApplicationUser { Id = "alice", UserName = "Alice", Email = "alice@example.test" });
        await db.SaveChangesAsync(Ct);
        await ResourceUsageTracker.RecordAsync(db, "alice", "Alice", cpuSeconds: gross, cancellationToken: Ct);
        var service = services.GetRequiredService<IResourceSubscriptionService>();
        var subscription = await service.PrepareAsync("alice", pack, Ct);
        gateway.Remote = new("I-TEST", subscription.Id.ToString("N"), subscription.PlanId, "ACTIVE", Start, Start.AddMonths(1), null);
        await service.BindAsync(subscription, gateway.Remote, Ct);
        return subscription;
    }

    [Theory]
    [InlineData(4000, 0, 10000, 8000)]
    [InlineData(15000, 0, 10000, 10000)]
    [InlineData(0, 0, 30000, 20000)]
    [InlineData(4000, 5000, 30000, 20000)]
    public void Monthly_credit_erases_usage_then_carries_two_thirds_of_unused_allowance(
        decimal gross, decimal previous, decimal allowance, decimal grant)
        => Assert.Equal(grant, ResourceUsageCredit.MonthlyGrant(gross, previous, allowance));

    [Theory]
    [InlineData("Premium", 2.4, 5)]
    [InlineData("Discovery", 1.2, 2)]
    public async Task Paid_disk_quota_uses_standard_reference_and_survives_cancellation_until_anniversary(
        string pack, decimal amount, long multiplier)
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, pack);
        clock.Now = Start.AddMinutes(1);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.SingleAsync(Ct);
        user.DiskQuota = 1234L;
        user.DiskUsage = 5000L;
        await db.SaveChangesAsync(Ct);
        gateway.Payments.Add(new("PAY-DISK", "COMPLETED", amount, "EUR", Start));
        var service = scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        await service.ReconcileAsync(subscription, Ct);
        Assert.Equal(Yavsc.Constants.DefaultFSQ * multiplier, user.GetEffectiveDiskQuota(clock.Now));
        Assert.Equal(Start.AddMonths(1), pack == "Premium" ? user.PremiumDiskQuotaUntil : user.DiscoveryDiskQuotaUntil);
        var usage = await scope.ServiceProvider.GetRequiredService<IResourceUsageService>().GetCurrentAsync("alice", "Alice", Ct);
        Assert.Equal(Yavsc.Constants.DefaultFSQ * multiplier, usage.DiskQuota);
        Assert.Equal(user.PremiumDiskQuotaUntil, usage.PremiumDiskQuotaUntil);
        Assert.Equal(user.DiscoveryDiskQuotaUntil, usage.DiscoveryDiskQuotaUntil);
        gateway.Remote = gateway.Remote with { State = "CANCELLED", NextBillingAt = null };
        await service.ReconcileAsync(subscription, Ct);
        Assert.Equal(Yavsc.Constants.DefaultFSQ * multiplier, user.GetEffectiveDiskQuota(Start.AddMonths(1).AddTicks(-1)));
        Assert.Equal(1234L, user.GetEffectiveDiskQuota(Start.AddMonths(1)));
        Assert.Equal(1234L, user.DiskQuota);
        Assert.Equal(5000L, user.DiskUsage);
    }

    [Theory]
    [InlineData("Discovery", "PENDING", 1.2)]
    [InlineData("Discovery", "FAILED", 1.2)]
    [InlineData("Premium", "PENDING", 2.4)]
    [InlineData("Premium", "FAILED", 2.4)]
    public async Task Unpaid_subscriptions_do_not_grant_disk_bonus(string pack, string state, decimal amount)
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, pack);
        clock.Now = Start.AddMinutes(1);
        gateway.Payments.Add(new("PAY-DISK", state, amount, "EUR", Start));
        await scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>().ReconcileAsync(subscription, Ct);
        var user = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.SingleAsync(Ct);
        Assert.Null(user.PremiumDiskQuotaUntil);
        Assert.Null(user.DiscoveryDiskQuotaUntil);
        Assert.Equal(user.DiskQuota, user.GetEffectiveDiskQuota(clock.Now));
    }

    [Theory]
    [InlineData("Premium", 2.4)]
    [InlineData("Discovery", 1.2)]
    public async Task Refund_revokes_disk_bonus_but_not_another_paid_period(string pack, decimal amount)
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, pack);
        clock.Now = Start.AddMonths(1).AddMinutes(1);
        gateway.Payments.Add(new("PAY-1", "COMPLETED", amount, "EUR", Start));
        gateway.Payments.Add(new("PAY-2", "COMPLETED", amount, "EUR", Start.AddMonths(1)));
        var service = scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        await service.ReconcileAsync(subscription, Ct);
        var user = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.SingleAsync(Ct);
        Assert.Equal(Start.AddMonths(2), pack == "Premium" ? user.PremiumDiskQuotaUntil : user.DiscoveryDiskQuotaUntil);
        gateway.Payments[0] = gateway.Payments[0] with { State = "REFUNDED" };
        await service.ReconcileAsync(subscription, Ct);
        Assert.Equal(Start.AddMonths(2), pack == "Premium" ? user.PremiumDiskQuotaUntil : user.DiscoveryDiskQuotaUntil);
        gateway.Payments[1] = gateway.Payments[1] with { State = "PARTIALLY_REFUNDED" };
        await service.ReconcileAsync(subscription, Ct);
        Assert.Null(user.PremiumDiskQuotaUntil);
        Assert.Null(user.DiscoveryDiskQuotaUntil);
        Assert.Equal(user.DiskQuota, user.GetEffectiveDiskQuota(clock.Now));
    }

    [Fact]
    public async Task Cached_disk_tracker_refreshes_paid_entitlement_before_admission()
    {
        using var provider = Provider(new Clock(), new Gateway());
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new ApplicationUser
        {
            Id = "alice", UserName = "Alice", Email = "alice@example.test", DiskQuota = 10L, DiskUsage = 11L,
            PremiumDiskQuotaUntil = DateTimeOffset.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync(Ct);
        var tracker = new DiskUsageTracker(Options.Create(new SiteSettings { DUUserListLen = 10 }), db);
        Assert.False(tracker.GetSpace("Alice", Yavsc.Constants.DefaultFSQ * 5L));
        using var other = provider.CreateScope();
        var otherDb = other.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await otherDb.Users.SingleAsync(Ct);
        user.PremiumDiskQuotaUntil = DateTimeOffset.UtcNow.AddSeconds(-1);
        await otherDb.SaveChangesAsync(Ct);
        Assert.False(tracker.GetSpace("Alice", 1L));
    }

    [Theory]
    [InlineData("Discovery", 1.2, 1000, 3000)]
    [InlineData("Premium", 2.4, 3000, 1000)]
    public async Task Current_backend_defaults_give_one_or_three_thousand_monthly_points(
        string pack, decimal amount, decimal monthlyPoints, decimal remainingUsage)
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway, useBackendDefaults: true);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, pack, 40000m);
        Assert.Equal(monthlyPoints, subscription.MonthlyPoints);
        gateway.Payments.Add(new("PAY-DEFAULT", "COMPLETED", amount, "EUR", Start));
        clock.Now = Start.AddMinutes(1);
        await scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>().ReconcileAsync(subscription, Ct);
        var usage = await scope.ServiceProvider.GetRequiredService<IResourceUsageService>().GetCurrentAsync("alice", "Alice", Ct);
        Assert.Equal(4000m, usage.GrossConsumption);
        Assert.Equal(remainingUsage, usage.Consumption);
        Assert.Equal(1000m, usage.ConsumptionLimit);
        Assert.Equal(0m, usage.RemainingConsumption);
    }

    [Fact]
    public void Overlapping_paid_periods_fall_back_from_premium_to_discovery_then_base_quota()
    {
        var user = new ApplicationUser
        {
            PremiumDiskQuotaUntil = Start.AddDays(1),
            DiscoveryDiskQuotaUntil = Start.AddDays(2)
        };
        Assert.Equal(Yavsc.Constants.DefaultFSQ * 5L, user.GetEffectiveDiskQuota(Start));
        Assert.Equal(Yavsc.Constants.DefaultFSQ * 2L, user.GetEffectiveDiskQuota(Start.AddDays(1)));
        Assert.Equal(Yavsc.Constants.DefaultFSQ, user.GetEffectiveDiskQuota(Start.AddDays(2)));
    }

    [Fact]
    public void Free_disk_quota_matches_the_standard_database_default()
    {
        var user = new ApplicationUser();
        Assert.Equal(Yavsc.Constants.DefaultFSQ, user.DiskQuota);
        Assert.Equal(Yavsc.Constants.DefaultFSQ, user.GetEffectiveDiskQuota(Start));
    }

    [Fact]
    public void Carried_credit_is_rounded_down_at_database_precision()
        => Assert.Equal(6666.66666666m, ResourceUsageCredit.MonthlyGrant(0m, 0m, 10000m));

    [Theory]
    [InlineData("2026-02-28T00:00:00Z", 1)]
    [InlineData("2026-03-30T23:59:59Z", 1)]
    [InlineData("2026-03-31T00:00:00Z", 2)]
    [InlineData("2027-01-31T00:00:00Z", 12)]
    public void Monthly_anniversaries_do_not_drift_after_a_short_month(string paidAt, int expected)
        => Assert.Equal(expected, ResourceUsageCredit.BillingCycle(
            new DateTimeOffset(Start.UtcDateTime.Date, TimeSpan.Zero), DateTimeOffset.Parse(paidAt)));

    [Fact]
    public async Task First_payment_preserves_counters_erases_usage_and_repeated_notifications_do_not_duplicate_credit()
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, gross: 4000m);
        gateway.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", Start));
        clock.Now = Start.AddMinutes(1);
        var service = scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        await service.ReconcileAsync(subscription, Ct);
        await service.ReconcileAsync(subscription, Ct);
        var usage = await scope.ServiceProvider.GetRequiredService<IResourceUsageService>().GetCurrentAsync("alice", "Alice", Ct);
        Assert.Equal(4000m, usage.CpuSeconds);
        Assert.Equal(4000m, usage.GrossConsumption);
        Assert.Equal(0m, usage.Consumption);
        Assert.Equal(4000m, usage.CreditBalance);
        Assert.Equal(14000m, usage.RemainingConsumption);
        Assert.Single(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().ResourceSubscriptionPayments);
    }

    [Fact]
    public async Task Premium_gives_three_default_quotas_not_three_individual_overrides()
    {
        var gateway = new Gateway();
        using var provider = Provider(new Clock { Now = Start.AddMinutes(1) }, gateway);
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IOptions<ResourceUsageOptions>>().Value.UserLimits["alice"] = 15000m;
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, "Premium", 35000m);
        gateway.Payments.Add(new("PAY-1", "COMPLETED", 2.4m, "EUR", Start.AddMinutes(1)));
        await scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>().ReconcileAsync(subscription, Ct);
        var usage = await scope.ServiceProvider.GetRequiredService<IResourceUsageService>().GetCurrentAsync("alice", "Alice", Ct);
        Assert.Equal(30000m, subscription.MonthlyPoints);
        Assert.Equal(2m, subscription.PriceExcludingTax);
        Assert.Equal(2.4m, subscription.PriceIncludingTax);
        Assert.Equal(5000m, usage.Consumption);
        Assert.Equal(10000m, usage.RemainingConsumption);
    }

    [Fact]
    public async Task Old_carry_is_not_reduced_again_and_remains_spendable_after_cancellation()
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, gross: 4000m);
        var service = scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        gateway.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", Start));
        clock.Now = Start.AddDays(1);
        await service.ReconcileAsync(subscription, Ct);
        gateway.Payments.Add(new("PAY-2", "COMPLETED", 1.2m, "EUR", Start.AddMonths(1)));
        clock.Now = Start.AddMonths(1).AddMinutes(1);
        gateway.Remote = gateway.Remote with { State = "CANCELLED", NextBillingAt = null };
        await service.ReconcileAsync(subscription, Ct);
        var usageService = scope.ServiceProvider.GetRequiredService<IResourceUsageService>();
        var usage = await usageService.GetCurrentAsync("alice", "Alice", Ct);
        Assert.Equal(10666.66666666m, usage.CreditBalance);
        Assert.Equal("CANCELLED", subscription.State);
        await ResourceUsageTracker.RecordAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            "alice", "Alice", cpuSeconds: 5000m, cancellationToken: Ct);
        usage = await usageService.GetCurrentAsync("alice", "Alice", Ct);
        Assert.Equal(5666.66666666m, usage.CreditBalance);
        Assert.Equal(0m, usage.Consumption);
    }

    [Theory]
    [InlineData("PENDING")]
    [InlineData("FAILED")]
    [InlineData("DECLINED")]
    public async Task Approval_and_unpaid_months_never_grant_points(string state)
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, gross: 12000m);
        gateway.Payments.Add(new("PAY-1", state, 1.2m, "EUR", Start));
        clock.Now = Start.AddMinutes(1);
        await scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>().ReconcileAsync(subscription, Ct);
        var usage = await scope.ServiceProvider.GetRequiredService<IResourceUsageService>().GetCurrentAsync("alice", "Alice", Ct);
        Assert.Equal(12000m, usage.Consumption);
        Assert.True(usage.IsQuotaReached);
        Assert.Equal(0m, usage.CreditBalance);
    }

    [Theory]
    [InlineData("REFUNDED")]
    [InlineData("PARTIALLY_REFUNDED")]
    [InlineData("FAILED")]
    public async Task Reversal_removes_the_awarded_credit_once_without_deleting_history(string state)
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, gross: 12000m);
        gateway.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", Start));
        clock.Now = Start.AddMinutes(1);
        var service = scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        await service.ReconcileAsync(subscription, Ct);
        gateway.Payments[0] = gateway.Payments[0] with { State = state };
        await service.ReconcileAsync(subscription, Ct);
        await service.ReconcileAsync(subscription, Ct);
        var usage = await scope.ServiceProvider.GetRequiredService<IResourceUsageService>().GetCurrentAsync("alice", "Alice", Ct);
        Assert.Equal(12000m, usage.Consumption);
        Assert.True(usage.IsQuotaReached);
        Assert.True(Assert.Single(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().ResourceSubscriptionPayments).Reversed);
    }

    [Fact]
    public async Task A_payment_unblocks_a_user_without_changing_the_free_limit()
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, gross: 12000m);
        var usage = scope.ServiceProvider.GetRequiredService<IResourceUsageService>();
        Assert.False(await usage.TryAdmitAsync("alice", "Alice", 1m, 0m, 0, Ct));
        gateway.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", Start));
        clock.Now = Start.AddMinutes(1);
        await scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>().ReconcileAsync(subscription, Ct);
        Assert.True(await usage.TryAdmitAsync("alice", "Alice", 1m, 0m, 0, Ct));
        Assert.Equal(10000m, (await usage.GetCurrentAsync("alice", "Alice", Ct)).ConsumptionLimit);
    }

    [Theory]
    [InlineData("USD", 1.2)]
    [InlineData("EUR", 1)]
    public async Task Wrong_currency_or_net_instead_of_gross_price_does_not_grant_credit(string currency, decimal amount)
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway);
        gateway.Payments.Add(new("PAY-1", "COMPLETED", amount, currency, Start));
        clock.Now = Start.AddMinutes(1);
        var error = await Assert.ThrowsAsync<SubscriptionOperationException>(() =>
            scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>().ReconcileAsync(subscription, Ct));
        Assert.Equal(409, error.StatusCode);
        Assert.Empty(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().ResourceSubscriptionPayments);
    }

    [Fact]
    public async Task Another_custom_account_or_plan_cannot_be_bound()
    {
        var gateway = new Gateway();
        using var provider = Provider(new Clock(), gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway);
        var service = scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        await Assert.ThrowsAsync<SubscriptionOperationException>(() =>
            service.BindAsync(subscription, gateway.Remote with { CustomId = Guid.NewGuid().ToString("N") }, Ct));
        await Assert.ThrowsAsync<SubscriptionOperationException>(() =>
            service.BindAsync(subscription, gateway.Remote with { PlanId = "P-OTHER" }, Ct));
    }

    [Fact]
    public async Task Concurrent_notifications_in_distinct_scopes_award_the_same_payment_once()
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var initialScope = provider.CreateScope();
        await BeginAsync(initialScope.ServiceProvider, gateway, gross: 4000m);
        gateway.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", Start));
        clock.Now = Start.AddMinutes(1);
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        var second = secondScope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        var firstSubscription = Assert.Single(await first.GetAllAsync("alice", Ct));
        var secondSubscription = Assert.Single(await second.GetAllAsync("alice", Ct));
        await Task.WhenAll(
            Task.Run(() => first.ReconcileAsync(firstSubscription, Ct), Ct),
            Task.Run(() => second.ReconcileAsync(secondSubscription, Ct), Ct));
        using var verificationScope = provider.CreateScope();
        var db = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Single(db.ResourceSubscriptionPayments);
        Assert.Equal(8000m, (await db.ResourceUsageRecords.SingleAsync(Ct)).CreditPoints);
    }

    [Fact]
    public async Task An_older_pending_payment_can_complete_after_a_more_recent_month_without_double_credit()
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway, gross: 12000m);
        gateway.Payments.Add(new("PAY-1", "PENDING", 1.2m, "EUR", Start));
        gateway.Payments.Add(new("PAY-2", "COMPLETED", 1.2m, "EUR", Start.AddMonths(1)));
        clock.Now = Start.AddMonths(1).AddMinutes(1);
        var service = scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        await service.ReconcileAsync(subscription, Ct);
        gateway.Payments[0] = gateway.Payments[0] with { State = "COMPLETED" };
        await service.ReconcileAsync(subscription, Ct);
        await service.ReconcileAsync(subscription, Ct);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(2, await db.ResourceSubscriptionPayments.CountAsync(Ct));
        Assert.Equal(1, subscription.LastCreditedCycle);
        Assert.Equal(17333.33333333m, (await db.ResourceUsageRecords.SingleAsync(Ct)).CreditPoints);
    }

    [Fact]
    public async Task Two_different_payment_ids_in_the_same_month_do_not_award_two_credits()
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway);
        gateway.Payments.Add(new("PAY-1", "COMPLETED", 1.2m, "EUR", Start));
        clock.Now = Start.AddMinutes(1);
        var service = scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        await service.ReconcileAsync(subscription, Ct);
        gateway.Payments.Add(new("PAY-2", "COMPLETED", 1.2m, "EUR", Start));
        var error = await Assert.ThrowsAsync<SubscriptionOperationException>(() => service.ReconcileAsync(subscription, Ct));
        Assert.Equal(409, error.StatusCode);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Single(db.ResourceSubscriptionPayments);
        Assert.Equal(6666.66666666m, (await db.ResourceUsageRecords.SingleAsync(Ct)).CreditPoints);
    }

    [Fact]
    public async Task Preparing_the_same_pack_is_idempotent_and_changing_pack_requires_cancellation()
    {
        var gateway = new Gateway();
        using var provider = Provider(new Clock(), gateway);
        using var scope = provider.CreateScope();
        var subscription = await BeginAsync(scope.ServiceProvider, gateway);
        var service = scope.ServiceProvider.GetRequiredService<IResourceSubscriptionService>();
        Assert.Equal(subscription.Id, (await service.PrepareAsync("alice", "Discovery", Ct)).Id);
        await Assert.ThrowsAsync<SubscriptionOperationException>(() => service.PrepareAsync("alice", "Premium", Ct));
        await Assert.ThrowsAsync<SubscriptionOperationException>(() => service.PrepareAsync("alice", "unknown", Ct));
    }
}
