using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Models.Billing;
using Yavsc.Models.Haircut;
using Yavsc;

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
    private readonly Func<string> _businessBaseAddress;

    public BillingApiClient(IYavscApiClient api, string businessBaseAddress)
        : this(api, () => businessBaseAddress)
    {
    }

    public BillingApiClient(IYavscApiClient api, Func<string> businessBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _businessBaseAddress = businessBaseAddress ?? throw new ArgumentNullException(nameof(businessBaseAddress));

        // Validate initial value early to fail fast on invalid setup.
        _ = ResolveBusinessBaseAddress();
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

    public async Task<BillingQueryDetailsDto> GetQueryAsync(string billingCode, long queryId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(billingCode))
            throw new ArgumentException("Billing code is required.", nameof(billingCode));
        if (queryId <= 0)
            throw new ArgumentOutOfRangeException(nameof(queryId));

        var code = billingCode.Trim();
        var path = Absolute($"{PathPrefix}/{Uri.EscapeDataString(code)}/{queryId}");

        if (string.Equals(code, BillingCodes.Rdv, StringComparison.Ordinal))
        {
            var dto = await _api.CallAsync<RdvQueryResponse>(HttpMethod.Get, path, ct: ct).ConfigureAwait(false);
            return MapRdv(dto, code);
        }

        if (string.Equals(code, BillingCodes.Brush, StringComparison.Ordinal))
        {
            var dto = await _api.CallAsync<HairCutQueryResponse>(HttpMethod.Get, path, ct: ct).ConfigureAwait(false);
            return MapBrush(dto, code);
        }

        if (string.Equals(code, BillingCodes.MBrush, StringComparison.Ordinal))
        {
            var dto = await _api.CallAsync<HairMultiCutQueryResponse>(HttpMethod.Get, path, ct: ct).ConfigureAwait(false);
            return MapMBrush(dto, code);
        }

        throw new NotSupportedException($"Billing code '{code}' is not supported.");
    }

    public Task UpdateAsync(string billingCode, long queryId, BillingQueryDetailsDto payload, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(billingCode))
            throw new ArgumentException("Billing code is required.", nameof(billingCode));
        if (queryId <= 0)
            throw new ArgumentOutOfRangeException(nameof(queryId));
        if (payload is null)
            throw new ArgumentNullException(nameof(payload));

        var code = billingCode.Trim();

        return _api.CallAsync(
            HttpMethod.Put,
            Absolute($"{PathPrefix}/{Uri.EscapeDataString(code)}/{queryId}"),
            body: BuildUpdatePayload(code, queryId, payload),
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

    private static BillingQueryDetailsDto MapRdv(RdvQueryResponse dto, string billingCode)
    {
        return new BillingQueryDetailsDto
        {
            Id = dto.Id,
            BillingCode = billingCode,
            ActivityCode = dto.ActivityCode ?? string.Empty,
            PerformerId = dto.PerformerId ?? string.Empty,
            ClientId = dto.ClientId ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Consent = dto.Consent,
            EventDate = dto.EventDate,
            Status = dto.Status,
            Reason = dto.Reason ?? string.Empty,
            Provisional = dto.Provisional,
            Location = dto.Location is null
                ? null
                : new BillingLocationDto
                {
                    Address = dto.Location.Address ?? string.Empty,
                    Latitude = dto.Location.Latitude,
                    Longitude = dto.Location.Longitude,
                }
        };
    }

    private static BillingQueryDetailsDto MapBrush(HairCutQueryResponse dto, string billingCode)
    {
        return new BillingQueryDetailsDto
        {
            Id = dto.Id,
            BillingCode = billingCode,
            ActivityCode = dto.ActivityCode ?? string.Empty,
            PerformerId = dto.PerformerId ?? string.Empty,
            ClientId = dto.ClientId ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Consent = dto.Consent,
            EventDate = dto.EventDate,
            Status = dto.Status,
            AdditionalInfo = dto.AdditionalInfo ?? string.Empty,
            Provisional = dto.Provisional,
            PrestationId = dto.PrestationId,
            Location = dto.Location is null
                ? null
                : new BillingLocationDto
                {
                    Address = dto.Location.Address ?? string.Empty,
                    Latitude = dto.Location.Latitude,
                    Longitude = dto.Location.Longitude,
                }
        };
    }

    private static BillingQueryDetailsDto MapMBrush(HairMultiCutQueryResponse dto, string billingCode)
    {
        return new BillingQueryDetailsDto
        {
            Id = dto.Id,
            BillingCode = billingCode,
            ActivityCode = dto.ActivityCode ?? string.Empty,
            PerformerId = dto.PerformerId ?? string.Empty,
            ClientId = dto.ClientId ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Consent = dto.Consent,
            EventDate = dto.EventDate,
            Status = dto.Status,
            Provisional = dto.Provisional,
            PrestationIds = (dto.Prestations ?? new List<HairPrestationCollectionItemResponse>())
                .Select(p => p.PrestationId)
                .Where(id => id > 0)
                .ToList(),
            Location = dto.Location is null
                ? null
                : new BillingLocationDto
                {
                    Address = dto.Location.Address ?? string.Empty,
                    Latitude = dto.Location.Latitude,
                    Longitude = dto.Location.Longitude,
                }
        };
    }

    private static object BuildUpdatePayload(string billingCode, long queryId, BillingQueryDetailsDto payload)
    {
        if (string.Equals(billingCode, BillingCodes.Rdv, StringComparison.Ordinal))
        {
            if (payload.EventDate is null)
                throw new ArgumentException("EventDate is required for Rdv.", nameof(payload));

            return new
            {
                Id = queryId,
                ActivityCode = payload.ActivityCode,
                PerformerId = payload.PerformerId,
                ClientId = payload.ClientId,
                Consent = payload.Consent,
                EventDate = payload.EventDate.Value,
                Location = ToLocationPayload(payload.Location),
                Reason = payload.Reason,
                Status = payload.Status,
                Provisional = payload.Provisional,
                Description = payload.Description,
            };
        }

        if (string.Equals(billingCode, BillingCodes.Brush, StringComparison.Ordinal))
        {
            if (payload.PrestationId is null || payload.PrestationId <= 0)
                throw new ArgumentException("PrestationId is required for Brush.", nameof(payload));

            return new
            {
                Id = queryId,
                ActivityCode = payload.ActivityCode,
                PerformerId = payload.PerformerId,
                ClientId = payload.ClientId,
                Consent = payload.Consent,
                EventDate = payload.EventDate,
                Location = ToLocationPayload(payload.Location),
                PrestationId = payload.PrestationId.Value,
                AdditionalInfo = payload.AdditionalInfo,
                Status = payload.Status,
                Provisional = payload.Provisional,
                Description = payload.Description,
            };
        }

        if (string.Equals(billingCode, BillingCodes.MBrush, StringComparison.Ordinal))
        {
            if (payload.EventDate is null)
                throw new ArgumentException("EventDate is required for MBrush.", nameof(payload));
            if (payload.PrestationIds is null || payload.PrestationIds.Count == 0)
                throw new ArgumentException("At least one prestation is required for MBrush.", nameof(payload));

            return new
            {
                Id = queryId,
                ActivityCode = payload.ActivityCode,
                PerformerId = payload.PerformerId,
                ClientId = payload.ClientId,
                Consent = payload.Consent,
                EventDate = payload.EventDate.Value,
                Location = ToLocationPayload(payload.Location),
                Prestations = payload.PrestationIds
                    .Where(id => id > 0)
                    .Select(id => new { PrestationId = id })
                    .ToList(),
                Status = payload.Status,
                Provisional = payload.Provisional,
                Description = payload.Description,
            };
        }

        throw new NotSupportedException($"Billing code '{billingCode}' is not supported.");
    }

    private static object? ToLocationPayload(BillingLocationDto? location)
    {
        if (location is null)
        {
            return null;
        }

        var payload = new Dictionary<string, object?>
        {
            ["Address"] = location.Address,
        };

        if (location.Latitude.HasValue)
        {
            payload["Latitude"] = location.Latitude.Value;
        }

        if (location.Longitude.HasValue)
        {
            payload["Longitude"] = location.Longitude.Value;
        }

        return payload;
    }

    private sealed class BillingLocationResponse
    {
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    private sealed class RdvQueryResponse
    {
        public long Id { get; set; }
        public string? ActivityCode { get; set; }
        public string? PerformerId { get; set; }
        public string? ClientId { get; set; }
        public string? Description { get; set; }
        public bool Consent { get; set; }
        public DateTime EventDate { get; set; }
        public QueryStatus Status { get; set; }
        public string? Reason { get; set; }
        public decimal? Provisional { get; set; }
        public BillingLocationResponse? Location { get; set; }
    }

    private sealed class HairCutQueryResponse
    {
        public long Id { get; set; }
        public string? ActivityCode { get; set; }
        public string? PerformerId { get; set; }
        public string? ClientId { get; set; }
        public string? Description { get; set; }
        public bool Consent { get; set; }
        public DateTime? EventDate { get; set; }
        public QueryStatus Status { get; set; }
        public decimal? Provisional { get; set; }
        public long PrestationId { get; set; }
        public string? AdditionalInfo { get; set; }
        public BillingLocationResponse? Location { get; set; }
    }

    private sealed class HairMultiCutQueryResponse
    {
        public long Id { get; set; }
        public string? ActivityCode { get; set; }
        public string? PerformerId { get; set; }
        public string? ClientId { get; set; }
        public string? Description { get; set; }
        public bool Consent { get; set; }
        public DateTime EventDate { get; set; }
        public QueryStatus Status { get; set; }
        public decimal? Provisional { get; set; }
        public BillingLocationResponse? Location { get; set; }
        public List<HairPrestationCollectionItemResponse>? Prestations { get; set; }
    }

    private sealed class HairPrestationCollectionItemResponse
    {
        public long PrestationId { get; set; }
    }
}
