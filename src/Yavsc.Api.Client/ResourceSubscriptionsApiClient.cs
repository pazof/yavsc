using Yavsc.Abstract.Resources;

namespace Yavsc.Api.Client;

public sealed class ResourceSubscriptionsApiClient(IYavscApiClient api, Func<string> authority)
{
    private string Absolute(string suffix = "")
        => new Uri(new Uri(authority().TrimEnd('/') + "/", UriKind.Absolute),
            "api/v1/resource-subscriptions" + suffix).AbsoluteUri;

    public Task<ResourceSubscriptionsResponse> GetAsync(CancellationToken ct = default)
        => api.CallAsync<ResourceSubscriptionsResponse>(HttpMethod.Get, Absolute(), ct: ct);

    public Task<ResourceSubscriptionCheckoutResponse> CreateAsync(string pack, CancellationToken ct = default)
        => api.CallAsync<ResourceSubscriptionCheckoutResponse>(HttpMethod.Post, Absolute(),
            new CreateResourceSubscriptionRequest { Pack = pack }, ct);

    public Task<ResourceSubscriptionsResponse> RefreshAsync(CancellationToken ct = default)
        => api.CallAsync<ResourceSubscriptionsResponse>(HttpMethod.Post, Absolute("/refresh"), ct: ct);

    public Task<ResourceSubscriptionsResponse> CancelAsync(Guid id, CancellationToken ct = default)
        => api.CallAsync<ResourceSubscriptionsResponse>(HttpMethod.Post, Absolute($"/{id}/cancel"), ct: ct);
}
