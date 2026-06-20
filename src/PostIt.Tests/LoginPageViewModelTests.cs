using System;
using System.Threading.Tasks;
using PostIt.ViewModels;
using Xunit;

namespace PostIt.Tests;

public class LoginPageViewModelTests
{
    [Fact]
    public async Task LoginAsync_acquires_access_token_from_stubbed_yavsc_authority()
    {
        // Arrange: spin up a stub OIDC authority and a fake browser that
        // short-circuits the system browser. The authority signs its
        // access_token with RS256; the fake browser captures the redirect
        // URI so the authority can complete the token exchange.
        using var authority = await OidcStubAuthority.StartAsync();
        var browser = new FakeAuthorizingBrowser(authority.LoopbackRedirectUri);

        var settings = new PostIt.Settings
        {
            Authentication = new AuthenticationSettings
            {
                Authority = authority.Issuer,
                ClientId = "postit-tests"
            },
            RedirectUri = authority.LoopbackRedirectUri,
            Scopes = new[] { "openid", "profile", "blog" }
        };

        var vm = new LoginPageViewModel(settings, browser.CreateBrowser);

        // Act
        await vm.LoginAsync();

        // Assert: the ViewModel surfaced a token, not an error.
        Assert.True(
            !string.IsNullOrEmpty(vm.AccessToken),
            $"Login did not produce a token. StatusMessage={vm.StatusMessage ?? "<null>"}");
        Assert.False(
            vm.StatusMessage?.StartsWith("Error") == true,
            $"Login reported error: {vm.StatusMessage}");
    }
}