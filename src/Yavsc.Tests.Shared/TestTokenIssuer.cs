using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Yavsc.Tests.Shared;

/// <summary>
/// Mints HS256-signed JWTs for integration tests. The signing key is
/// held in a static field shared with the test host's
/// <c>AddJwtBearer</c> registration: whatever the host validates
/// against, this issuer signs with.
///
/// <para>
/// HS256 (symmetric) is the right choice for a unit-test issuer:
/// no key generation ceremony, no PEM round-trip, no asymmetric
/// crypto on the hot path. The key never leaves the test process.
/// Production continues to validate against the OIDC authority via
/// <c>AddYavscJwtBearer</c> — this issuer is *only* for the
/// in-process test host.
/// </para>
/// </summary>
public static class TestTokenIssuer
{
    /// <summary>
    /// Symmetric signing key shared with the test host's
    /// <c>TokenValidationParameters.IssuerSigningKey</c>.
    /// 32 bytes of zeros is enough entropy for HS256 *within the test
    /// process*; the assertion we care about is "does the policy
    /// evaluate a properly-signed token", not "is the key unguessable
    /// by an attacker" (there is no attacker here).
    /// </summary>
    public static readonly SymmetricSecurityKey SigningKey =
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('k', 32)));

    /// <summary>
    /// Issuer stamped into the <c>iss</c> claim and checked by the
    /// test host. Must match
    /// <c>TokenValidationParameters.ValidIssuer</c>.
    /// </summary>
    public const string Issuer = "yavsc-test-issuer";

    /// <summary>
    /// Audience stamped into the <c>aud</c> claim. The test host
    /// does not validate audience (production may), so this is here
    /// for shape only.
    /// </summary>
    public const string Audience = "yavsc-test";

    private static bool _inboundClaimTypeMapCleared;

    /// <summary>
    /// Mint a JWT carrying the given <paramref name="subject"/> as
    /// the <c>sub</c> claim, a single <c>scope</c> claim with value
    /// <paramref name="scope"/>, and any additional
    /// <paramref name="extraClaims"/>. Token is valid for one hour
    /// from now.
    /// </summary>
    /// <param name="subject">Value of the <c>sub</c> claim. Read
    /// back by <c>UserHelpers.GetUserId</c>, which is how
    /// <c>PermissionHandler.IsOwner</c> identifies the author of a
    /// <c>BlogPost</c> on PUT.</param>
    /// <param name="scope">Value of the <c>scope</c> claim. The
    /// production <c>BlogScope</c> policy requires
    /// <c>RequireClaim("scope", "blogs")</c>.</param>
    /// <param name="extraClaims">Optional additional claims
    /// (e.g. a role for an admin-bypass test).</param>
    public static string Issue(
        string subject,
        string scope = "blogs",
        IEnumerable<Claim>? extraClaims = null)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new("sub", subject),
            new("scope", scope),
        };
        if (extraClaims is not null) claims.AddRange(extraClaims);

        // JwtSecurityTokenHandler ships with a static
        // DefaultInboundClaimTypeMap that rewrites short JWT claim
        // names to their long Microsoft URIs at deserialisation
        // time. The most relevant rewrite for us is
        // "sub" → ClaimTypes.NameIdentifier. Without clearing the
        // map, UserHelpers.GetUserId() — which reads the literal
        // "sub" claim — would not find the value, PermissionHandler
        // .IsOwner would compare against null, and the controller
        // would return 401 on every PUT. Clearing is the standard
        // way to opt out of the legacy mapping.
        if (!_inboundClaimTypeMapCleared)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            _inboundClaimTypeMapCleared = true;
        }

        var creds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
