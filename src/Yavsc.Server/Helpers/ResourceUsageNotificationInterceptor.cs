#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Models.Messaging;
using Yavsc.Models;
using Yavsc.Settings;

namespace Yavsc.Server.Helpers;

public sealed class ResourceUsageNotificationInterceptor(IOptions<ResourceUsageOptions> options) : SaveChangesInterceptor
{
    private static readonly int[] Thresholds = [50, 75, 90, 95, 99];

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddNotifications(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddNotifications(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void AddNotifications(DbContext? context)
    {
        if (context is not ApplicationDbContext db) return;
        db.ChangeTracker.DetectChanges();
        var policy = options.Value;
        foreach (var entry in db.ChangeTracker.Entries<ResourceUsageRecord>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified).ToArray())
        {
            var record = entry.Entity;
            var previous = entry.State == EntityState.Added ? 0m :
                ResourceUsageCredit.NetConsumption(policy.Calculate(
                    entry.OriginalValues.GetValue<decimal>(nameof(record.ApiCalls)),
                    entry.OriginalValues.GetValue<decimal>(nameof(record.CpuSeconds)),
                    entry.OriginalValues.GetValue<decimal>(nameof(record.BandwidthMb)),
                    entry.OriginalValues.GetValue<decimal>(nameof(record.StorageMb))),
                    entry.OriginalValues.GetValue<decimal>(nameof(record.CreditPoints)));
            var current = ResourceUsageCredit.NetConsumption(policy.Calculate(
                record.ApiCalls, record.CpuSeconds, record.BandwidthMb, record.StorageMb), record.CreditPoints);
            var limit = policy.GetLimit(record.UserId);
            var threshold = Thresholds.LastOrDefault(value =>
                previous < limit * value / 100m && current >= limit * value / 100m);
            if (threshold == 0) continue;

            var target = $"user/{record.UserId}";
            var tag = $"resource-usage/{threshold}";
            // A failed save may be retried with the same pending notification.
            if (db.ChangeTracker.Entries<Notification>().Any(e =>
                e.State == EntityState.Added && e.Entity.Target == target && e.Entity.tag == tag))
                continue;
            db.Notification.Add(new Notification
            {
                Target = target,
                tag = tag,
                title = $"Consommation des ressources : seuil de {threshold} % atteint",
                body = $"Votre consommation globale de ressources, après déduction des crédits, a atteint {threshold} % de votre limite. Consultez votre consommation et vos abonnements pour éviter le blocage des opérations à la limite.",
                click_action = "Fermer"
            });
        }
    }
}
