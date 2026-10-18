using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Abstract.Models.Messaging;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class HomePageNotificationsTests
{
    [Fact]
    public async Task Refresh_loads_public_notifications_from_the_business_host()
    {
        var transport = new NotificationTransport();
        var vm = CreateViewModel(transport);
        transport.Response = Task.FromResult(new List<Notification> { Note(1, "Information") });

        await vm.RefreshNotificationsAsync();

        Assert.Equal("https://business.example/api/v1/notifications", transport.LastPath);
        Assert.True(transport.WasAnonymous);
        Assert.Equal("Information", Assert.Single(vm.Notifications).title);
        Assert.False(vm.IsLoadingNotifications);
        Assert.False(vm.HasNoNotifications);
        Assert.Null(vm.NotificationsError);
    }

    [Fact]
    public async Task Refresh_replaces_notifications_and_distinguishes_empty_from_failed()
    {
        var transport = new NotificationTransport();
        var vm = CreateViewModel(transport);
        vm.Notifications.Add(Note(1, "Old"));
        await vm.RefreshNotificationsAsync();

        Assert.Empty(vm.Notifications);
        Assert.True(vm.HasNoNotifications);

        transport.Response = Task.FromException<List<Notification>>(new HttpRequestException("Server unavailable"));
        await vm.RefreshNotificationsAsync();

        Assert.Empty(vm.Notifications);
        Assert.False(vm.HasNoNotifications);
        Assert.Contains("Server unavailable", vm.NotificationsError);
        Assert.False(vm.IsLoadingNotifications);

        transport.Response = Task.FromResult(new List<Notification> { Note(2, "Recovered") });
        await vm.RefreshNotificationsAsync();
        Assert.Null(vm.NotificationsError);
        Assert.Equal(2, Assert.Single(vm.Notifications).Id);
    }

    [Fact]
    public async Task Refresh_ignores_an_older_response_after_a_newer_refresh()
    {
        var transport = new NotificationTransport();
        var vm = CreateViewModel(transport);
        var oldResponse = new TaskCompletionSource<List<Notification>>();
        transport.Response = oldResponse.Task;
        var oldLoad = vm.RefreshNotificationsAsync();
        Assert.True(vm.IsLoadingNotifications);

        transport.Response = Task.FromResult(new List<Notification> { Note(2, "Current") });
        await vm.RefreshNotificationsAsync();
        oldResponse.SetResult(new List<Notification> { Note(1, "Stale") });
        await oldLoad;

        Assert.Equal(2, Assert.Single(vm.Notifications).Id);
        Assert.False(vm.IsLoadingNotifications);
    }

    [AvaloniaFact]
    public async Task Login_and_logout_reload_notifications_and_discard_in_flight_private_data()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), $"postit-notifications-{Guid.NewGuid():N}.json");
        var settings = new Settings();
        var store = new TokenStore(tokenPath);
        store.Save(new RefreshTokenRecord("test-access-token", "test-refresh-token",
            DateTimeOffset.UtcNow.AddHours(1), null));
        await using var api = new YavscApiClient(settings, store);
        try
        {
            var session = new SessionStatusViewModel { Api = api };
            var transport = new NotificationTransport();
            var vm = CreateViewModel(transport, session);
            session.Refresh();
            Assert.False(transport.WasAnonymous);
            Assert.EndsWith("/notifications/me", transport.LastPath);

            var note = Note(3, "Dismiss me");
            vm.Notifications.Add(note);
            Assert.True(vm.DismissNotificationCommand.CanExecute(note));
            transport.DismissResponse = Task.FromException(new HttpRequestException("Server unavailable"));
            await vm.DismissNotificationCommand.ExecuteAsync(note);
            Assert.Contains(note, vm.Notifications);
            Assert.Contains("Server unavailable", vm.NotificationsError);

            transport.DismissResponse = Task.CompletedTask;
            await vm.DismissNotificationCommand.ExecuteAsync(note);
            Assert.Equal(HttpMethod.Post, transport.LastMethod);
            Assert.Equal("https://business.example/api/v1/dismiss/click/3", transport.LastPath);
            Assert.DoesNotContain(note, vm.Notifications);
            Assert.Null(vm.NotificationsError);

            var privateResponse = new TaskCompletionSource<List<Notification>>();
            transport.Response = privateResponse.Task;
            var privateLoad = vm.RefreshNotificationsAsync();
            Assert.False(transport.WasAnonymous);
            Assert.EndsWith("/notifications/me", transport.LastPath);

            vm.Notifications.Add(Note(9, "Private"));
            transport.Response = Task.FromResult(new List<Notification> { Note(2, "Public") });
            session.SetError("Disconnected");
            Assert.Equal(2, Assert.Single(vm.Notifications).Id);
            Assert.True(transport.WasAnonymous);

            privateResponse.SetResult(new List<Notification> { Note(1, "Late private data") });
            await privateLoad;
            Assert.Equal(2, Assert.Single(vm.Notifications).Id);
        }
        finally
        {
            File.Delete(tokenPath);
        }
    }

    [Fact]
    public async Task Anonymous_dismiss_removes_the_notification_without_an_api_call()
    {
        var transport = new NotificationTransport();
        var vm = CreateViewModel(transport);
        vm.Notifications.Add(Note(1, "Keep"));
        vm.Notifications.Add(Note(2, "Dismiss me"));

        Assert.True(vm.DismissNotificationCommand.CanExecute(vm.Notifications[1]));
        Assert.False(vm.DismissNotificationCommand.CanExecute(null));

        await vm.DismissNotificationCommand.ExecuteAsync(vm.Notifications[1]);

        Assert.Null(transport.LastMethod);
        Assert.Null(transport.LastPath);
        Assert.Equal(1, Assert.Single(vm.Notifications).Id);
        Assert.Null(vm.NotificationsError);
    }

    [Fact]
    public async Task Anonymous_dismiss_of_the_last_notification_updates_the_empty_state()
    {
        var transport = new NotificationTransport();
        var vm = CreateViewModel(transport);
        transport.Response = Task.FromResult(new List<Notification> { Note(3, "Public") });
        await vm.RefreshNotificationsAsync();
        Assert.False(vm.HasNoNotifications);

        await vm.DismissNotificationCommand.ExecuteAsync(vm.Notifications[0]);

        Assert.Empty(vm.Notifications);
        Assert.True(vm.HasNoNotifications);
        Assert.Null(vm.NotificationsError);
        Assert.Null(transport.LastMethod);
    }

    [AvaloniaFact]
    public async Task Home_page_loads_on_navigation_and_renders_wrapped_titles_and_bodies()
    {
        var transport = new NotificationTransport
        {
            Response = Task.FromResult(new List<Notification> { Note(1, "Notification title") })
        };
        var vm = CreateViewModel(transport);
        var page = new HomePage { DataContext = vm };
        var nav = new NavigationPage();
        var window = new Window { Content = nav, Width = 320, Height = 450 };
        try
        {
            window.Show();
            await nav.PushAsync(page);
            window.UpdateLayout();

            Assert.Single(vm.Notifications);
            var texts = page.GetVisualDescendants().OfType<TextBlock>().ToArray();
            Assert.Contains(texts, t => t.Text == "Notification title"
                && t.TextWrapping == Avalonia.Media.TextWrapping.Wrap);
            Assert.Contains(texts, t => t.Text == "Notification body"
                && t.TextWrapping == Avalonia.Media.TextWrapping.Wrap);
            var refresh = Assert.Single(page.GetVisualDescendants().OfType<Button>(),
                b => b.Command == vm.RefreshNotificationsCommand);
            Assert.True(refresh.IsEnabled);
            var dismiss = Assert.Single(page.GetVisualDescendants().OfType<Button>(),
                b => b.Command == vm.DismissNotificationCommand);
            Assert.Same(vm.Notifications[0], dismiss.CommandParameter);
            Assert.Equal("Close", dismiss.Content);
            Assert.True(dismiss.IsVisible);
            Assert.True(dismiss.IsEnabled);
            Assert.Contains(page.GetVisualDescendants().OfType<ScrollViewer>(),
                s => s.Extent.Height > s.Viewport.Height);

            await nav.PushAsync(new ContentPage());
            transport.Response = Task.FromResult(new List<Notification> { Note(2, "Updated title") });
            await nav.PopAsync();
            Assert.Equal(2, Assert.Single(vm.Notifications).Id);
            await vm.DismissNotificationCommand.ExecuteAsync(vm.Notifications[0]);
            window.UpdateLayout();
            Assert.Empty(vm.Notifications);
            Assert.Null(transport.LastMethod);
        }
        finally
        {
            window.Close();
        }
    }

    private static HomePageViewModel CreateViewModel(NotificationTransport transport,
        SessionStatusViewModel? session = null)
        => new(null!, new Settings(), session ?? new SessionStatusViewModel(),
            new NotificationsApiClient(transport, "https://business.example/api/v1/"));

    private static Notification Note(long id, string title)
        => new() { Id = id, title = title, body = "Notification body", click_action = "Close" };

    private sealed class NotificationTransport : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public string? LastPath { get; private set; }
        public bool WasAnonymous { get; private set; }
        public Task<List<Notification>> Response { get; set; } = Task.FromResult(new List<Notification>());

        public Task<T> GetAnonymousAsync<T>(string path, CancellationToken ct = default)
            => ReadAsync<T>(path, true);

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null,
            CancellationToken ct = default)
        {
            LastMethod = method;
            if (method == HttpMethod.Post)
            {
                LastPath = path;
                WasAnonymous = false;
                return DismissAsync<T>(path);
            }
            Assert.Equal(HttpMethod.Get, method);
            return ReadAsync<T>(path, false);
        }

        private async Task<T> DismissAsync<T>(string path)
        {
            await DismissResponse;
            return (T)(object)new DismissedNotification
            {
                NotificationId = long.Parse(path[(path.LastIndexOf('/') + 1)..])
            };
        }

        public HttpMethod? LastMethod { get; private set; }
        public Task DismissResponse { get; set; } = Task.CompletedTask;

        private async Task<T> ReadAsync<T>(string path, bool anonymous)
        {
            LastPath = path;
            WasAnonymous = anonymous;
            return (T)(object)await Response;
        }

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory,
            CancellationToken ct = default) => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, object? body = null,
            CancellationToken ct = default) => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory,
            CancellationToken ct = default) => throw new NotSupportedException();
        public ValueTask DisposeAsync()
        {
            Http.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
