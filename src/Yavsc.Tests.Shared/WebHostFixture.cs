using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Yavsc.Tests.Shared;

/// <summary>
/// Base class for ASP.NET Core integration test hosts. Provides the
/// cross-cutting plumbing shared by every test fixture in the
/// repository:
///
/// <list type="bullet">
///   <item><description>Kestrel with a self-signed HTTPS certificate
///   bound to the URL declared in configuration under
///   <c>Site:Authority</c> (port fixed by the specialisation — no
///   port collisions since all Yavsc.Org tests share a single
///   collection).</description></item>
///   <item><description>A per-process single-instance host initialised
///   on first construction and torn down when the last fixture is
///   disposed — same lazy + lock + count pattern as the original Org
///   fixture, lifted out of the specialisation.</description></item>
///   <item><description>Address list sourced from
///   <c>Site:Authority</c> so the listen URL and the OIDC
///   discovery / <c>issuer</c> URLs always match.</description></item>
/// </list>
///
/// The actual service registration, middleware pipeline and route
/// mapping are the responsibility of the subclass, through
/// <see cref="BuildApp"/>.
/// </summary>
public abstract class WebHostFixture : IDisposable
{
    private static readonly Lazy<X509Certificate2> _selfSignedCertificate =
        new Lazy<X509Certificate2>(CreateSelfSignedCertificate);
    private static readonly object _sync = new object();
    private static WebApplication? _app;
    private static bool _isInitialized;
    private static int _instanceCount;
    private static readonly List<string> _sharedAddresses = new();
    private static IServiceProvider? _sharedServices;

    /// <summary>HTTPS listen URLs the host bound to.</summary>
    public IReadOnlyList<string> Addresses { get; private set; } = Array.Empty<string>();

    /// <summary>The DI service provider of the running host. Read from
    /// the shared static slot so every fixture instance (xUnit creates
    /// one per <c>IClassFixture</c>) sees the same provider after the
    /// first initialisation. Throws if <see cref="IsInitialized"/> is
    /// false.</summary>
    public IServiceProvider Services => _sharedServices
        ?? throw new InvalidOperationException(
            "WebHostFixture has not been initialised. Call InitializeAsync first.");

    /// <summary>True once <see cref="InitializeAsync"/> has completed
    /// successfully and the host is running.</summary>
    public bool IsInitialized { get; private set; }

    protected WebHostFixture()
    {
        lock (_sync)
        {
            _instanceCount++;
            if (!_isInitialized)
            {
                InitializeAsync().GetAwaiter().GetResult();
                _isInitialized = true;
            }
            CopySharedState();
            CopySpecialisedSharedState();
        }
    }

    private void CopySharedState()
    {
        Addresses = _sharedAddresses.ToArray();
    }

    /// <summary>Hook for specialisations to copy any other shared
    /// state (test client credentials, user names, factories, etc.)
    /// from the static slots exposed by the base class onto instance
    /// properties. Called once per fixture construction, after
    /// <see cref="InitializeAsync"/> has populated the shared state
    /// the first time.</summary>
    protected virtual void CopySpecialisedSharedState() { }

    /// <summary>Specialisations register their services and middleware
    /// here. The base class has already configured Kestrel HTTPS on a
    /// dynamic port — do not bind additional listeners.</summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/>
    /// configured with Kestrel HTTPS on a dynamic port and the shared
    /// self-signed certificate.</param>
    /// <returns>The fully built <see cref="WebApplication"/>, ready
    /// for <c>ConfigurePipeline</c> + <c>StartAsync</c>.</returns>
    protected abstract WebApplication BuildApp(WebApplicationBuilder builder);

    /// <summary>Apply the production pipeline to <paramref name="app"/>.
    /// Defaults to identity + routing + auth + MapStaticAssets; override
    /// only if your host needs a different shape.</summary>
    protected virtual async Task<WebApplication> ConfigurePipelineAsync(WebApplication app)
    {
        await Task.CompletedTask;
        return app;
    }

    private async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();

        // Bind Kestrel to the URL the specialisation declared in
        // Site:Authority (the same value IdentityServer8 reads to
        // build its discovery document). Reading it from
        // configuration makes the server URL and the issuer URLs
        // refer to the same base — tests can just take
        // _sharedAddresses[0] and trust it.
        var authority = builder.Configuration["Site:Authority"]
            ?? throw new InvalidOperationException(
                "WebHostFixture: Site:Authority must be configured before InitializeAsync runs.");
        var authorityUri = new Uri(authority);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, authorityUri.Port, listenOptions =>
            {
                listenOptions.UseHttps(_selfSignedCertificate.Value);
            });
        });

        var app = BuildApp(builder);
        app = await ConfigurePipelineAsync(app);
        await app.StartAsync();

        _app = app;
        _sharedServices = app.Services;

        // Source of truth for the listen URL is the configuration
        // (Site:Authority) — not the IServerAddressesFeature, which
        // can be a different representation (e.g. 127.0.0.1 vs
        // localhost) and causes discovery / issuer mismatches when
        // tests contact the host.
        _sharedAddresses.Clear();
        _sharedAddresses.Add(authority.TrimEnd('/') + "/");
        Addresses = _sharedAddresses.ToArray();
        IsInitialized = true;
    }

    public virtual void Dispose()
    {
        lock (_sync)
        {
            _instanceCount--;
            if (_instanceCount == 0 && _app is not null)
            {
                _app.StopAsync().GetAwaiter().GetResult();
                _app = null;
                _isInitialized = false;
                _sharedAddresses.Clear();
                _sharedServices = null;
            }
        }
    }

    private static X509Certificate2 CreateSelfSignedCertificate()
    {
        var rsa = RSA.Create(2048);
        var certRequest = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        certRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

        certRequest.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

        return certRequest.CreateSelfSigned(
            new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
            new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
    }
}
