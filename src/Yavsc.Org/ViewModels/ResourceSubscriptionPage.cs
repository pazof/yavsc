#nullable enable
using Yavsc.Abstract.Resources;
using Yavsc.Models.Payment;
using Yavsc.Settings;

namespace Yavsc.ViewModels;

public sealed record ResourceSubscriptionPage(ResourceSubscriptionOptions Prices, decimal FreeQuota,
    ResourceUsageSummary Usage, IReadOnlyList<ResourceSubscription> Subscriptions);
