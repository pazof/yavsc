using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Yavsc.Api.Client;

/// <summary>
/// HTTP client for the front-office query routes
/// (<c>api/v1/front/query/accept</c> / <c>.../reject</c>, served by
/// <see cref="Yavsc.ApiControllers.FrontOfficeApiController"/>). Same
/// DTO↔path mapper shape as <see cref="EstimateApiClient"/>: transport
/// (base URL, JSON, Bearer auth, silent refresh on 401) is delegated
/// to <see cref="IYavscApiClient"/>.
///
/// The server resolves the caller's role (provider vs client vs admin)
/// from the authenticated user compared to the query's
/// <c>PerformerId</c>/<c>ClientId</c>; the client does not declare a
/// role. <paramref name="billingCode"/> is the estimate's
/// <see cref="EstimateDto.CommandType"/> and <paramref name="queryId"/>
/// is its <see cref="EstimateDto.CommandId"/>.
/// </summary>
public sealed class FrontOfficeApiClient
{
    private const string PathPrefix = "front";

    private readonly IYavscApiClient _api;
    private readonly Func<string> _businessBaseAddress;

    public FrontOfficeApiClient(IYavscApiClient api, string businessBaseAddress)
        : this(api, () => businessBaseAddress)
    {
    }

    public FrontOfficeApiClient(IYavscApiClient api, Func<string> businessBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _businessBaseAddress = businessBaseAddress ?? throw new ArgumentNullException(nameof(businessBaseAddress));
        _ = ResolveBusinessBaseAddress();
    }

    /// <summary>
    /// Accept (validate) the query <paramref name="queryId"/> under
    /// billing code <paramref name="billingCode"/>. When
    /// <paramref name="body"/> carries a non-empty signature, the
    /// server stages it on the query's linked estimate and stamps the
    /// matching validation date.
    /// </summary>
    public async Task<QueryAcceptanceResponseDto> AcceptQueryAsync(
        string billingCode,
        long queryId,
        QueryAcceptanceRequestDto body,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(billingCode))
            throw new ArgumentException("billingCode is required.", nameof(billingCode));
        if (queryId <= 0)
            throw new ArgumentOutOfRangeException(nameof(queryId));
        if (body is null)
            throw new ArgumentNullException(nameof(body));

        var path = $"{PathPrefix}/query/accept?billingCode={Uri.EscapeDataString(billingCode)}&queryId={queryId}";
        var response = await _api.CallAsync<QueryAcceptanceResponseDto>(
            HttpMethod.Post,
            Absolute(path),
            body: body,
            ct: ct).ConfigureAwait(false);

        return response ?? new QueryAcceptanceResponseDto { QueryId = queryId };
    }

    /// <summary>
    /// Reject the query <paramref name="queryId"/> under billing code
    /// <paramref name="billingCode"/>. A signature is not meaningful on
    /// a rejection; the server refuses one with 400, so this sends no
    /// body.
    /// </summary>
    public async Task<QueryAcceptanceResponseDto> RejectQueryAsync(
        string billingCode,
        long queryId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(billingCode))
            throw new ArgumentException("billingCode is required.", nameof(billingCode));
        if (queryId <= 0)
            throw new ArgumentOutOfRangeException(nameof(queryId));

        var path = $"{PathPrefix}/query/reject?billingCode={Uri.EscapeDataString(billingCode)}&queryId={queryId}";
        var response = await _api.CallAsync<QueryAcceptanceResponseDto>(
            HttpMethod.Post,
            Absolute(path),
            body: null,
            ct: ct).ConfigureAwait(false);

        return response ?? new QueryAcceptanceResponseDto { QueryId = queryId };
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