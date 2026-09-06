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
    private readonly Func<string> _businessBaseAddress;
    private readonly Func<string?> _avatarBaseAddress;

    public ActivityApiClient(IYavscApiClient api, string businessBaseAddress, string? avatarBaseAddress = null)
        : this(
            api,
            () => businessBaseAddress,
            () => avatarBaseAddress)
    {
    }

    public ActivityApiClient(
        IYavscApiClient api,
        Func<string> businessBaseAddress,
        Func<string?>? avatarBaseAddress = null)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _businessBaseAddress = businessBaseAddress ?? throw new ArgumentNullException(nameof(businessBaseAddress));
        _avatarBaseAddress = avatarBaseAddress ?? (() => null);

        // Validate initial values early to fail fast on invalid setup.
        _ = ResolveBusinessBaseAddress();
        _ = ResolveAvatarBaseAddress();
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

    public string BuildAvatarXsUrl(string? userName)
    {
        var siteRoot = new Uri(ResolveAvatarBaseAddress(), "/");

        if (string.IsNullOrWhiteSpace(userName))
        {
            return new Uri(siteRoot, "images/Users/icon_user.xs.png").ToString();
        }

        return new Uri(siteRoot, $"avatars/{Uri.EscapeDataString(userName)}.xs.png").ToString();
    }

    private string Absolute(string relativePath) => new Uri(ResolveBusinessBaseAddress(), relativePath).ToString();

    private Uri ResolveBusinessBaseAddress()
    {
        var raw = _businessBaseAddress();
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("Business base address is required.");

        return new Uri(raw, UriKind.Absolute);
    }

    private Uri ResolveAvatarBaseAddress()
    {
        var raw = _avatarBaseAddress();
        return string.IsNullOrWhiteSpace(raw)
            ? ResolveBusinessBaseAddress()
            : new Uri(raw, UriKind.Absolute);
    }
}
