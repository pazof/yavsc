using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Yavsc.Api.Client;

/// <summary>
/// HTTP client for the estimate routes (<c>api/v1/estimate</c>,
/// served by <c>EstimateApiController</c>). Follows the same
/// DTO↔path mapper shape as <see cref="BillingApiClient"/>: all
/// transport concerns (base URL, JSON, Bearer auth, silent refresh
/// on 401) are delegated to <see cref="IYavscApiClient"/>.
/// </summary>
public sealed class EstimateApiClient
{
    private const string PathPrefix = "estimate";

    private readonly IYavscApiClient _api;
    private readonly Func<string> _businessBaseAddress;

    public EstimateApiClient(IYavscApiClient api, string businessBaseAddress)
        : this(api, () => businessBaseAddress)
    {
    }

    public EstimateApiClient(IYavscApiClient api, Func<string> businessBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _businessBaseAddress = businessBaseAddress ?? throw new ArgumentNullException(nameof(businessBaseAddress));

        // Validate initial value early to fail fast on invalid setup.
        _ = ResolveBusinessBaseAddress();
    }

    /// <summary>
    /// Lists the estimates of the given owner; when <paramref name="ownerId"/>
    /// is null, the server falls back to the current user.
    /// </summary>
    public Task<List<EstimateDto>> GetEstimatesAsync(string? ownerId = null, CancellationToken ct = default)
    {
        var path = string.IsNullOrWhiteSpace(ownerId)
            ? PathPrefix
            : $"{PathPrefix}?ownerId={Uri.EscapeDataString(ownerId)}";

        return _api.CallAsync<List<EstimateDto>>(HttpMethod.Get, Absolute(path), ct: ct);
    }

    public Task<EstimateDto> GetEstimateAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        return _api.CallAsync<EstimateDto>(HttpMethod.Get, Absolute($"{PathPrefix}/{id}"), ct: ct);
    }

    /// <summary>
    /// Creates an estimate. When <see cref="EstimateDto.CommandId"/> is set,
    /// the server also stamps the linked command as validated.
    /// </summary>
    public async Task<EstimateCreatedDto> CreateAsync(EstimateDto estimate, CancellationToken ct = default)
    {
        if (estimate is null)
            throw new ArgumentNullException(nameof(estimate));

        var created = await _api.CallAsync<EstimateCreatedDto>(
            HttpMethod.Post,
            Absolute(PathPrefix),
            body: estimate,
            ct: ct).ConfigureAwait(false);

        return created ?? new EstimateCreatedDto();
    }

    public Task UpdateAsync(long id, EstimateDto estimate, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        if (estimate is null)
            throw new ArgumentNullException(nameof(estimate));

        estimate.Id = id;

        return _api.CallAsync(
            HttpMethod.Put,
            Absolute($"{PathPrefix}/{id}"),
            body: estimate,
            ct: ct);
    }

    public Task DeleteAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        return _api.CallAsync(HttpMethod.Delete, Absolute($"{PathPrefix}/{id}"), ct: ct);
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
