using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Abstract.Workflow;

namespace Yavsc.Api.Client;

/// <summary>
/// HTTP client for browsing business activities and their performers.
/// Uses absolute URLs so it can coexist with other Yavsc clients that
/// target a different API host on the same shared transport. The same
/// activity payload also carries the eligible billing forms for a
/// selected performer/activity pair.
/// </summary>
public sealed class ActivityApiClient
{
    private const string PathPrefix = "activity";

    private readonly IYavscApiClient _api;
    private readonly Uri _baseAddress;

    public ActivityApiClient(IYavscApiClient api, string businessBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        if (string.IsNullOrEmpty(businessBaseAddress))
            throw new ArgumentException("Base address is required.", nameof(businessBaseAddress));

        _baseAddress = new Uri(businessBaseAddress, UriKind.Absolute);
    }

    public Task<List<ActivityInfo>> GetCatalogAsync(
        string? parentCode = null,
        CancellationToken ct = default)
    {
        var path = string.IsNullOrWhiteSpace(parentCode)
            ? $"{PathPrefix}/catalog"
            : $"{PathPrefix}/catalog?parentCode={Uri.EscapeDataString(parentCode)}";

        return _api.CallAsync<List<ActivityInfo>>(HttpMethod.Get, Absolute(path), ct: ct);
    }

    public Task<List<PerformerActivity>> GetUsersAsync(
        string activityCode,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(activityCode))
            throw new ArgumentException("Activity code is required.", nameof(activityCode));

        return _api.CallAsync<List<PerformerActivity>>(
            HttpMethod.Get,
            Absolute($"{PathPrefix}/{Uri.EscapeDataString(activityCode)}/users"),
            ct: ct);
    }

    public Task<List<PerformerActivity>> GetPerformersAsync(
        string activityCode,
        CancellationToken ct = default)
        => GetUsersAsync(activityCode, ct);

    private string Absolute(string relativePath) => new Uri(_baseAddress, relativePath).ToString();
}
