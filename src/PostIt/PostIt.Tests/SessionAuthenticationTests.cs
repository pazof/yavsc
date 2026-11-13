using System.Net.Http;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
[Collection(AvaloniaUiCollection.Name)]
public sealed class SessionAuthenticationTests(AvaloniaUiFixture ui)
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 600)]
    [InlineData(true, -10)]
    public Task Startup_restores_the_session_or_logs_in_without_confirmation(bool cached, int lifetime)
        => ui.RunAsync(async () =>
        {
            using var authority = await OIDCStubAuthority.StartAsync();
            var path = Path.Combine(Path.GetTempPath(), $"postit-session-{Guid.NewGuid():N}.json");
            var previousBrowser = Platform.CreateBrowser;
            var launches = 0;
            try
            {
                var store = new TokenStore(path);
                var settings = SettingsFor(authority);
                Platform.CreateBrowser = () =>
                {
                    launches++;
                    return new FakeAuthorizingBrowser(authority.LoopbackRedirectUri).CreateBrowser();
                };
                if (cached)
                {
                    await using var original = new ProfileApi(settings, store);
                    await original.LoginInteractiveAsync();
                    store.Save(store.Load()! with { AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(lifetime) });
                }
                launches = 0;
                await using var api = new ProfileApi(settings, store);
                var model = new SessionStatusViewModel { Api = api };
                await model.InitializeAsync();
                Assert.Equal(cached ? 0 : 1, launches);
                Assert.Equal(cached && lifetime < 0 ? 1 : 0, authority.RefreshRequestCount);
                Assert.True(model.IsLoggedIn);
                Assert.True(model.IsAdministrator);
                Assert.False(model.IsAuthenticationRequired);
                Assert.NotNull(store.Load());
            }
            finally
            {
                Platform.CreateBrowser = previousBrowser;
                File.Delete(path);
            }
        }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Failed_startup_login_exposes_retry_and_account_actions()
        => ui.RunAsync(async () =>
        {
            var path = Path.Combine(Path.GetTempPath(), $"postit-session-{Guid.NewGuid():N}.json");
            var previousBrowser = Platform.CreateBrowser;
            try
            {
                Platform.CreateBrowser = null;
                await using var api = new ProfileApi(new Settings(), new TokenStore(path));
                var model = new SessionStatusViewModel { Api = api };
                await model.InitializeAsync();
                Assert.True(model.IsAuthenticationRequired);
                Assert.False(model.IsLoggedIn);
                Assert.Contains("Connexion impossible", model.SessionLabel);
                Assert.True(model.LoginCommand.CanExecute(null));
                Assert.True(model.RegisterCommand.CanExecute(null));
                Assert.True(model.ForgotPasswordCommand.CanExecute(null));
            }
            finally
            {
                Platform.CreateBrowser = previousBrowser;
                File.Delete(path);
            }
        }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Transient_refresh_failure_keeps_tokens_and_successful_retry_closes_the_modal()
        => ui.RunAsync(async () =>
        {
            using var authority = await OIDCStubAuthority.StartAsync();
            var path = Path.Combine(Path.GetTempPath(), $"postit-session-{Guid.NewGuid():N}.json");
            var previousBrowser = Platform.CreateBrowser;
            try
            {
                var store = new TokenStore(path);
                var settings = SettingsFor(authority);
                Platform.CreateBrowser = () => new FakeAuthorizingBrowser(authority.LoopbackRedirectUri).CreateBrowser();
                await using (var original = new ProfileApi(settings, store))
                    await original.LoginInteractiveAsync();
                store.Save(store.Load()! with { AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(40) });
                await using var api = new ProfileApi(settings, store);
                var model = new SessionStatusViewModel { Api = api };
                model.Refresh();
                authority.RefreshError = "temporarily_unavailable";
                var error = await Assert.ThrowsAsync<RefreshFailedException>(() =>
                    api.RefreshSessionIfNeededAsync(true, TestContext.Current.CancellationToken));
                Dispatcher.UIThread.RunJobs();
                Assert.False(error.IsPermanent);
                Assert.NotNull(store.Load());
                Assert.True(model.IsAuthenticationRequired);
                authority.RefreshError = null;
                await api.RefreshSessionIfNeededAsync(true, TestContext.Current.CancellationToken);
                Dispatcher.UIThread.RunJobs();
                Assert.True(model.IsLoggedIn);
                Assert.True(model.IsAdministrator);
                Assert.False(model.IsAuthenticationRequired);
                Assert.Equal(2, authority.RefreshRequestCount);
            }
            finally
            {
                Platform.CreateBrowser = previousBrowser;
                File.Delete(path);
            }
        }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Expired_session_without_refresh_token_requires_authentication()
        => ui.RunAsync(async () =>
        {
            var path = Path.Combine(Path.GetTempPath(), $"postit-session-{Guid.NewGuid():N}.json");
            try
            {
                var store = new TokenStore(path);
                store.Save(new RefreshTokenRecord("access", "", DateTimeOffset.UtcNow.AddSeconds(-1)));
                await using var api = new ProfileApi(new Settings(), store);
                var model = new SessionStatusViewModel { Api = api };
                Assert.Equal(TimeSpan.Zero, api.GetSessionRefreshDelay(DateTimeOffset.UtcNow, true));
                await Assert.ThrowsAsync<RefreshFailedException>(() =>
                    api.RefreshSessionIfNeededAsync(true, TestContext.Current.CancellationToken));
                Dispatcher.UIThread.RunJobs();
                Assert.True(model.IsAuthenticationRequired);
                Assert.False(api.HasValidSession);
                Assert.Null(store.Load());
            }
            finally { File.Delete(path); }
        }, TestContext.Current.CancellationToken);

    [Fact]
    public async Task Modal_restores_the_previous_page_and_focus_after_silent_recovery()
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-session-{Guid.NewGuid():N}.json");
        try
        {
            var store = new TokenStore(path);
            store.Save(new RefreshTokenRecord("access", "refresh", DateTimeOffset.UtcNow.AddMinutes(10)));
            await using var api = new ProfileApi(new Settings(), store);
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            await ui.WithWindowAsync(() => new MainView
            {
                DataContext = new HomePageViewModel(api, api.Settings, session, null!)
            }, async window =>
            {
                var root = Assert.IsType<MainView>(window.Content);
                var editor = new TextBox { Text = "Brouillon conservé" };
                var page = new ContentPage { Content = editor };
                await root.NavRoot.PushAsync(page, null);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Assert.True(editor.Focus());
                session.SetError("Session temporairement indisponible");
                Dispatcher.UIThread.RunJobs();
                Assert.True(root.LoginButton.IsFocused);
                session.Refresh();
                Dispatcher.UIThread.RunJobs();
                Assert.False(root.AuthenticationModal.IsVisible);
                Assert.True(root.NavRoot.IsEnabled);
                Assert.True(editor.IsFocused);
                Assert.Same(page, root.NavRoot.NavigationStack.Last());
                Assert.Equal("Brouillon conservé", editor.Text);
            }, TestContext.Current.CancellationToken);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public Task Revoked_background_session_opens_the_modal_and_login_recovers()
        => ui.RunAsync(async () =>
        {
            using var authority = await OIDCStubAuthority.StartAsync();
            var path = Path.Combine(Path.GetTempPath(), $"postit-session-{Guid.NewGuid():N}.json");
            var previousBrowser = Platform.CreateBrowser;
            try
            {
                var settings = SettingsFor(authority);
                var store = new TokenStore(path);
                store.Save(new RefreshTokenRecord("access", "revoked", DateTimeOffset.UtcNow.AddSeconds(40)));
                await using var api = new ProfileApi(settings, store);
                var model = new SessionStatusViewModel { Api = api };
                model.Refresh();
                Assert.False(model.IsAuthenticationRequired);
                await Assert.ThrowsAsync<RefreshFailedException>(() =>
                    api.RefreshSessionIfNeededAsync(true, TestContext.Current.CancellationToken));
                Dispatcher.UIThread.RunJobs();
                Assert.False(model.IsLoggedIn);
                Assert.False(model.IsAdministrator);
                Assert.True(model.IsAuthenticationRequired);
                Assert.Contains("Session expirée", model.SessionLabel);
                Platform.CreateBrowser = () => new FakeAuthorizingBrowser(authority.LoopbackRedirectUri).CreateBrowser();
                await model.LoginCommand.ExecuteAsync(null);
                Assert.True(model.IsLoggedIn);
                Assert.False(model.IsAuthenticationRequired);
                await model.LogoutAsync();
                Assert.True(model.IsAuthenticationRequired);
                Assert.Null(store.Load());
            }
            finally
            {
                Platform.CreateBrowser = previousBrowser;
                File.Delete(path);
            }
        }, TestContext.Current.CancellationToken);

    [Theory]
    [InlineData(320, 480)]
    [InlineData(640, 240)]
    public Task Modal_blocks_navigation_and_scrolls_on_small_screens_without_a_status_bar(int width, int height)
    {
        var session = new SessionStatusViewModel();
        session.SetError("Session expirée");
        var model = new HomePageViewModel(null!, new Settings(), session, null!);
        return ui.WithWindowAsync(() => new MainView { DataContext = model }, async window =>
        {
            var root = Assert.IsType<MainView>(window.Content);
            Assert.True(root.AuthenticationModal.IsVisible);
            Assert.False(root.NavRoot.IsEnabled);
            Assert.True(root.LoginButton.IsFocused);
            Assert.Equal(KeyboardNavigationMode.Cycle, KeyboardNavigation.GetTabNavigation(root.AuthenticationModal));
            var buttons = new[] { root.LoginButton, root.RegisterButton, root.ForgotPasswordButton };
            foreach (var button in buttons)
            {
                Assert.True(button.IsEffectivelyEnabled);
                button.BringIntoView();
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                var top = button.TranslatePoint(new Point(), root)!.Value.Y;
                Assert.InRange(top, 0, root.Bounds.Height - button.Bounds.Height);
                Assert.InRange(button.Bounds.Width, 1, width - 24);
            }
            Assert.DoesNotContain(root.GetVisualDescendants(),
                control => control.GetType().Name == "SessionStatusBanner");
            Assert.Equal(root.Bounds.Height, root.NavRoot.Bounds.Height);
            await Task.CompletedTask;
        }, TestContext.Current.CancellationToken, width, height);
    }

    [Theory]
    [InlineData("https://oidc.example", "https://oidc.example/Account/")]
    [InlineData("https://oidc.example/tenant/", "https://oidc.example/tenant/Account/")]
    public async Task Account_actions_target_the_configured_authority(string authority, string expectedBase)
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-session-{Guid.NewGuid():N}.json");
        await using var api = new ProfileApi(new Settings
        {
            Authentication = new AuthenticationSettings { Authority = authority }
        }, new TokenStore(path));
        var model = new SessionStatusViewModel { Api = api };
        Assert.Equal(expectedBase + "Register", model.RegistrationUri.AbsoluteUri);
        Assert.Equal(expectedBase + "ForgotPassword", model.ForgotPasswordUri.AbsoluteUri);
        api.Settings.Authentication.Authority = "https://new.example";
        Assert.Equal("https://new.example/Account/ForgotPassword", model.ForgotPasswordUri.AbsoluteUri);
    }

    private static Settings SettingsFor(OIDCStubAuthority authority) => new()
    {
        Authentication = new AuthenticationSettings
        {
            Authority = authority.Issuer,
            ClientId = "postit-tests",
            RedirectUri = authority.LoopbackRedirectUri,
            Scopes = ["openid", "profile", "blog"]
        }
    };

    private sealed class ProfileApi(Settings settings, TokenStore store) : YavscApiClient(settings, store)
    {
        public override Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null,
            CancellationToken ct = default)
            => Task.FromResult(JsonSerializer.Deserialize<T>("""{"Roles":["Administrator"]}""")!);
    }
}
