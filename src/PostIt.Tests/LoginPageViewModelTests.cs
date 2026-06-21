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

    [Fact]
    public void RegisterUrl_and_ForgotPasswordUrl_are_derived_from_authority()
    {
        var settings = new PostIt.Settings
        {
            Authentication = new AuthenticationSettings
            {
                Authority = "https://yavsc.example.com/",
                ClientId = "postit-tests"
            },
            RedirectUri = "http://127.0.0.1:7890/",
            Scopes = new[] { "openid" }
        };

        var vm = new LoginPageViewModel(settings);

        // Trailing slash on Authority is normalised away.
        Assert.Equal(
            "https://yavsc.example.com/Account/Register",
            vm.RegisterUrl);
        Assert.Equal(
            "https://yavsc.example.com/Account/ForgotPassword",
            vm.ForgotPasswordUrl);
        Assert.True(vm.HasRegisterUrl);
        Assert.True(vm.HasForgotPasswordUrl);
    }

    [Fact]
    public void RegisterUrl_is_empty_when_authority_is_unset()
    {
        var vm = new LoginPageViewModel(new PostIt.Settings());
        Assert.Equal(string.Empty, vm.RegisterUrl);
        Assert.Equal(string.Empty, vm.ForgotPasswordUrl);
        Assert.False(vm.HasRegisterUrl);
        Assert.False(vm.HasForgotPasswordUrl);
    }

    [Fact]
    public void ConfigMissing_is_true_when_authority_is_unset()
    {
        var vm = new LoginPageViewModel(new PostIt.Settings());
        Assert.True(vm.ConfigMissing);
        Assert.Contains("~/.config/PostIt/postit-settings.json", vm.ConfigMissingMessage);
    }

    [Fact]
    public void ConfigMissing_is_false_when_authority_is_set()
    {
        var settings = new PostIt.Settings
        {
            Authentication = new AuthenticationSettings
            {
                Authority = "https://yavsc.example.com/",
                ClientId = "postit-tests"
            }
        };
        var vm = new LoginPageViewModel(settings);
        Assert.False(vm.ConfigMissing);
    }

    [Theory]
    [InlineData("https://yavsc.example.com/", "https://yavsc.example.com/.well-known/openid-configuration")]
    [InlineData("https://yavsc.example.com", "https://yavsc.example.com/.well-known/openid-configuration")]
    [InlineData("https://yavsc.example.com/sub/", "https://yavsc.example.com/sub/.well-known/openid-configuration")]
    public void DiscoveryUrl_is_externalurl_plus_well_known(string authority, string expected)
    {
        var settings = new PostIt.Settings
        {
            Authentication = new AuthenticationSettings { Authority = authority }
        };
        var vm = new LoginPageViewModel(settings);
        Assert.Equal(expected, vm.DiscoveryUrl);
        // ExternalUrl is the slash-normalised form of Authority.
        Assert.Equal(expected[..expected.LastIndexOf("/.well-known/openid-configuration")], vm.ExternalUrl);
    }

    [Fact]
    public void DiscoveryUrl_is_empty_when_authority_is_unset()
    {
        var vm = new LoginPageViewModel(new PostIt.Settings());
        Assert.Equal(string.Empty, vm.DiscoveryUrl);
    }

    [Fact]
    public async Task LoginAsync_failure_message_includes_discovery_url()
    {
        // Arrange: settings point at an unreachable authority; the test
        // browser throws synchronously to guarantee the catch branch runs.
        var settings = new PostIt.Settings
        {
            Authentication = new AuthenticationSettings
            {
                Authority = "https://does-not-exist.invalid/",
                ClientId = "postit-tests"
            },
            RedirectUri = "http://127.0.0.1:7890/",
            Scopes = new[] { "openid" }
        };

        var vm = new LoginPageViewModel(settings, () => throw new InvalidOperationException("boom"));

        // Act
        await vm.LoginAsync();

        // Assert: the surfaced error mentions the canonical discovery URL,
        // so it can be copy-pasted into a browser to diagnose reachability.
        Assert.NotNull(vm.StatusMessage);
        Assert.StartsWith("Error:", vm.StatusMessage);
        Assert.Contains(
            "https://does-not-exist.invalid/.well-known/openid-configuration",
            vm.StatusMessage);
    }

    [Fact]
    public async Task LoginAsync_reports_discovery_url_when_no_browser_available()
    {
        var settings = new PostIt.Settings
        {
            Authentication = new AuthenticationSettings
            {
                Authority = "https://yavsc.example.com/",
                ClientId = "postit-tests"
            },
            RedirectUri = "http://127.0.0.1:7890/",
            Scopes = new[] { "openid" }
        };

        var vm = new LoginPageViewModel(settings, () => null);

        await vm.LoginAsync();

        Assert.Contains(
            "https://yavsc.example.com/.well-known/openid-configuration",
            vm.StatusMessage);
    }
}