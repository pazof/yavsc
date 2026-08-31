using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Models.Haircut;

namespace Yavsc.Api.Client;

/// <summary>
/// HTTP client for posting commands to the business billing routes.
/// Uses absolute URLs so it can coexist with blog-targeting clients on
/// the same shared transport.
/// </summary>
public sealed class BillingApiClient
{
    private const string PathPrefix = "billing";

    private readonly IYavscApiClient _api;
    private readonly Uri _baseAddress;

    public BillingApiClient(IYavscApiClient api, string businessBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        if (string.IsNullOrWhiteSpace(businessBaseAddress))
            throw new ArgumentException("Base address is required.", nameof(businessBaseAddress));

        _baseAddress = new Uri(businessBaseAddress, UriKind.Absolute);
    }

    public Task CreateAsync(string billingCode, object payload, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(billingCode))
            throw new ArgumentException("Billing code is required.", nameof(billingCode));

        return _api.CallAsync(
            HttpMethod.Post,
            Absolute($"{PathPrefix}/{Uri.EscapeDataString(billingCode)}"),
            body: payload,
            ct: ct);
    }

    public Task<List<HairPrestationDto>> GetHairPrestationsAsync(string billingCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(billingCode))
            throw new ArgumentException("Billing code is required.", nameof(billingCode));

        return _api.CallAsync<List<HairPrestationDto>>(
            HttpMethod.Get,
            Absolute($"{PathPrefix}/{Uri.EscapeDataString(billingCode)}/prestations"),
            ct: ct);
    }

    public async Task<List<BillingQuerySummaryDto>> GetQuerySummariesAsync(string billingCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(billingCode))
            throw new ArgumentException("Billing code is required.", nameof(billingCode));

        var items = await _api.CallAsync<List<BillingQuerySummaryDto>>(
            HttpMethod.Get,
            Absolute($"{PathPrefix}/{Uri.EscapeDataString(billingCode)}"),
            ct: ct) ?? new List<BillingQuerySummaryDto>();

        foreach (var item in items)
        {
            item.BillingCode = billingCode;
        }

        return items;
    }

    private string Absolute(string relativePath) => new Uri(_baseAddress, relativePath).ToString();
}