#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Models.Payment;
using Yavsc.Server.Helpers;
using Yavsc.Settings;

namespace Yavsc.Web.Services;

public interface IResourceSubscriptionService
{
    Task<List<ResourceSubscription>> GetAllAsync(string userId, CancellationToken ct);
    Task<ResourceSubscription?> FindPayPalAsync(string paypalId, CancellationToken ct);
    Task<ResourceSubscription?> FindPaymentAsync(string transactionId, CancellationToken ct);
    Task<ResourceSubscription> PrepareAsync(string userId, string pack, CancellationToken ct);
    Task BindAsync(ResourceSubscription subscription, PayPalSubscriptionDetails remote, CancellationToken ct);
    Task ReconcileAsync(ResourceSubscription subscription, CancellationToken ct);
}

public sealed class ResourceSubscriptionService(ApplicationDbContext db, IPayPalSubscriptionGateway paypal,
    IOptions<ResourceSubscriptionOptions> options, IOptions<ResourceUsageOptions> usageOptions,
    TimeProvider clock) : IResourceSubscriptionService
{
    public Task<List<ResourceSubscription>> GetAllAsync(string userId, CancellationToken ct)
        => db.ResourceSubscriptions.Where(s => s.UserId == userId).OrderByDescending(s => s.CreatedAt).ToListAsync(ct);

    public Task<ResourceSubscription?> FindPayPalAsync(string paypalId, CancellationToken ct)
        => db.ResourceSubscriptions.SingleOrDefaultAsync(s => s.PayPalId == paypalId, ct);

    public Task<ResourceSubscription?> FindPaymentAsync(string transactionId, CancellationToken ct)
        => db.ResourceSubscriptions.Join(db.ResourceSubscriptionPayments.Where(p => p.Id == transactionId),
            s => s.Id, p => p.SubscriptionId, (s, p) => s).SingleOrDefaultAsync(ct);

    public Task<ResourceSubscription> PrepareAsync(string userId, string pack, CancellationToken ct)
    {
        if (pack is not ("Discovery" or "Premium"))
            throw new SubscriptionOperationException(400, "Pack inconnu.");
        return ResourceUsageTracker.WithUserLockAsync(db, userId, async () =>
        {
            var existing = await db.ResourceSubscriptions.Where(s => s.UserId == userId
                && s.State != "CANCELLED" && s.State != "EXPIRED").SingleOrDefaultAsync(ct);
            if (existing is not null)
            {
                if (existing.Pack != pack)
                    throw new SubscriptionOperationException(409, "Resiliez l'abonnement existant avant de changer de pack.");
                if (existing.State == "CREATING" && existing.CreatedAt < clock.GetUtcNow().AddHours(-24))
                    throw new SubscriptionOperationException(409, "Une creation PayPal ancienne doit etre rapprochee par l'administration avant tout nouvel essai.");
                return existing;
            }
            var settings = options.Value;
            var plan = pack == "Premium" ? settings.PremiumPlanId : settings.DiscoveryPlanId;
            if (string.IsNullOrWhiteSpace(plan))
                throw new SubscriptionOperationException(503, "Ce pack PayPal n'est pas configure.");
            var price = pack == "Premium" ? settings.PremiumPriceExcludingTax : settings.DiscoveryPriceExcludingTax;
            var subscription = new ResourceSubscription
            {
                Id = Guid.NewGuid(), UserId = userId, Pack = pack, PlanId = plan,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(clock.GetUtcNow().ToUnixTimeSeconds()),
                MonthlyPoints = usageOptions.Value.DefaultLimit * (pack == "Premium" ? 3m : 1m),
                PriceExcludingTax = price, VatPercent = settings.VatPercent, PriceIncludingTax = settings.TotalPrice(price)
            };
            db.ResourceSubscriptions.Add(subscription);
            await db.SaveChangesAsync(ct);
            return subscription;
        }, ct);
    }

    public Task BindAsync(ResourceSubscription subscription, PayPalSubscriptionDetails remote, CancellationToken ct)
        => ResourceUsageTracker.WithUserLockAsync(db, subscription.UserId, async () =>
        {
            await db.Entry(subscription).ReloadAsync(ct);
            CheckBinding(subscription, remote);
            subscription.PayPalId = remote.Id;
            subscription.State = remote.State;
            subscription.StartsAt = remote.StartsAt;
            subscription.NextBillingAt = remote.NextBillingAt;
            await db.SaveChangesAsync(ct);
            return true;
        }, ct);

    public async Task ReconcileAsync(ResourceSubscription subscription, CancellationToken ct)
    {
        if (subscription.PayPalId is null) return;
        var remote = await paypal.GetAsync(subscription.PayPalId, ct);
        CheckBinding(subscription, remote);
        var transactions = await paypal.TransactionsAsync(remote.Id, subscription.CreatedAt, clock.GetUtcNow(), ct);
        await ResourceUsageTracker.WithUserLockAsync(db, subscription.UserId, async () =>
        {
            await db.Entry(subscription).ReloadAsync(ct);
            CheckBinding(subscription, remote);
            subscription.State = remote.State;
            subscription.StartsAt = remote.StartsAt;
            subscription.NextBillingAt = remote.NextBillingAt;
            var record = await db.ResourceUsageRecords.Where(r => r.UserId == subscription.UserId)
                .OrderByDescending(r => r.RecordedAtUtc).ThenByDescending(r => r.Id).FirstOrDefaultAsync(ct);
            if (record is not null) await db.Entry(record).ReloadAsync(ct);
            foreach (var payment in transactions.DistinctBy(p => p.Id).OrderBy(p => p.PaidAt).ThenBy(p => p.Id))
            {
                if (payment.State is not ("COMPLETED" or "REFUNDED" or "PARTIALLY_REFUNDED" or "FAILED" or "DECLINED")) continue;
                if (payment.Currency != "EUR" || payment.Amount != subscription.PriceIncludingTax
                    || payment.PaidAt > clock.GetUtcNow() || payment.PaidAt < subscription.CreatedAt)
                    throw new SubscriptionOperationException(409, "La mensualite PayPal ne correspond pas au tarif de cet abonnement.");
                if (remote.StartsAt is null)
                    throw new SubscriptionOperationException(409, "La date de debut PayPal est manquante.");
                var anchor = new DateTimeOffset(remote.StartsAt.Value.UtcDateTime.Date, TimeSpan.Zero);
                if (payment.PaidAt < anchor)
                    throw new SubscriptionOperationException(409, "Le paiement precede le debut de l'abonnement.");
                var cycle = ResourceUsageCredit.BillingCycle(anchor, payment.PaidAt);
                var receipt = await db.ResourceSubscriptionPayments.FindAsync([payment.Id], ct);
                if (receipt is not null)
                {
                    if (receipt.SubscriptionId != subscription.Id)
                        throw new SubscriptionOperationException(409, "La transaction PayPal est deja affectee a un autre abonnement.");
                    if (payment.State != "COMPLETED" && !receipt.Reversed)
                    {
                        if (record is null || record.CreditPoints < receipt.GrantedPoints)
                            throw new SubscriptionOperationException(409, "Le solde des credits ne permet pas le rapprochement du remboursement.");
                        record.CreditPoints -= receipt.GrantedPoints;
                        receipt.Reversed = true;
                    }
                    continue;
                }
                if (payment.State is "FAILED" or "DECLINED") continue;
                if (await db.ResourceSubscriptionPayments.AnyAsync(p => p.SubscriptionId == subscription.Id && p.Cycle == cycle, ct))
                    throw new SubscriptionOperationException(409, "Plusieurs paiements PayPal correspondent a la meme mensualite. Contactez l'administration.");
                if (record is null)
                {
                    var name = await db.Users.Where(u => u.Id == subscription.UserId).Select(u => u.UserName).SingleAsync(ct);
                    record = new ResourceUsageRecord
                    {
                        UserId = subscription.UserId, UserName = name ?? subscription.UserId,
                        UserCreated = subscription.UserId, UserModified = subscription.UserId
                    };
                    db.ResourceUsageRecords.Add(record);
                }
                var gross = usageOptions.Value.Calculate(record.ApiCalls, record.CpuSeconds, record.BandwidthMb, record.StorageMb);
                var grant = payment.State == "COMPLETED"
                    ? ResourceUsageCredit.MonthlyGrant(gross, record.CreditPoints, subscription.MonthlyPoints) : 0m;
                record.CreditPoints += grant;
                db.ResourceSubscriptionPayments.Add(new ResourceSubscriptionPayment
                {
                    Id = payment.Id, SubscriptionId = subscription.Id, Cycle = cycle, PaidAt = payment.PaidAt,
                    GrantedPoints = grant, Reversed = payment.State != "COMPLETED"
                });
                subscription.LastCreditedCycle = Math.Max(subscription.LastCreditedCycle, cycle);
                // Make subsequent transactions in this reconciliation see the receipt.
                await db.SaveChangesAsync(subscription.UserId, ct);
            }
            await db.SaveChangesAsync(subscription.UserId, ct);
            var owner = await db.Users.SingleAsync(u => u.Id == subscription.UserId, ct);
            await db.Entry(owner).ReloadAsync(ct);
            var paidPeriods = await db.ResourceSubscriptions.Where(s => s.UserId == subscription.UserId
                    && (s.Pack == "Premium" || s.Pack == "Discovery") && s.StartsAt != null)
                .Join(db.ResourceSubscriptionPayments.Where(p => !p.Reversed), s => s.Id, p => p.SubscriptionId,
                    (s, p) => new { s.Pack, s.StartsAt, p.Cycle }).ToListAsync(ct);
            owner.PremiumDiskQuotaUntil = paidPeriods.Where(p => p.Pack == "Premium")
                .Select(p => p.StartsAt!.Value.AddMonths(p.Cycle + 1))
                .Cast<DateTimeOffset?>().Max();
            owner.DiscoveryDiskQuotaUntil = paidPeriods.Where(p => p.Pack == "Discovery")
                .Select(p => p.StartsAt!.Value.AddMonths(p.Cycle + 1))
                .Cast<DateTimeOffset?>().Max();
            await db.SaveChangesAsync(subscription.UserId, ct);
            return true;
        }, ct);
    }

    private static void CheckBinding(ResourceSubscription subscription, PayPalSubscriptionDetails remote)
    {
        if (remote.CustomId != subscription.Id.ToString("N") || remote.PlanId != subscription.PlanId
            || (subscription.PayPalId is not null && subscription.PayPalId != remote.Id)
            || (subscription.StartsAt is not null && subscription.StartsAt != remote.StartsAt)
            || (subscription.State is "CANCELLED" or "EXPIRED" && remote.State is not ("CANCELLED" or "EXPIRED"))
            || remote.State is not ("APPROVAL_PENDING" or "APPROVED" or "ACTIVE" or "SUSPENDED" or "CANCELLED" or "EXPIRED"))
            throw new SubscriptionOperationException(409, "L'abonnement PayPal ne correspond pas au compte et au pack attendus.");
    }
}
