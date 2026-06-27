using System.Threading.Tasks;
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
public class AccountSmokeTests : SmokeTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AccountSmokeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSignin_returns_a_page()
    {
        using var client = _factory.CreateClient();
        await AssertResponds(client, "/signin");
    }
}
