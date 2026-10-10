using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Abstract.Models.Messaging;

namespace Yavsc.Api.Client;

/// <summary>
/// Thin HTTP client for the notifications exposed to PostIt on the home page.
/// </summary>
public sealed class NotificationsApiClient
{
    private const string PathPrefix = Constants.NotificationsPath;

    private readonly IYavscApiClient _api;
    private readonly Func<string> _businessBaseAddress;

    public NotificationsApiClient(IYavscApiClient api, string businessBaseAddress)
        : this(api, () => businessBaseAddress)
    {
    }

    public NotificationsApiClient(IYavscApiClient api, Func<string> businessBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _businessBaseAddress = businessBaseAddress ?? throw new ArgumentNullException(nameof(businessBaseAddress));
        _ = ResolveBusinessBaseAddress();
    }

    public Task<List<Notification>> GetCurrentAsync(CancellationToken ct = default)
    {
        return _api.CallAsync<List<Notification>>(
            HttpMethod.Get,
            Absolute($"{PathPrefix}/{Constants.NotificationsCurrentUserRoute}"),
            ct: ct);
    }

    public Task<List<Notification>> GetAllAsync(CancellationToken ct = default)
    {
        return _api.CallAsync<List<Notification>>(
            HttpMethod.Get,
            Absolute(PathPrefix),
            ct: ct);
    }

    public Task<List<Notification>> GetPublicAsync(CancellationToken ct = default)
        => _api.GetAnonymousAsync<List<Notification>>(Absolute(PathPrefix), ct);

    private string Absolute(string relativePath) => new Uri(ResolveBusinessBaseAddress(), relativePath).ToString();

    private Uri ResolveBusinessBaseAddress()
    {
        var raw = _businessBaseAddress();
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("Business base address is required.");

        return new Uri(raw, UriKind.Absolute);
    }
}
