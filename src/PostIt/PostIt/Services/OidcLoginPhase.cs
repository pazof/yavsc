namespace PostIt.Services;

/// <summary>
/// Discrete phases of the OIDC Authorization Code + PKCE flow,
/// surfaced through <see cref="System.IProgress{T}"/> on
/// <c>YavscApiClient.LoginInteractiveAsync</c> so the UI can show
/// exactly where we are — including the parts that happen out of
/// process (the 2nd-instance hand-off via
/// <see cref="SingleInstance.TryHandOffAsync"/> when the OS routes the
/// custom scheme callback to a fresh PostIt process).
///
/// The set is deliberately small: each value is a milestone an
/// operator can grep for in logs / StatusMessage, not a heartbeat.
/// </summary>
public enum OidcLoginPhase
{
    /// <summary>No login in flight (or login has settled).</summary>
    Idle,

    /// <summary>Fetching the OIDC discovery document from the OP.</summary>
    Discovering,

    /// <summary>Handing the authorize URL to the system browser (or
    /// Chrome Custom Tabs on Android).</summary>
    OpeningBrowser,

    /// <summary>The browser is on the IdP's login page; we are waiting
    /// for the OS to deliver <c>postit://callback?code=…</c> back to a
    /// running PostIt instance. On desktop this is the time window
    /// during which the named-pipe server is listening.</summary>
    AwaitingCallback,

    /// <summary>Exchanging the authorization code + PKCE verifier at
    /// the token endpoint and persisting the bundle.</summary>
    ExchangingCode,

    /// <summary>Login succeeded; tokens are on disk and in memory.</summary>
    Success,

    /// <summary>Login failed; check <c>StatusMessage</c> for details.</summary>
    Error,
}
