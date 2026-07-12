using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Yavsc.Server.Helpers;

/// <summary>
/// Shared service registration helpers for Yavsc runtime services (Api, Blogs, Org, ...).
///
/// Conventions: every service reads its CORS origin allow-list from
/// <c>Site:CorsAllowedOrigins</c> as a JSON array, and its JWT Bearer authority
/// from <c>Site:Authority</c>. These helpers enforce that contract so a service
/// only needs to opt-in via a single line.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Default policy name used across all Yavsc runtime services.
    /// </summary>
    public const string DefaultCorsPolicyName = "default";

    /// <summary>
    /// Register the shared <c>"default"</c> CORS policy, sourcing the allow-list
    /// from <c>Site:CorsAllowedOrigins</c>. Fails closed (no origins registered)
    /// when the array is missing or empty.
    /// </summary>
    /// <param name="services">The service collection to add CORS to.</param>
    /// <param name="configuration">Configuration root, used to read <c>Site:CorsAllowedOrigins</c>.</param>
    /// <param name="policyName">Optional policy name override (defaults to <see cref="DefaultCorsPolicyName"/>).</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddYavscCors(
        this IServiceCollection services,
        IConfiguration configuration,
        string policyName = DefaultCorsPolicyName)
    {
        var allowedOrigins = configuration
            .GetSection("Site:CorsAllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    // Fail closed: with no origins configured, don't fall back to "*".
                    // The policy ends up effectively denying cross-origin requests,
                    // which is the safe default.
                    return;
                }
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    /// <summary>
    /// Register the standard Yavsc JWT Bearer authentication scheme, sourcing
    /// the authority from <c>Site:Authority</c>. Throws at startup if the
    /// configuration is missing — this is intentional, we'd rather fail to
    /// boot than accept tokens from an unconfigured issuer.
    /// </summary>
    /// <param name="builder">The authentication builder to extend.</param>
    /// <param name="configuration">Configuration root, used to read <c>Site:Authority</c>.</param>
    /// <param name="configure">Optional callback for service-specific options
    /// (e.g. setting <c>options.Audience</c> in <c>Yavsc.Org</c>).</param>
    /// <param name="schemeName">Optional scheme name override (defaults to <c>"Bearer"</c>).</param>
    /// <returns>The same authentication builder, for chaining.</returns>
    public static AuthenticationBuilder AddYavscJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = "Bearer")
    {
        var authority = configuration.GetSection("Site")["Authority"]
            ?? throw new InvalidOperationException(
                "Site:Authority is required to configure Yavsc JWT Bearer authentication.");


        string[] audiences = configuration.GetSection("Site").GetSection("Audience").Get<string[]>() ?? Array.Empty<string>();
        AuthenticationBuilder result = builder;
        foreach (var audience in audiences)
        {
            result = builder.AddJwtBearer(schemeName, options =>
            {
                options.IncludeErrorDetails = true;
                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = audience,
                    RoleClaimType = YavscConstants.RoleClaimType,
                    NameClaimType = YavscConstants.NameClaimType,
                };
                options.MapInboundClaims = true;
                options.ClaimsIssuer = authority;
                options.Audience = audience;

                // Dev: every Yavsc resource service (Yavsc.Api, Yavsc.Blogs,
                // Yavsc.Org itself) validates JWTs against the OP that runs
                // on https://localhost:5001 with a self-signed dev cert.
                // The default .NET HttpClient rejects self-signed certs, so
                // JwtBearer's backchannel silently fails to fetch the OIDC
                // discovery + JWKS. With an empty ValidIssuer, every token
                // is rejected with IDX10204 ("ValidIssuer is null or
                // whitespace"). Telling the backchannel to skip TLS
                // validation unblocks discovery in dev. Production uses a
                // real CA-signed cert and the default validation path; the
                // override is gated on HostingEnvironment == Development
                // and only fires when the consumer opt-in via the
                // 'Yavsc:Dev:TlsInsecure' configuration flag (default
                // false), so a misconfigured production environment cannot
                // silently downgrade TLS.
                if (configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            (_, _, _, _) => true
                    };
                }
            });
        }

        return result;

    }
}
