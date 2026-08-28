using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Yavsc.Blogs.Controllers;
using Yavsc.Models;
using Yavsc.Services;
using Yavsc.Tests.Shared;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Dedicated integration-test host that mirrors the production JWT
/// remapping behavior: MapInboundClaims remains enabled and the
/// default inbound map rewrites "sub" to ClaimTypes.NameIdentifier.
/// This is the closest in-process reproduction of the production
/// authentication surface for the blog API.
/// </summary>
public sealed class MappedClaimsBlogsWebServerFixture : IDisposable, IBackendFixture
{
    private readonly InMemoryDatabaseRoot _inMemoryRoot = new();
    private readonly Dictionary<string, string> _savedInboundMap;
    private readonly WebApplication _app;

    public MappedClaimsBlogsWebServerFixture()
    {
        _savedInboundMap = new Dictionary<string, string>(JwtSecurityTokenHandler.DefaultInboundClaimTypeMap);
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["sub"] = ClaimTypes.NameIdentifier;

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:5104");

        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseInMemoryDatabase("Yavsc.Blogs.Tests.MappedClaims", _inMemoryRoot));

        builder.Services.AddSingleton<IFileSystemAuthManager>(new NoopFileSystemAuthManager());
        builder.Services.AddScoped<BlogSpotService>();
        builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(BlogApiController).Assembly);
        builder.Services.AddAuthorization(opt =>
        {
            opt.AddPolicy("BlogScope", policy =>
            {
                policy.RequireAuthenticatedUser()
                      .RequireClaim("scope", "blogs");
            });
        });
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.IncludeErrorDetails = true;
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = TestTokenIssuer.Issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TestTokenIssuer.SigningKey,
                    RoleClaimType = Yavsc.Constants.RoleClaimType,
                    NameClaimType = Yavsc.Constants.NameClaimType,
                };
            });

        _app = builder.Build();
        _app.UseRouting();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();
        _app.StartAsync().GetAwaiter().GetResult();

        Addresses = ["http://127.0.0.1:5104"];
        Services = _app.Services;
    }

    public IReadOnlyList<string> Addresses { get; }

    public IServiceProvider Services { get; }

    public void Dispose()
    {
        _app.StopAsync().GetAwaiter().GetResult();
        _app.DisposeAsync().AsTask().GetAwaiter().GetResult();

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        foreach (var kvp in _savedInboundMap)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[kvp.Key] = kvp.Value;
        }
    }

    private sealed class NoopFileSystemAuthManager : IFileSystemAuthManager
    {
        public FileAccessRight GetFilePathAccess(System.Security.Claims.ClaimsPrincipal user, string fileRelativePath)
            => FileAccessRight.None;

        public void SetAccess(long circleId, string normalizedFullPath, FileAccessRight access)
        {
        }
    }
}
