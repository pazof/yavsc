using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Yavsc.Org.Tests;

/// <summary>
/// Authorization policy provider used by integration tests. Replaces the
/// production provider in the WebApplicationFactory so that any
/// policy-protected controller can be exercised by sending a
/// <c>X-Test-Role: Administrator</c> header — no login roundtrip, no
/// cookie, no database user.
///
/// The role names accepted in the header are the same as the
/// production <see cref="YavscConstants.AdminGroupName"/>. Any
/// policy that requires one of those roles short-circuits to success
/// when the matching header is present; otherwise the production
/// policy is preserved.
/// </summary>
public sealed class TestAuthPolicyProvider : IAuthorizationPolicyProvider
{
    public const string HeaderName = "X-Test-Role";
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
                var http = ctx.Resource as Microsoft.AspNetCore.Http.HttpContext;
                if (http is null) return false;
                var role = http.Request.Headers[HeaderName].ToString();
                if (string.IsNullOrEmpty(role)) return false;
                // The test does not perform a real login, so the
                // authenticated user has no claims. Attach an
                // in-memory identity carrying the role claim to the
                // HttpContext (ctx.User is read-only) so the
                // production policy's claim requirement is satisfied.
                if (http.User.Identity is null || !http.User.Identity.IsAuthenticated)
                {
                    var identity = new System.Security.Claims.ClaimsIdentity(
                        new[]
                        {
                            new System.Security.Claims.Claim(
                                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                                role),
                        },
                        authenticationType: "TestAuth");
                    http.User = new System.Security.Claims.ClaimsPrincipal(identity);
                }
                return true;
            })
            .Build();
    }
}
