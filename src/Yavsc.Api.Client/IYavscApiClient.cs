using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Yavsc.Api.Client;

/// <summary>
/// Transport surface that the high-level clients
/// (<see cref="BlogApiClient"/>, <see cref="CircleApiClient"/>,
/// <see cref="BlogAclApiClient"/>) need to do their work.
///
/// <para>This is intentionally a thin, transport-only contract. It
/// does not include the OIDC login / refresh / logout surface —
/// that lives on the concrete <c>YavscApiClient</c> in the
/// consuming application and is wired by the application
/// composition root. Splitting the two keeps <c>Yavsc.Api.Client</c>
/// usable from any host (a CLI, a unit test, a future iOS
/// client) without dragging OIDC, identity, and a <c>Settings</c>
/// POMVO everywhere.</para>
///
/// <para>Implementations are expected to:</para>
/// <list type="bullet">
///   <item>Attach a Bearer access token to every outbound request.</item>
///   <item>Silently refresh the token on a 401 and retry once.</item>
///   <item>Serialise the request body as JSON and deserialise the
///   response body with case-insensitive property matching.</item>
/// </list>
///
/// The exception contract on non-2xx responses is
/// <see cref="HttpRequestException"/> with a message that includes
/// the response body (capped), so callers can surface the
/// server-side validation problem to the UI without losing
/// context.
/// </summary>
public interface IYavscApiClient : IAsyncDisposable
{
    /// <summary>
    /// The configured <see cref="HttpClient"/>. Clients set its
    /// <c>BaseAddress</c> in their constructors to point at the
    /// API host they target.
    /// </summary>
    HttpClient Http { get; }

    /// <summary>Call a JSON endpoint with a typed return value.</summary>
    /// <param name="method">HTTP verb.</param>
    /// <param name="path">Path relative to <see cref="HttpClient.BaseAddress"/>.</param>
    /// <param name="body">Optional request body, serialised as JSON.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<T> CallAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken ct = default);

    /// <summary>Call a JSON endpoint that returns no useful body (DELETE, 204, etc.).</summary>
    Task CallAsync(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken ct = default);
}
