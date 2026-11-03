using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Security.Cryptography;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.ViewModels.Chat;
using PostIt.Views;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public sealed class MessagingSessionTests
{
#if DEBUG
    [Fact]
    public async Task Native_start_generations_reject_stale_requests_after_stop_and_timeout()
        => await PostIt.Android.Tests.ChatServiceEpochTests.VerifyAsync();
#endif

    [AvaloniaFact]
    public void Settings_page_binds_opt_in_and_hides_it_on_other_platforms()
    {
        var settings = new Settings();
        var page = new SettingsPage { DataContext = settings };
        var window = new Window { Content = page };
        try
        {
            window.Show();
            var checkbox = page.FindControl<CheckBox>("BackgroundChatCheckBox")!;
            var container = checkbox.GetVisualAncestors().OfType<StackPanel>().First();
            Assert.Equal(OperatingSystem.IsAndroid(), container.IsVisible);
            var explanation = Assert.Single(container.Children.OfType<TextBlock>()).Text!;
            Assert.Contains("sans notification permanente", explanation);
            Assert.Contains("ne garantissent pas", explanation);
            Assert.False(checkbox.IsChecked);
            checkbox.IsChecked = true;
            Assert.True(settings.AndroidKeepChatConnected);
            Assert.True(settings.IsDirty);
            settings.AndroidKeepChatConnected = false;
            Assert.False(checkbox.IsChecked);
        }
        finally { window.Close(); }
    }

    [Fact]
    public void Background_chat_service_are_opt_in_and_saved_only_after_persistence()
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-chat-{Guid.NewGuid():N}.json");
        try
        {
            var settings = new Settings(path);
            Assert.False(settings.AndroidKeepChatConnected);
            var saves = 0;
            settings.Saved += (_, _) =>
            {
                Assert.True(File.Exists(path));
                Assert.False(settings.IsDirty);
                saves++;
            };
            settings.AndroidKeepChatConnected = true;
            Assert.True(settings.IsDirty);
            settings.Save();
            var restored = new Settings(path);
            restored.Authentication.Authority = "";
            restored.Load();
            Assert.True(restored.AndroidKeepChatConnected);
            Assert.False(restored.IsDirty);
            Assert.Equal(1, saves);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task Logout_exposes_credentials_for_service_shutdown_but_clears_them_even_if_cleanup_fails()
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-chat-{Guid.NewGuid():N}");
        try
        {
            var store = CreateStore(path);
            await using var api = new YavscApiClient(new Settings(), store);
            api.SigningOut += () =>
            {
                Assert.True(api.IsSigningOut);
                Assert.True(api.HasValidSession);
                Assert.NotNull(api.CurrentAccessToken);
                throw new HttpRequestException("offline");
            };
            await Assert.ThrowsAsync<HttpRequestException>(() => api.LogoutAsync());
            Assert.False(api.HasValidSession);
            Assert.False(api.IsSigningOut);
            Assert.Null(api.CurrentAccessToken);
            Assert.Null(store.Load());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task Starting_a_connection_during_logout_is_rejected_even_while_credentials_exist()
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-chat-signout-{Guid.NewGuid():N}");
        try
        {
            var settings = new Settings();
            await using var api = new YavscApiClient(settings, CreateStore(path));
            await using var transport = new ChatHubConnection(api);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            api.SigningOut += () => release.Task;
            var logout = api.LogoutAsync();
            try
            {
                Assert.True(api.IsSigningOut);
                Assert.True(api.HasValidSession);
                await Assert.ThrowsAsync<InvalidOperationException>(() => transport.StartAsync());
                var vm = new ChatViewModel(api, settings, transport);
                await vm.ConnectAsync();
                Assert.False(vm.ConnectionRequested);
                Assert.False(vm.IsConnected);
            }
            finally
            {
                release.TrySetResult();
                await logout;
            }
            Assert.False(api.HasValidSession);
            Assert.False(api.IsSigningOut);
        }
        finally { File.Delete(path); }
    }

    [AvaloniaFact]
    public async Task Disconnect_cancels_an_in_flight_chat_handshake_instead_of_waiting_for_network_timeout()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        var handshake = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var server = builder.Build();
        server.MapPost("/" + Yavsc.Constants.ChatHubPath + "/negotiate", async (Microsoft.AspNetCore.Http.HttpContext context) =>
        {
            handshake.TrySetResult();
            await Task.Delay(Timeout.Infinite, context.RequestAborted);
        });
        await server.StartAsync();
        var path = Path.Combine(Path.GetTempPath(), $"postit-chat-cancel-{Guid.NewGuid():N}");
        try
        {
            var settings = new Settings { ApiUrl = server.Urls.Single() + "/api/v1/" };
            settings.Authentication.Authority = "https://login.example.test";
            await using var api = new YavscApiClient(settings, CreateStore(path));
            await using var transport = new ChatHubConnection(api);
            var vm = new ChatViewModel(api, settings, transport);
            var connect = vm.ConnectAsync();
            await handshake.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var disconnect = vm.DisconnectAsync();
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (!disconnect.IsCompleted && DateTime.UtcNow < deadline)
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10, TestContext.Current.CancellationToken);
            }
            Assert.True(disconnect.IsCompleted, $"Transport={transport.Connection.State}, chat={vm.Status.Message}");
            await disconnect;
            await connect;
            Assert.False(vm.ConnectionRequested);
            Assert.False(vm.IsConnected);
        }
        finally
        {
            File.Delete(path);
            await server.StopAsync();
        }
    }

    [Theory]
    [InlineData("alice", "https://api.example.test/api/v1/", true)]
    [InlineData("bob", "https://api.example.test/api/v1/", false)]
    [InlineData("alice", "https://other.example.test/api/v1/", false)]
    public async Task Remembered_rooms_are_isolated_by_account_and_server(string user, string apiUrl, bool restored)
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-chat-identity-{Guid.NewGuid():N}");
        try
        {
            const string authority = "https://login.example.test";
            var settings = new Settings { ApiUrl = apiUrl };
            settings.Authentication.Authority = authority;
            var aliceKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                authority + "\nhttps://api.example.test/api/v1/\nalice")));
            settings.ChatRoomsByAccount[aliceKey] = ["PrivateRoom"];
            var jwt = "e30." + Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"sub\":\"" + user + "\"}"))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_') + ".signature";
            var store = new TokenStore(path);
            store.Save(new RefreshTokenRecord(jwt, "refresh", DateTimeOffset.UtcNow.AddHours(1), jwt));
            await using var api = new YavscApiClient(settings, store);
            await using var transport = new ChatHubConnection(api);
            Assert.Equal(restored ? 1 : 0, transport.JoinedRooms.Length);
        }
        finally { File.Delete(path); }
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Session_uses_one_chat_connection_and_logout_disables_service_before_token_purge(bool connectOnLogin)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<ConnectionCounter>();
        await using var server = builder.Build();
        server.MapHub<TestChatHub>("/" + Yavsc.Constants.ChatHubPath);
        await server.StartAsync();
        var path = Path.Combine(Path.GetTempPath(), $"postit-chat-{Guid.NewGuid():N}");
        var previous = Platform.BackgroundChatService;
        var native = new RecordingChatService();
        Platform.BackgroundChatService = native;
        try
        {
            var settings = new Settings(path + ".settings")
            {
                ApiUrl = server.Urls.Single() + "/api/v1/",
                AndroidKeepChatConnected = true
            };
            settings.Authentication.Authority = "https://login.example.test";
            await using var api = new YavscApiClient(settings, CreateStore(path));
            await using var transport = new ChatHubConnection(api);
            var chat = new ChatViewModel(api, settings, transport);
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            await using var messaging = new MessagingSession(api, session, chat);
            await messaging.StartAsync(connectOnLogin);
            if (!connectOnLogin)
            {
                Assert.False(chat.IsConnected);
                await chat.ConnectAsync();
            }
            Assert.True(chat.IsConnected, chat.Status.Message);
            await Task.WhenAll(chat.ConnectAsync(), chat.ConnectAsync(), messaging.StartAsync());
            Assert.Equal(1, await transport.Connection.InvokeAsync<int>("ConnectionCount"));
            Assert.Equal(connectOnLogin ? new[] { true } : new[] { false, true }, native.Preferences);
            chat.CurrentRoom = "first";
            await chat.JoinRoomCommand();
            chat.CurrentRoom = "second";
            await chat.JoinRoomCommand();
            Assert.Equal(new[] { "first", "second" }, chat.JoinedRooms);
            var counter = server.Services.GetRequiredService<ConnectionCounter>();
            counter.Context!.Abort();
            var deadline = DateTime.UtcNow.AddSeconds(40);
            while (!counter.RoomsRejoined.Task.IsCompleted && DateTime.UtcNow < deadline)
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(20, TestContext.Current.CancellationToken);
            }
            Assert.True(counter.RoomsRejoined.Task.IsCompleted,
                $"Connections={counter.Count}, transport={transport.Connection.State}, chat={chat.Status.Message}");
            Assert.Equal(2, await transport.Connection.InvokeAsync<int>("ConnectionCount"));
            Assert.Equal(new[] { "first", "second" }, counter.RejoinedRooms);

            Platform.SuspendPlayback();
            Assert.True(chat.IsConnected);
            await chat.DisconnectAsync();
            settings.Save();
            Assert.False(chat.ConnectionRequested);
            Assert.False(native.Preferences.Last());
            await chat.ConnectAsync();
            Assert.True(native.Preferences.Last());
            settings.AndroidKeepChatConnected = false;
            settings.Save();
            await native.DisabledByPreference.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(native.Preferences.Last());
            native.BlockNextStart = true;
            settings.AndroidKeepChatConnected = true;
            settings.Save();
            await native.ServiceStartRequested.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await session.LogoutAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(chat.IsConnected);
            Assert.False(chat.ConnectionRequested);
            Assert.False(api.HasValidSession);
            Assert.True(native.DisabledWithValidToken);
            Assert.True(native.ServiceStartCancelled);
        }
        finally
        {
            Platform.BackgroundChatService = previous;
            File.Delete(path);
            File.Delete(path + ".settings");
            await server.StopAsync();
        }
    }

    [AvaloniaFact]
    public async Task Headless_transport_restores_account_rooms_and_the_later_ui_reuses_its_socket()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<ConnectionCounter>();
        await using var server = builder.Build();
        server.MapHub<TestChatHub>("/" + Yavsc.Constants.ChatHubPath);
        await server.StartAsync();
        var path = Path.Combine(Path.GetTempPath(), $"postit-chat-rooms-{Guid.NewGuid():N}");
        try
        {
            var settings = new Settings(path + ".settings") { ApiUrl = server.Urls.Single() + "/api/v1/" };
            settings.Authentication.Authority = "https://login.example.test";
            var store = CreateStore(path);
            var jwt = "e30." + Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"sub\":\"alice\"}")).TrimEnd('=')
                .Replace('+', '-').Replace('/', '_') + ".signature";
            store.Save(new RefreshTokenRecord(jwt, "refresh", DateTimeOffset.UtcNow.AddHours(1), jwt));
            await using (var api = new YavscApiClient(settings, store))
            await using (var transport = new ChatHubConnection(api))
            {
                await transport.StartAsync(TestContext.Current.CancellationToken);
                await transport.JoinAsync("SavedRoom", TestContext.Current.CancellationToken);
                Assert.Single(settings.ChatRoomsByAccount);
            }
            var restored = new Settings(path + ".settings");
            restored.Authentication.Authority = "";
            restored.Load();
            await using var resumedApi = new YavscApiClient(restored, store);
            await using var resumed = new ChatHubConnection(resumedApi);
            await resumed.StartAsync(TestContext.Current.CancellationToken);
            Assert.Equal(2, await resumed.Connection.InvokeAsync<int>("ConnectionCount"));
            Assert.Equal("SavedRoom", Assert.Single(resumed.JoinedRooms).Name);
            Assert.Equal(new[] { "SavedRoom" }, server.Services.GetRequiredService<ConnectionCounter>().RejoinedRooms);
            var firstMessages = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var allMessages = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            resumed.MessageReceived += delivery =>
            {
                if (delivery.Sequence == 2) firstMessages.TrySetResult();
                if (delivery.Sequence == 504) allMessages.TrySetResult();
            };
            await resumed.Connection.InvokeAsync("EmitMessages", 1);
            await firstMessages.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(2, resumed.RecentMessages.Length);

            var vm = new ChatViewModel(resumedApi, restored, resumed);
            await vm.ConnectAsync();
            Assert.Equal(2, await resumed.Connection.InvokeAsync<int>("ConnectionCount"));
            Assert.Equal(new[] { "SavedRoom" }, vm.JoinedRooms);
            Assert.Contains("alice : room-0", vm.FocusedMessages);
            vm.SelectedRoom = null;
            Assert.Contains("[Prive] bob : private-0", vm.FocusedMessages);
            vm.SelectedRoom = "SavedRoom";
            await resumed.Connection.InvokeAsync("EmitMessages", 251);
            await allMessages.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(500, resumed.RecentMessages.Length);
            await vm.LeaveRoom();
            Assert.Empty(resumed.JoinedRooms);
            Assert.Empty(vm.JoinedRooms);
            Assert.Empty(Assert.Single(restored.ChatRoomsByAccount).Value);
            await vm.DisconnectAsync();
            Assert.Empty(resumed.RecentMessages);
            Assert.DoesNotContain(vm.FocusedMessages, message => message.Contains("private-0"));
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".settings");
            await server.StopAsync();
        }
    }

    private static TokenStore CreateStore(string path)
    {
        var store = new TokenStore(path);
        store.Save(new RefreshTokenRecord("access", "refresh", DateTimeOffset.UtcNow.AddHours(1), null));
        return store;
    }

    public sealed class ConnectionCounter
    {
        public int Count;
        public HubCallerContext? Context;
        public List<string> RejoinedRooms { get; } = [];
        public TaskCompletionSource RoomsRejoined { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public sealed class TestChatHub(ConnectionCounter counter) : Hub
    {
        public int ConnectionCount() => counter.Count;
        public ChatRoomInfo Join(string room)
        {
            if (counter.Count > 1)
            {
                counter.RejoinedRooms.Add(room);
                if (counter.RejoinedRooms.Count == 2) counter.RoomsRejoined.TrySetResult();
            }
            return new ChatRoomInfo { Name = room };
        }
        public Task Part(string room, string reason) => Task.CompletedTask;
        public async Task EmitMessages(int count)
        {
            for (var index = 0; index < count; index++)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", new ChatMessage
                {
                    Room = "SavedRoom", Name = "alice", Message = $"room-{index}"
                });
                await Clients.Caller.SendAsync("addPV", "bob", $"private-{index}");
            }
        }
        public override Task OnConnectedAsync()
        {
            counter.Context = Context;
            Interlocked.Increment(ref counter.Count);
            return base.OnConnectedAsync();
        }
    }

    private sealed class RecordingChatService : IBackgroundChatService
    {
        public List<bool> Preferences { get; } = [];
        public bool DisabledWithValidToken { get; private set; }
        public bool BlockNextStart { get; set; }
        public bool ServiceStartCancelled { get; private set; }
        public TaskCompletionSource DisabledByPreference { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ServiceStartRequested { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task SynchronizeAsync(YavscApiClient api, bool enabled, CancellationToken ct = default)
        {
            Preferences.Add(enabled);
            if (!enabled) DisabledByPreference.TrySetResult();
            if (BlockNextStart)
            {
                ServiceStartRequested.TrySetResult();
                try { await Task.Delay(Timeout.Infinite, ct); }
                finally { ServiceStartCancelled = ct.IsCancellationRequested; }
            }
        }
        public Task DisableAsync(YavscApiClient api, CancellationToken ct = default)
        {
            DisabledWithValidToken = api.HasValidSession;
            return Task.CompletedTask;
        }
    }
}
