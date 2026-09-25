using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityModel.Client;
using Xunit;
using Yavsc.Server.Helpers;

namespace Yavsc.Org.Tests
{
    [Collection("Yavsc Server")]
    [Trait("regression", "oui")]
    public class Remoting : BaseTestContext, IClassFixture<WebServerFixture>
    {
        private readonly ITestOutputHelper _output;

        public Remoting(WebServerFixture serverFixture, ITestOutputHelper output)
        : base(output, serverFixture)
        {
            _output = output;
        }


        [Fact]
        public async Task ObtainServiceToken()
        {
            var serverUrl = GetServerUrl();
            var cancellationToken = TestContext.Current.CancellationToken;

            HttpClient client = NewHttpClient();
            var tokenEndpoint = await ResolveTokenEndpointAsync(client, serverUrl, cancellationToken);

            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = RequireNonEmpty(_serverFixture.TestClientId, nameof(_serverFixture.TestClientId)),
                ClientSecret = RequireNonEmpty(_serverFixture.TestClientSecret, nameof(_serverFixture.TestClientSecret)),
                Scope = "test",
                GrantType = "client_credentials"
            }, cancellationToken);
            if (response.IsError) throw new Exception(response.Error);
        }

        private static HttpClient NewHttpClient()
        {
            return new HttpClient(new BypassSslValidationHandler());
        }

        [Fact]
        public async Task ObtainResourceOwnerPasswordToken()
        {
            var serverUrl = GetServerUrl();
            var cancellationToken = TestContext.Current.CancellationToken;

            var client = NewHttpClient();
            var tokenEndpoint = await ResolveTokenEndpointAsync(client, serverUrl, cancellationToken);

            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = RequireNonEmpty(_serverFixture.TestClientId, nameof(_serverFixture.TestClientId)),
                ClientSecret = RequireNonEmpty(_serverFixture.TestClientSecret, nameof(_serverFixture.TestClientSecret)),
                UserName = RequireNonEmpty(_serverFixture.TestingUserName, nameof(_serverFixture.TestingUserName)),
                Password = RequireNonEmpty(_serverFixture.TestingUserPassword, nameof(_serverFixture.TestingUserPassword)),
                Scope = "test",
                Parameters =
                {
                    { "acr_values", "tenant:custom_account_store1 foo bar quux" }
                }
            }, cancellationToken);

            if (response.IsError) throw new Exception(response.Error);

        }

        /// <summary>
        /// Regression for the MyFiles upload 500 / null
        /// <c>User.Identity.Name</c>: the access token minted by the
        /// real IdentityServer (resource-owner password grant) must
        /// carry a <c>name</c> claim whose value is the user's login
        /// (<c>UserName</c>), and <see cref="UserHelpers.GetUserName"/>
        /// — which reads <c>FindFirstValue("name")</c> — must resolve
        /// to it on a principal built the way
        /// <c>AddYavscJwtBearer</c> builds one
        /// (<c>MapInboundClaims=false</c>, <c>NameClaimType="name"</c>).
        ///
        /// <para>Prerequisites wired by <see cref="WebServerFixture"/>:
        /// the test client's <c>AllowedScopes</c> include
        /// <c>openid</c>+<c>profile</c>, and the standard identity
        /// resources are seeded by the production pipeline. Requesting
        /// <c>openid profile</c> makes <c>ProfileService</c> emit
        /// <c>name</c> = <c>user.UserName</c>.</para>
        /// </summary>
        [Fact]
        public async Task PasswordToken_carries_name_claim_equal_to_login()
        {
            var serverUrl = GetServerUrl();
            var cancellationToken = TestContext.Current.CancellationToken;

            var client = NewHttpClient();
            var tokenEndpoint = await ResolveTokenEndpointAsync(client, serverUrl, cancellationToken);

            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = RequireNonEmpty(_serverFixture.TestClientId, nameof(_serverFixture.TestClientId)),
                ClientSecret = RequireNonEmpty(_serverFixture.TestClientSecret, nameof(_serverFixture.TestClientSecret)),
                UserName = RequireNonEmpty(_serverFixture.TestingUserName, nameof(_serverFixture.TestingUserName)),
                Password = RequireNonEmpty(_serverFixture.TestingUserPassword, nameof(_serverFixture.TestingUserPassword)),
                // "openid profile" triggers ProfileService to emit the
                // "name" claim (user.UserName). Without profile, the
                // token carries only sub + scopes and GetUserName()
                // resolves to null — the exact regression behind the
                // MyFiles upload 500. "test" is the API scope, so a real
                // JWT access token (not just an id_token) is issued.
                Scope = "openid profile test"
            }, cancellationToken);

            Assert.False(response.IsError,
                $"Token request failed: {response.Error} ({response.ErrorDescription})");

            // Decode the access token straight from the payload so short
            // claim names ("name", "sub") stay as-emitted — mirrors
            // AddYavscJwtBearer with MapInboundClaims=false.
            var jwt = new JwtSecurityToken(response.AccessToken);
            var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == "name");
            Assert.True(nameClaim is not null,
                "access_token has no 'name' claim; ProfileService did not emit it " +
                "(is the 'profile' scope in the client's AllowedScopes and requested?).");
            Assert.Equal(_serverFixture.TestingUserName, nameClaim!.Value);

            // Rebuild the principal the way a bearer consumer host does
            // and exercise the real GetUserName() helper used by the
            // file-system / avatar paths.
            var identity = new ClaimsIdentity(
                jwt.Claims, authenticationType: "Bearer",
                nameType: "name", roleType: null);
            var principal = new ClaimsPrincipal(identity);
            Assert.Equal(_serverFixture.TestingUserName, principal.GetUserName());
        }

 [Fact]
    public async Task GetSignin_returns_a_page()
    {
        using var client = new HttpClient(new BypassSslValidationHandler())
        {
            BaseAddress = new Uri(this._serverFixture.HttpsAuthority ?? throw new InvalidOperationException("Missing HttpsAuthority"))
        };
        await AssertResponds(client, "/signin");
    }



    [Fact]
    public async Task GetOpenIdConfiguration_returns_ok()
    {
        using var client = CreateHttpClient();
        var response = await GetRaw(client, "/.well-known/openid-configuration");
        var payload = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );

        Assert.True(
            response.IsSuccessStatusCode,
            $"GET /.well-known/openid-configuration returned {(int)response.StatusCode} {response.StatusCode}. Body: {payload}");
    }
        public static IEnumerable<object[]> GetLoginIntentData()
        {
            return new object[][] { new object[] { "testuser", "test" } };
        }

        private async Task<string> ResolveTokenEndpointAsync(HttpClient client, string serverUrl, CancellationToken cancellationToken)
        {
            var disco = await client.GetDiscoveryDocumentAsync(serverUrl, cancellationToken);
            if (!disco.IsError && !string.IsNullOrWhiteSpace(disco.TokenEndpoint))
            {
                return disco.TokenEndpoint;
            }

            // Some full-suite runs intermittently return 500 on the OIDC
            // discovery document while /connect/token remains available.
            var fallback = new Uri(new Uri(serverUrl), "/connect/token").ToString();
            _output.WriteLine($"WARNING: OIDC discovery failed ({disco.Error}). Fallback token endpoint: {fallback}");
            return fallback;
        }

        private string GetServerUrl()
        {
            return RequireNonEmpty(_serverFixture.SiteSettings?.Authority, "SiteSettings.Authority");
        }

        private static string RequireNonEmpty(string? value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing required test setting: {name}");
            }

            return value;
        }

    }

    internal class BypassSslValidationHandler : HttpClientHandler
    {
        public BypassSslValidationHandler()
        {
            // Override validation for this handler only
            ServerCertificateCustomValidationCallback = ValidateCertificate;
        }

        private bool ValidateCertificate(
            HttpRequestMessage request,
            X509Certificate2? certificate,
            X509Chain? chain,
            SslPolicyErrors errors)
        {
            // Accept all certificates (bypass validation)
            return true;
        }
    }

}
