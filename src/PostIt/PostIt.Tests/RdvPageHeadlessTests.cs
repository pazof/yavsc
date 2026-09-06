using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using PostIt.ViewModels;
using PostIt.ViewModels.Commands;
using PostIt.Views.Commands;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class RdvPageHeadlessTests
{
    [AvaloniaFact]
    public void Suggested_address_panel_is_hidden_by_default()
    {
        var page = CreatePage(out _);

        var panel = page.FindControl<Grid>("SuggestedAddressPanel");
        var progress = page.FindControl<ProgressBar>("SuggestedAddressProgress");

        Assert.NotNull(panel);
        Assert.NotNull(progress);
        Assert.False(panel!.IsVisible);
        Assert.False(progress!.IsVisible);
    }

    [AvaloniaFact]
    public async Task Suggested_address_panel_and_spinner_follow_viewmodel_state()
    {
        var page = CreatePage(out var vm);
        var panel = page.FindControl<Grid>("SuggestedAddressPanel")!;
        var progress = page.FindControl<ProgressBar>("SuggestedAddressProgress")!;
        var applyButton = page.FindControl<Button>("ApplySuggestedAddressButton")!;

        vm.Address = "Saisie manuelle";
        vm.NotifyReverseGeocodingStarted();
        await Task.Delay(50);

        Assert.True(panel.IsVisible);
        Assert.True(progress.IsVisible);
        Assert.False(applyButton.IsVisible);

        vm.ApplyResolvedAddress("10 rue de Rivoli, 75001 Paris");
        await Task.Delay(50);

        Assert.True(panel.IsVisible);
        Assert.False(progress.IsVisible);
        Assert.True(applyButton.IsVisible);
    }

    private static RdvPage CreatePage(out RdvViewModel vm)
    {
        vm = new RdvViewModel(
            new ActivityInfo { Code = "dev", Name = "Développement" },
            new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
            new CommandFormSummary { Id = 12, ActionName = "Rdv", Title = "Rendez-vous" },
            new BillingApiClient(new RecordingApi(), "https://business.example/api/v1/"));

        var page = new RdvPage { DataContext = vm };
        var nav = new NavigationPage();
        _ = nav.PushAsync(page);
        var window = new Window { Content = nav };
        window.Show();
        return page;
    }

    private sealed class RecordingApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => Task.FromResult(default(T)!);

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}