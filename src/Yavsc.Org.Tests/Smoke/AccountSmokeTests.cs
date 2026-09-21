using System.Net;
using System.Net.Http;
using IdentityServer8.Stores;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Tests.Shared;
using Xunit;

namespace Yavsc.Org.Tests.Smoke;

/// <summary>
/// Smoke for the Account BC (login / registration / OIDC discovery).
/// Hits the in-memory test server through
/// <see cref="TestWebApplicationFactory"/>: no sockets, no
/// self-signed certificates, no real DB (the
/// <c>AddInMemoryCollection</c> in <c>WebServerFixture.SetupHost</c>
/// points the EF context at the in-memory provider).
///
/// <c>GET /signin</c> is the public login endpoint served by
/// <c>Yavsc.Org/Controllers/Accounting/AccountController</c>
/// (decorated <c>[HttpGet(YavscConstants.SigninPath)]</c> with
/// <c>YavscConstants.SigninPath = "~/signin"</c>). A 200 means the
/// entire pipeline (routing + Razor + IdentityServer + EF + DI)
/// is wired correctly end-to-end.
/// </summary>
public class AccountSmokeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AccountSmokeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }



    [Fact]
    public async Task ResourceStore_get_all_resources_does_not_throw()
    {
        using var scope = _factory.Services.CreateScope();
        var resourceStore = scope.ServiceProvider.GetRequiredService<IResourceStore>();

        var exception = await Record.ExceptionAsync(resourceStore.GetAllResourcesAsync);

        Assert.Null(exception);
    }

    // Regression: anonymous access to a restricted page used to redirect to
    // the ASP.NET Identity default LoginPath "/Account/Login", which has no
    // controller here. The application cookie must redirect to the real
    // sign-in endpoint at /signin instead.
    [Fact]
    public async Task Anonymous_access_to_protected_page_redirects_to_signin()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        // EstimateController is [Authorize]; GET /Estimate hits Index().
        var response = await client.GetAsync("/Estimate", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        Assert.Equal("/signin", location.AbsolutePath);
        Assert.Contains("ReturnUrl=%2FEstimate", location.Query);
    }

    // Regression: the POST Delete action existed, but neither the GET
    // Delete view nor a rendered confirmation page was reachable —
    // Views/Account/Delete.cshtml was missing. An authenticated GET
    // must now return the confirmation page (200).
    [Fact]
    public async Task Authenticated_get_account_delete_returns_confirmation_page()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        client.DefaultRequestHeaders.Add(
            TestAuthPolicyProvider.HeaderName, TestAuthPolicyProvider.AdminRole);

        var response = await client.GetAsync("/Account/Delete", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);
        Assert.Contains("Supprimer mon compte", body);
    }
}
