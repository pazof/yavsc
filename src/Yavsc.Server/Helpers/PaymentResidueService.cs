using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yavsc.Abstract.Administration;
using Yavsc.Models;

namespace Yavsc.Server.Helpers;

public interface IPaymentResidueService
{
    Task<PaymentResiduePurgeResult> PurgeAsync(CancellationToken ct);
    Task<DateTimeOffset?> GetNextExpiryAsync(CancellationToken ct);
}

public sealed class PaymentResidueService(ApplicationDbContext db, TimeProvider clock,
    ILogger<PaymentResidueService> logger) : IPaymentResidueService
{
    private IQueryable<ApplicationUser> AccountsWithResidues()
        => db.Users.AsNoTracking().Where(u => u.WithdrawnAt != null
            && (db.ResourceSubscriptions.Any(s => s.UserId == u.Id)
                || db.PayPalPayment.Any(p => p.ExecutorId == u.Id)
                || db.PerformerDonations.Any(d => d.DonorId == u.Id || d.PerformerId == u.Id)));

    public async Task<DateTimeOffset?> GetNextExpiryAsync(CancellationToken ct)
    {
        var dates = await AccountsWithResidues().Select(u => u.WithdrawnAt).ToListAsync(ct);
        return dates.Select(d => d?.AddYears(1)).Min();
    }

    public async Task<PaymentResiduePurgeResult> PurgeAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var candidates = await AccountsWithResidues()
            .Select(u => new { u.Id, u.WithdrawnAt }).ToListAsync(ct);
        var ids = candidates.Where(u => u.WithdrawnAt!.Value.AddYears(1) <= now).Select(u => u.Id);
        var users = 0;
        var subscriptions = 0;
        var payments = 0;
        var donations = 0;
        var legacyPayments = 0;
        foreach (var id in ids)
        {
            var count = await ResourceUsageTracker.WithUserLockAsync(db, id, async () =>
            {
                var owner = await db.Users.AsNoTracking().SingleAsync(u => u.Id == id, ct);
                if (owner.WithdrawnAt is null || owner.WithdrawnAt.Value.AddYears(1) > now)
                    throw new InvalidOperationException("Withdrawal retention deadline changed during purge.");
                var rows = await db.ResourceSubscriptions.Where(s => s.UserId == id).ToListAsync(ct);
                var rowIds = rows.Select(s => s.Id).ToArray();
                var receipts = await db.ResourceSubscriptionPayments.Where(p => rowIds.Contains(p.SubscriptionId)).ToListAsync(ct);
                var donationRows = await db.PerformerDonations.Where(p => p.DonorId == id || p.PerformerId == id).ToListAsync(ct);
                var legacy = await db.PayPalPayment.Include(p => p.Links).Where(p => p.ExecutorId == id).ToListAsync(ct);
                foreach (var payment in legacy)
                    if (payment.Links is not null) db.HyperLink.RemoveRange(payment.Links);
                db.PerformerDonations.RemoveRange(donationRows);
                db.PayPalPayment.RemoveRange(legacy);
                db.ResourceSubscriptionPayments.RemoveRange(receipts);
                await db.SaveChangesAsync(ct);
                db.ResourceSubscriptions.RemoveRange(rows);
                await db.SaveChangesAsync(ct);
                return (Subscriptions: rows.Count, Payments: receipts.Count, Donations: donationRows.Count, LegacyPayments: legacy.Count);
            }, ct);
            if (count.Subscriptions != 0 || count.Payments != 0 || count.Donations != 0 || count.LegacyPayments != 0) users++;
            subscriptions += count.Subscriptions;
            payments += count.Payments;
            donations += count.Donations;
            legacyPayments += count.LegacyPayments;
        }
        logger.LogInformation("Purged payment residues: {Users} withdrawn accounts, {Subscriptions} subscriptions, {Payments} receipts, {Donations} donations, {LegacyPayments} legacy payments",
            users, subscriptions, payments, donations, legacyPayments);
        return new(users, subscriptions, payments, donations, legacyPayments);
    }
}
