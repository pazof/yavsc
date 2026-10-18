using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Abstract.Administration;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class AdministrationToolsTests
{
    [AvaloniaTheory]
    [InlineData(400, 800)]
    [InlineData(800, 400)]
    public void All_administration_tabs_and_typed_forms_are_accessible_in_portrait_and_landscape(int width, int height)
    {
        var vm = new AdministrationPageViewModel();
        var page = new AdministrationPage { DataContext = vm };
        var window = new Window { Content = page, Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var tabs = Assert.Single(page.GetVisualDescendants().OfType<TabControl>());
            for (var index = 0; index < tabs.Items.Count; ++index)
            {
                var tab = Assert.IsType<TabItem>(tabs.ContainerFromIndex(index));
                var point = tab.TranslatePoint(new Point(), page);
                Assert.NotNull(point);
                Assert.True(point.Value.X >= 0 && point.Value.X + tab.Bounds.Width <= page.Bounds.Width + 0.5);
            }
            tabs.SelectedIndex = 3;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(page.GetVisualDescendants().OfType<TextBlock>(), block => block.Text == "RequirePkce");
            Assert.Contains(page.GetVisualDescendants().OfType<TextBox>(),
                box => box.DataContext is AdministrationFieldViewModel field && field.Name == "ClientId");
            tabs.SelectedIndex = 5;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(page.GetVisualDescendants().OfType<ComboBox>(),
                box => box.DataContext is AdministrationFieldViewModel field && field.Name == "ToSend");
        }
        finally { window.Close(); }
    }

    [Fact]
    public void Client_form_retains_every_scalar_field_and_validates_numeric_input()
    {
        var settings = new ClientSettings { ClientId = "postit", ClientName = "PostIt", AccessTokenLifetime = 900,
            ConsentLifetime = null, RequireConsent = true, PairWiseSubjectSalt = null };
        var fields = AdministrationFieldViewModel.From(settings);
        Assert.Equal(typeof(ClientSettings).GetProperties().Length, fields.Count);
        Assert.Equal(settings, AdministrationFieldViewModel.Build<ClientSettings>(fields));
        var lifetime = fields.Single(field => field.Name == nameof(ClientSettings.AccessTokenLifetime));
        lifetime.Text = "not-a-number";
        Assert.Throws<ValidationException>(() => AdministrationFieldViewModel.Build<ClientSettings>(fields));
        lifetime.Text = "-1";
        Assert.Throws<ValidationException>(() => AdministrationFieldViewModel.Build<ClientSettings>(fields));
    }

    [Fact]
    public void Mailing_form_round_trips_enum_and_multiline_body()
    {
        var settings = new MailingTemplateSettings { Id = "template", Topic = "Topic", Body = "First\nSecond",
            ReplyToAddress = "reply@example.invalid", ToSend = MailPeriodicity.Monthly };
        var fields = AdministrationFieldViewModel.From(settings);
        Assert.Equal(settings, AdministrationFieldViewModel.Build<MailingTemplateSettings>(fields));
        Assert.True(fields.Single(field => field.Name == "Body").IsMultiline);
        var periodicity = fields.Single(field => field.Name == "ToSend");
        Assert.True(periodicity.IsChoice);
        periodicity.Text = "Unknown";
        Assert.Throws<ValidationException>(() => AdministrationFieldViewModel.Build<MailingTemplateSettings>(fields));
    }

    [Fact]
    public async Task Client_commands_save_typed_data_and_require_confirmation_for_delete()
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new AdminTransport();
        var vm = new AdministrationPageViewModel(null, session: session,
            administrationClient: new AdministrationApiClient(transport, () => "https://example.invalid/api/v1"));
        vm.ClientFields.Single(field => field.Name == "ClientId").Text = "new-client";
        await vm.SaveClientCommand.ExecuteAsync(null);
        Assert.Equal("new-client", transport.SavedClient?.ClientId);
        Assert.Equal(1, vm.SelectedClient?.Id);
        transport.Paths.Clear();
        await vm.DeleteClientCommand.ExecuteAsync(null);
        Assert.Empty(transport.Paths);
        Assert.Contains("confirmation", vm.StatusMessage);
        vm.ConfirmDestructiveAction = true;
        await vm.DeleteClientCommand.ExecuteAsync(null);
        Assert.Contains("DELETE https://example.invalid/api/v1/administration/clients/1", transport.Paths);
        Assert.False(vm.ConfirmDestructiveAction);
        Assert.Null(vm.SelectedClient);
    }

    [Fact]
    public async Task Secrets_are_shown_once_and_cleared_on_selection_and_logout_denies_operations()
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new AdminTransport();
        var vm = new AdministrationPageViewModel(null, session: session,
            administrationClient: new AdministrationApiClient(transport, () => "https://example.invalid/api/v1/"));
        vm.SelectedClient = new AdministrationClient(1, new ClientSettings { ClientId = "client" }, [], []);
        await vm.AddSecretCommand.ExecuteAsync(null);
        Assert.Equal("generated-test-value", vm.NewSecretValue);
        Assert.Equal("", vm.SecretInput);
        vm.NewClientCommand.Execute(null);
        Assert.Null(vm.NewSecretValue);
        session.Api = null;
        session.Refresh();
        transport.Paths.Clear();
        await vm.AddSecretCommand.ExecuteAsync(null);
        Assert.Empty(transport.Paths);
        Assert.Contains("Administrator", vm.StatusMessage);
    }

    [Fact]
    public async Task Secret_response_received_after_logout_is_discarded()
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new AdminTransport
        {
            PendingSecret = new TaskCompletionSource<NewClientSecret>(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        var vm = new AdministrationPageViewModel(null, session: session,
            administrationClient: new AdministrationApiClient(transport, () => "https://example.invalid/api/v1/"));
        vm.SelectedClient = new AdministrationClient(1, new ClientSettings { ClientId = "client" }, [], []);
        var operation = vm.AddSecretCommand.ExecuteAsync(null);
        Assert.True(vm.IsBusy);
        session.Api = null;
        session.Refresh();
        transport.PendingSecret.SetResult(new NewClientSecret(1, "must-not-be-shown", null));
        await operation;
        Assert.Null(vm.NewSecretValue);
        Assert.Empty(vm.Clients);
        Assert.Null(vm.SelectedClient);
        Assert.Contains("Administrator", vm.StatusMessage);
        Assert.Equal(["POST https://example.invalid/api/v1/administration/clients/1/secrets"], transport.Paths);
    }

    [Fact]
    public async Task Refresh_without_administrator_access_clears_previous_administration_data()
    {
        var vm = new AdministrationPageViewModel();
        var client = new AdministrationClient(1, new ClientSettings { ClientId = "previous-client" }, [], []);
        vm.Clients.Add(client);
        vm.SelectedClient = client;
        vm.NewSecretValue = "previous-secret";
        await vm.RefreshAsync();
        Assert.Empty(vm.Clients);
        Assert.Null(vm.SelectedClient);
        Assert.Null(vm.NewSecretValue);
        Assert.Equal("", vm.ClientFields.Single(field => field.Name == "ClientId").Text);
        Assert.Contains("Administrator", vm.StatusMessage);
    }

    [Fact]
    public async Task Import_requires_confirmation_and_uses_only_administration_host()
    {
        await using var profile = new AdministrationAccessTests.ProfileTransport("Administrator");
        var session = new SessionStatusViewModel { Api = profile };
        await session.RefreshAsync();
        await using var transport = new AdminTransport();
        var vm = new AdministrationPageViewModel(null, session: session,
            administrationClient: new AdministrationApiClient(transport, () => "https://example.invalid/api/v1/"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => vm.ImportTransferAsync(new AdministrationTransfer()));
        Assert.Empty(transport.Paths);
        vm.ConfirmDestructiveAction = true;
        var result = await vm.ImportTransferAsync(new AdministrationTransfer());
        Assert.Equal(new ImportResult(0, 0, 0), result);
        Assert.False(vm.ConfirmDestructiveAction);
        Assert.Equal(["POST https://example.invalid/api/v1/administration/transfer"], transport.Paths);
    }

    private sealed class AdminTransport : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public List<string> Paths { get; } = [];
        public ClientSettings? SavedClient { get; private set; }
        public TaskCompletionSource<NewClientSecret>? PendingSecret { get; init; }
        public async Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Paths.Add($"{method} {path}");
            object result;
            if (typeof(T) == typeof(AdministrationClient))
            {
                SavedClient = Assert.IsType<ClientSettings>(body);
                result = new AdministrationClient(1, SavedClient, [], []);
            }
            else if (typeof(T) == typeof(List<AdministrationClient>))
                result = new List<AdministrationClient> { new(1, SavedClient ?? new ClientSettings { ClientId = "client" }, [], []) };
            else if (typeof(T) == typeof(NewClientSecret))
                result = PendingSecret is null
                    ? new NewClientSecret(1, "generated-test-value", null)
                    : await PendingSecret.Task;
            else if (typeof(T) == typeof(ImportResult))
                result = new ImportResult(0, 0, 0);
            else
                throw new NotSupportedException(typeof(T).Name);
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(result))!;
        }
        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Paths.Add($"{method} {path}");
            return Task.CompletedTask;
        }
        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => throw new NotSupportedException();
        public Task<byte[]> DownloadAsync(HttpMethod method, string path, CancellationToken ct = default)
            => throw new NotSupportedException();
        public ValueTask DisposeAsync() { Http.Dispose(); return ValueTask.CompletedTask; }
    }
}
