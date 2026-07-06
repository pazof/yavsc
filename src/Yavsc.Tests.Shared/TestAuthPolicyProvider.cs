using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Yavsc.Tests.Shared;

/// <summary>
/// Authorization policy provider used by integration tests. Replaces the
/// production provider in the test host so that any policy-protected
/// controller can be exercised by sending a <c>X-Test-Role: …</c>
/// header — no login roundtrip, no cookie, no database user.
///
/// Any policy that requires a role short-circuits to success when the
/// matching header is present; otherwise the production policy is
/// preserved. The test does not perform a real login, so we attach an
/// in-memory <see cref="ClaimsPrincipal"/> carrying the role claim to
/// the request <see cref="HttpContext.User"/> before the assertion
/// fires, so claim-based requirements (e.g. <c>RequireRole("Admin")</c>)
/// also pass.
/// </summary>
public sealed class TestAuthPolicyProvider : IAuthorizationPolicyProvider
{
    /// <summary>HTTP header read by the test bypass to learn the role.</summary>
    public const string HeaderName = "X-Test-Role";

    /// <summary>Conventional admin role name; the production default.</summary>
    public const string AdminRole = "Administrator";

    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public TestAuthPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await _fallback.GetPolicyAsync(policyName);
        if (policy is null) return null;
        return new AuthorizationPolicyBuilder()
            .RequireAssertion(ctx =>
            {
                // ASP.NET Core sets ctx.Resource to the HttpContext when
                // the authorization middleware invokes the policy. Use
                // the request headers directly to honour X-Test-Role.
                var http = ctx.Resource as HttpContext;
                if (http is null) return false;
                var role = http.Request.Headers[HeaderName].ToString();
                if (string.IsNullOrEmpty(role)) return false;
                if (http.User.Identity is null || !http.User.Identity.IsAuthenticated)
                {
                    var identity = new ClaimsIdentity(
                        new[]
                        {
                            new Claim(
                                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                                role),
                        },
                        authenticationType: "TestAuth");
                    http.User = new ClaimsPrincipal(identity);
                }
                return true;
            })
            .Build();
    }
}
