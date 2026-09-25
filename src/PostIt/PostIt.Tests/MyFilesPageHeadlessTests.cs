using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Api.Client;

namespace PostIt.Tests;

/// <summary>
/// Headless UI coverage for the "Mes fichiers" page
/// (<see cref="MyFilesPage"/>). The user reported that once a file
/// is listed, "l'item est inerte au click, et ses boutons restent
/// grisés, désactivés" — neither the row nor its action buttons
/// (Télécharger / Supprimer) respond to a click.
///
/// <para>These tests mount a real <see cref="MyFilesPage"/> under
/// the Avalonia headless platform with a stub
/// <see cref="IYavscApiClient"/> that lists one file, then walk the
/// realised visual tree to find the row's action buttons and
/// assert the two things that would silently break and reproduce
/// the "grayed / inert" symptom:</para>
/// <list type="number">
///   <item><description>The <c>Command</c> binding
///   (<c>$parent[vm:MyFilesViewModel].DownloadFileCommand</c>)
///   resolved to a non-null command — a null command leaves the
///   button disabled (grayed) and click-silent.</description></item>
///   <item><description>The command is <b>armed</b>
///   (<c>CanExecute</c> true) for the listed file — an
///   <c>AsyncRelayCommand</c> with
///   <c>AllowConcurrentExecutions=false</c> (the
///   <see cref="CommunityToolkit.Mvvm.Input.RelayCommandAttribute"/>
///   default) disables itself while running; a button stuck
///   "running" is exactly the grayed-and-inert report.</description></item>
///   <item><description>Executing the command actually invokes the
///   file download / delete on the API client — the row
///   "activates" on click instead of being inert.</description></item>
/// </list>
/// <para>The OS save picker is absent under headless, so
/// <c>FileSaveHelpers.SaveAsync</c> throws or cancels and the
/// download command's <c>catch</c> sets an error status — but the
/// API download is awaited <em>before</em> the picker, so the call
/// is recorded regardless. We do not <c>await</c> the command's
/// <c>ExecutionTask</c> to completion, to avoid blocking on the
/// (platform-dependent) picker.</para>
/// </summary>
public class MyFilesPageHeadlessTests
{
    private static (MyFilesPage page, MyFilesViewModel vm, StubFsApi api) CreatePage()
    {
        var api = new StubFsApi();
        var fsClient = new UserFilesApiClient(api, "https://blogs.example/api/v1/");
        var vm = new MyFilesViewModel(fsClient);
        var page = new MyFilesPage { DataContext = vm };
        return (page, vm, api);
    }

    /// <summary>
    /// Mount the page the way the app does — pushed onto a
    /// <see cref="NavigationPage"/> shown in a <see cref="Window"/>
    /// (a <see cref="ContentPage"/> realizes its content template via
    /// the navigation host, cf. <c>RdvPageHeadlessTests</c>) — after
    /// listing a single file by awaiting
    /// <see cref="MyFilesViewModel.InitializeAsync"/>. Populating
    /// <see cref="MyFilesViewModel.Files"/> before the first layout
    /// pass lets the <c>ItemsControl</c> realise its item templates
    /// under headless.
    /// </summary>
    private static async Task<(MyFilesPage page, MyFilesViewModel vm, StubFsApi api)> MountAsync()
    {
        var (page, vm, api) = CreatePage();
        await vm.InitializeAsync();
        var nav = new NavigationPage();
        nav.PushAsync(page).GetAwaiter().GetResult();
        var window = new Window { Content = nav };
        window.Show();
        // Let bindings / the ItemsControl item templates apply.
        await Task.Delay(50);
        return (page, vm, api);
    }

    /// <summary>
    /// Find the first action button in a file row whose content
    /// matches <paramref name="content"/>. The file rows live in an
    /// <c>ItemsControl</c> DataTemplate (no <c>x:Name</c>), so we
    /// walk the visual tree.
    /// </summary>
    private static Button? FindRowButton(MyFilesPage page, string content)
        => page.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(b => b.Content is string s && s == content);

    [AvaloniaFact]
    public async Task Download_button_on_a_file_row_is_bound_armed_and_fires_on_click()
    {
        var (page, vm, api) = await MountAsync();

        // Sanity: the listing populated a file row.
        Assert.NotEmpty(vm.Files);
        Assert.NotNull(vm.Files[0].Id);

        var downloadBtn = FindRowButton(page, "Télécharger");
        Assert.NotNull(downloadBtn);
        Assert.NotNull(downloadBtn!.Command);               // binding resolved (not grayed-via-null)
        var param = downloadBtn.CommandParameter;
        Assert.True(downloadBtn.Command!.CanExecute(param), // armed (not stuck running)
            "Télécharger must be armed (CanExecute true) for a listed file.");

        // Act: click → fire the command.
        downloadBtn.Command.Execute(param);

        // Assert: the row activated — the API download was invoked
        // (it is awaited before the headless-absent save picker).
        var recorded = false;
        for (int i = 0; i < 40; i++)
        {
            if (api.Calls.Any(c => c.Method == HttpMethod.Get && c.Path == "fs/file/42"))
            {
                recorded = true;
                break;
            }
            await Task.Delay(25);
        }
        Assert.True(recorded,
            "Clicking Télécharger must invoke GET fs/file/{id} on the API client.");
    }

    [AvaloniaFact]
    public async Task Delete_button_on_a_file_row_is_bound_armed_and_fires_on_click()
    {
        var (page, vm, api) = await MountAsync();

        Assert.NotEmpty(vm.Files);

        var deleteBtn = FindRowButton(page, "Supprimer");
        Assert.NotNull(deleteBtn);
        Assert.NotNull(deleteBtn!.Command);
        var param = deleteBtn.CommandParameter;
        Assert.True(deleteBtn.Command!.CanExecute(param),
            "Supprimer must be armed (CanExecute true) for a listed file.");

        deleteBtn.Command.Execute(param);

        var recorded = false;
        for (int i = 0; i < 40; i++)
        {
            if (api.Calls.Any(c => c.Method == HttpMethod.Delete))
            {
                recorded = true;
                break;
            }
            await Task.Delay(25);
        }
        Assert.True(recorded,
            "Clicking Supprimer must invoke DELETE fs/{path} on the API client.");
    }

    private sealed class StubFsApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();

        public List<(HttpMethod Method, string Path, object? Body)> Calls { get; } = new();

        private void Record(HttpMethod method, string path, object? body)
            => Calls.Add((method, path, body));

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Record(method, path, body);
            if (typeof(T) == typeof(UserDirectoryDto))
            {
                var dir = new UserDirectoryDto
                {
                    UserName = "alice",
                    Files = new[]
                    {
                        new FileEntryDto { Id = 42, Name = "note.txt", Size = 4 }
                    }
                };
                return Task.FromResult((T)(object)dir);
            }
            return Task.FromResult(default(T)!);
        }

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Record(method, path, body);
            return Task.CompletedTask;
        }

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync<T>(method, path, (object?)null, ct);

        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync(method, path, (object?)null, ct);

        public Task<byte[]> DownloadAsync(HttpMethod method, string path, CancellationToken ct = default)
        {
            Record(method, path, null);
            return Task.FromResult(new byte[] { 1, 2, 3, 4 });
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}