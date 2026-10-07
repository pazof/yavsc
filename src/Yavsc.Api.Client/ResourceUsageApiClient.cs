using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Abstract.Resources;

namespace Yavsc.Api.Client;

/// <summary>
/// Thin HTTP client for the PostIt resource-usage reporting flow.
/// It follows the same pattern as the other Yavsc API clients: absolute
/// paths, JSON request/response mapping, and no application logic.
/// </summary>
public sealed class ResourceUsageApiClient
{
    private const string PathPrefix = Constants.ResourceUsagePath;

    private readonly IYavscApiClient _api;
    private readonly Func<string> _businessBaseAddress;

    public ResourceUsageApiClient(IYavscApiClient api, string businessBaseAddress)
        : this(api, () => businessBaseAddress)
    {
    }

    public ResourceUsageApiClient(IYavscApiClient api, Func<string> businessBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _businessBaseAddress = businessBaseAddress ?? throw new ArgumentNullException(nameof(businessBaseAddress));
        _ = ResolveBusinessBaseAddress();
    }

    public Task<ResourceUsageSummary> GetCurrentAsync(CancellationToken ct = default)
    {
        return _api.CallAsync<ResourceUsageSummary>(
            HttpMethod.Get,
            Absolute($"{PathPrefix}/{Constants.ResourceUsageCurrentUserRoute}"),
            ct: ct);
    }

    public Task<List<ResourceUsageSummary>> GetAdminOverviewAsync(CancellationToken ct = default)
    {
        return _api.CallAsync<List<ResourceUsageSummary>>(
            HttpMethod.Get,
            Absolute($"{PathPrefix}/{Constants.ResourceUsageAdminRoute}"),
            ct: ct);
    }

    private string Absolute(string relativePath) => new Uri(ResolveBusinessBaseAddress(), relativePath).ToString();

    private Uri ResolveBusinessBaseAddress()
    {
        var raw = _businessBaseAddress();
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("Business base address is required.");

        return new Uri(raw, UriKind.Absolute);
    }
}
