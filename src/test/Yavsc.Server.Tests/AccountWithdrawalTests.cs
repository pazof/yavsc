using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Models.Payment;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Yavsc.Web.Services;

namespace Yavsc.Server.Tests;

[Trait("Category", "non-reg")]
public sealed class AccountWithdrawalTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "yavsc-withdrawal-" + Guid.NewGuid().ToString("N"));
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 11, 8, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Gateway : IPayPalSubscriptionGateway
    {
        public bool FailCancellation { get; set; }
        public int CancelCount { get; set; }
        public Task CancelAsync(string id, CancellationToken ct)
        {
            if (FailCancellation) throw new HttpRequestException("Payment service unavailable");
            CancelCount++;
            return Task.CompletedTask;
        }
        public Task<PayPalSubscriptionDetails> GetAsync(string id, CancellationToken ct)
            => Task.FromResult(new PayPalSubscriptionDetails(id, "", "", "ACTIVE", null, null, null));
        public Task<PayPalSubscriptionDetails> CreateAsync(ResourceSubscription s, string r, string c, CancellationToken ct)
            => throw new NotSupportedException();
        public Task<IReadOnlyList<PayPalSubscriptionTransaction>> TransactionsAsync(string id, DateTimeOffset f, DateTimeOffset t, CancellationToken ct)
            => throw new NotSupportedException();
        public Task VerifyWebhookAsync(JsonElement n, IReadOnlyDictionary<string, string> h, CancellationToken ct)
            => throw new NotSupportedException();
    }

    private ServiceProvider Provider(Clock clock, Gateway gateway, Microsoft.Data.Sqlite.SqliteConnection? connection = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var databaseName = Guid.NewGuid().ToString();
        services.AddDbContext<ApplicationDbContext>(o =>
        {
            if (connection is null) o.UseInMemoryDatabase(databaseName);
            else o.UseSqlite(connection);
        });
        services.AddIdentityCore<ApplicationUser>().AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton<IPayPalSubscriptionGateway>(gateway);
        services.Configure<SiteSettings>(o =>
        {
            o.UserFilesRoot = Path.Combine(root, "files");
            o.Avatars = Path.Combine(root, "avatars");
        });
        services.Configure<PodcastOptions>(o => o.RootPath = Path.Combine(root, "podcasts"));
        services.AddScoped<IAccountWithdrawalService, AccountWithdrawalService>();
        services.AddScoped<IPaymentResidueService, PaymentResidueService>();
        return services.BuildServiceProvider();
    }

    private static async Task<ApplicationUser> SeedAsync(ApplicationDbContext db, string id = "alice")
    {
        var user = new ApplicationUser
        {
            Id = id, UserName = id, Email = id + "@example.test", FullName = "Full name",
            PasswordHash = "old", PhoneNumber = "123", SecurityStamp = "old"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(Ct);
        return user;
    }

    [Fact]
    public async Task Withdrawal_erases_files_revokes_identity_retains_payments_and_is_idempotent()
    {
        var clock = new Clock();
        var gateway = new Gateway();
        using var provider = Provider(clock, gateway);
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await SeedAsync(db);
        var subscription = new ResourceSubscription { UserId = user.Id, Id = Guid.NewGuid(), State = "ACTIVE", PayPalId = "I-TEST" };
        db.ResourceSubscriptions.Add(subscription);
        db.ResourceSubscriptionPayments.Add(new() { Id = "PAY-1", SubscriptionId = subscription.Id });
        await db.SaveChangesAsync(Ct);
        var directory = Path.Combine(root, "files", "alice");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(Path.Combine(directory, "personal.txt"), "private", Ct);
        Directory.CreateDirectory(Path.Combine(root, "avatars"));
        await File.WriteAllTextAsync(Path.Combine(root, "avatars", "alice.png"), "avatar", Ct);
        var podcast = Path.Combine(root, "podcasts", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(podcast);
        await File.WriteAllTextAsync(Path.Combine(podcast, "index.json"),
            """{"Manifest":{"Podcast":{"OwnerId":"alice"}}}""", Ct);
        var other = Path.Combine(root, "files", "bob");
        Directory.CreateDirectory(other);
        await File.WriteAllTextAsync(Path.Combine(other, "keep.txt"), "other", Ct);
        var service = scope.ServiceProvider.GetRequiredService<IAccountWithdrawalService>();
        await service.WithdrawAsync(user.Id, Ct);
        Assert.False(Directory.Exists(directory));
        Assert.False(Directory.Exists(podcast));
        Assert.False(File.Exists(Path.Combine(root, "avatars", "alice.png")));
        Assert.True(File.Exists(Path.Combine(other, "keep.txt")));
        Assert.Equal(clock.Now, user.WithdrawnAt);
        Assert.Null(user.PasswordHash);
        Assert.Null(user.PhoneNumber);
        Assert.Null(user.FullName);
        Assert.EndsWith("@invalid.invalid", user.Email);
        Assert.Equal(DateTimeOffset.MaxValue, user.LockoutEnd);
        Assert.Equal("CANCELLED", subscription.State);
        Assert.Single(db.ResourceSubscriptionPayments);
        await ResourceUsageTracker.RecordAsync(db, user.Id, "alice", apiCalls: 1m, cancellationToken: Ct);
        Assert.Empty(db.ResourceUsageRecords);
        var subscriptionService = new ResourceSubscriptionService(db, gateway,
            Options.Create(new Yavsc.Settings.ResourceSubscriptionOptions()),
            Options.Create(new Yavsc.Settings.ResourceUsageOptions()), clock);
        var denied = await Assert.ThrowsAsync<SubscriptionOperationException>(() =>
            subscriptionService.PrepareAsync(user.Id, "Discovery", Ct));
        Assert.Equal(409, denied.StatusCode);
        clock.Now = clock.Now.AddMonths(1);
        var withdrawalTime = user.WithdrawnAt;
        await service.WithdrawAsync(user.Id, Ct);
        Assert.Equal(withdrawalTime, user.WithdrawnAt);
        Assert.Equal(1, gateway.CancelCount);
    }

    [Fact]
    public async Task Withdrawal_refuses_live_podcast_and_can_finish_when_recording_is_stopped()
    {
        using var provider = Provider(new Clock(), new Gateway());
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await SeedAsync(db);
        var podcast = Path.Combine(root, "podcasts", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(podcast);
        var index = Path.Combine(podcast, "index.json");
        await File.WriteAllTextAsync(index, """{"Manifest":{"Podcast":{"OwnerId":"alice","State":"Live"}}}""", Ct);
        var service = scope.ServiceProvider.GetRequiredService<IAccountWithdrawalService>();
        await Assert.ThrowsAsync<SubscriptionOperationException>(() => service.WithdrawAsync(user.Id, Ct));
        Assert.True(Directory.Exists(podcast));
        Assert.Null(user.WithdrawnAt);
        await File.WriteAllTextAsync(index, """{"Manifest":{"Podcast":{"OwnerId":"alice","State":"Completed"}}}""", Ct);
        await service.WithdrawAsync(user.Id, Ct);
        Assert.False(Directory.Exists(podcast));
    }

    [Fact]
    public async Task Withdrawal_refuses_symbolic_links_without_deleting_other_users_files()
    {
        using var provider = Provider(new Clock(), new Gateway());
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await SeedAsync(db);
        var other = Path.Combine(root, "files", "bob");
        Directory.CreateDirectory(other);
        await File.WriteAllTextAsync(Path.Combine(other, "keep.txt"), "other", Ct);
        Directory.CreateSymbolicLink(Path.Combine(root, "files", "alice"), other);
        await Assert.ThrowsAsync<IOException>(() =>
            scope.ServiceProvider.GetRequiredService<IAccountWithdrawalService>().WithdrawAsync(user.Id, Ct));
        Assert.True(File.Exists(Path.Combine(other, "keep.txt")));
        Assert.Null(user.WithdrawnAt);
    }


    [Fact]
    public async Task Failed_payment_cancellation_blocks_account_without_claiming_withdrawal_and_can_be_retried()
    {
        var gateway = new Gateway { FailCancellation = true };
        using var provider = Provider(new Clock(), gateway);
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await SeedAsync(db);
        db.ResourceSubscriptions.Add(new() { Id = Guid.NewGuid(), UserId = user.Id, PayPalId = "I-TEST", State = "ACTIVE" });
        await db.SaveChangesAsync(Ct);
        var service = scope.ServiceProvider.GetRequiredService<IAccountWithdrawalService>();
        await Assert.ThrowsAsync<HttpRequestException>(() => service.WithdrawAsync(user.Id, Ct));
        Assert.NotNull(user.WithdrawalRequestedAt);
        Assert.Null(user.WithdrawnAt);
        gateway.FailCancellation = false;
        await service.WithdrawAsync(user.Id, Ct);
        Assert.NotNull(user.WithdrawnAt);
    }

    [Fact]
    public async Task Invalid_owner_path_never_deletes_a_storage_root()
    {
        using var provider = Provider(new Clock(), new Gateway());
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await SeedAsync(db);
        user.UserName = "..";
        await db.SaveChangesAsync(Ct);
        Directory.CreateDirectory(root);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            scope.ServiceProvider.GetRequiredService<IAccountWithdrawalService>().WithdrawAsync(user.Id, Ct));
        Assert.True(Directory.Exists(root));
        Assert.Null(user.WithdrawnAt);
    }

    [Fact]
    public async Task Purge_applies_exact_anniversary_including_leap_day_and_preserves_active_and_pending_accounts()
    {
        var clock = new Clock { Now = new(2025, 2, 28, 12, 0, 0, TimeSpan.Zero) };
        using var provider = Provider(clock, new Gateway());
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var eligible = await SeedAsync(db, "eligible");
        eligible.WithdrawnAt = new(2024, 2, 29, 12, 0, 0, TimeSpan.Zero);
        var young = await SeedAsync(db, "young");
        young.WithdrawnAt = new(2024, 2, 29, 12, 0, 1, TimeSpan.Zero);
        var pending = await SeedAsync(db, "pending");
        pending.WithdrawalRequestedAt = clock.Now.AddYears(-2);
        await SeedAsync(db, "active");
        foreach (var userId in new[] { "eligible", "young", "pending", "active" })
        {
            var subscription = new ResourceSubscription { Id = Guid.NewGuid(), UserId = userId };
            db.ResourceSubscriptions.Add(subscription);
            db.ResourceSubscriptionPayments.Add(new() { Id = "PAY-" + userId, SubscriptionId = subscription.Id });
        }
        db.PerformerDonations.Add(new()
        {
            Id = Guid.NewGuid(), DonorId = eligible.Id, PerformerId = "active",
            RecipientEmail = "recipient@example.test", Amount = 1m
        });
        db.InvoicePayments.Add(new() { CommandId = 1, Reference = "accounting-kept", Amount = 1m });
        await db.SaveChangesAsync(Ct);
        var service = scope.ServiceProvider.GetRequiredService<IPaymentResidueService>();
        var result = await service.PurgeAsync(Ct);
        Assert.Equal(new Yavsc.Abstract.Administration.PaymentResiduePurgeResult(1, 1, 1, 1), result);
        Assert.Single(db.InvoicePayments);
        Assert.Empty(db.PerformerDonations);
        Assert.Equal(clock.Now.AddSeconds(1), await service.GetNextExpiryAsync(Ct));
        Assert.Equal(3, await db.ResourceSubscriptions.CountAsync(Ct));
        Assert.Equal(3, await db.ResourceSubscriptionPayments.CountAsync(Ct));
        Assert.Equal(new Yavsc.Abstract.Administration.PaymentResiduePurgeResult(0, 0, 0), await service.PurgeAsync(Ct));
    }

    [Fact]
    public async Task Relational_purge_removes_legacy_payment_links_without_deleting_account_identities()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(Ct);
        var clock = new Clock();
        using var provider = Provider(clock, new Gateway(), connection);
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync(Ct);
        var user = await SeedAsync(db);
        user.WithdrawnAt = clock.Now.AddYears(-1);
        db.PayPalPayment.Add(new()
        {
            CreationToken = "technical-token", ExecutorId = user.Id, State = "SUCCESS",
            Links = [new() { HRef = "https://example.test/payment", Method = "GET" }]
        });
        await db.SaveChangesAsync(Ct);
        var report = await scope.ServiceProvider.GetRequiredService<IPaymentResidueService>().PurgeAsync(Ct);
        Assert.Equal(1, report.LegacyPayments);
        Assert.Empty(db.PayPalPayment);
        Assert.Empty(db.HyperLink);
        Assert.Single(db.Users);
        Assert.Equal(0, (await scope.ServiceProvider.GetRequiredService<IPaymentResidueService>().PurgeAsync(Ct)).Users);
        Assert.Null(await scope.ServiceProvider.GetRequiredService<IPaymentResidueService>().GetNextExpiryAsync(Ct));
    }

    [Fact]
    public async Task Old_authenticated_session_is_refused_even_before_withdrawal_completion()
    {
        using var provider = Provider(new Clock(), new Gateway());
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await SeedAsync(db);
        user.WithdrawalRequestedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(Ct);
        var context = new DefaultHttpContext
        {
            User = new(new System.Security.Claims.ClaimsIdentity([new("sub", user.Id)], "Bearer"))
        };
        context.Response.Body = new MemoryStream();
        var middleware = new WithdrawnAccountMiddleware(_ => throw new InvalidOperationException("Must not execute"));
        await middleware.InvokeAsync(context, db);
        Assert.Equal(401, context.Response.StatusCode);
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
