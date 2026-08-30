using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Blogspot;
using Yavsc.Api.Client;
using PostIt.Helpers;

namespace PostIt.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    /// <summary>Window/tab title. Cosmetic — bound by
    /// <c>MainPage.axaml</c> if at all. Not the post title.</summary>
    [ObservableProperty]
    public partial string WindowTitle { get; set; }

    /// <summary>Editor buffer for the post title. Bound TwoWay to
    /// the title <c>TextBox</c> in <c>MainPage.axaml</c>. The Save
    /// command reads from this buffer (not from
    /// <see cref="SelectedPost"/>) so that typing into a freshly
    /// mounted editor (no post selected yet) is captured. With the
    /// previous "{Binding SelectedPost.Title}" binding, the user's
    /// keystrokes were silently dropped whenever
    /// <c>SelectedPost was null</c>, which made the editor a trap
    /// and caused Save to POST a <c>BlogPostDto</c> with an empty
    /// title — hence the 400 "The Title field is required".</summary>
    [ObservableProperty]
    public partial string DraftTitle { get; set; }

    /// <summary>Editor buffer for the post body. Same pattern as
    /// <see cref="DraftTitle"/>.</summary>
    [ObservableProperty]
    public partial string DraftArticle { get; set; }

    /// <summary>Editor buffer for the post's publication state.
    /// Reflects the server-side <c>IsPublished</c> flag (the
    /// existence of a row in <c>BlogSpotPublication</c>) and
    /// is pushed to the server via
    /// <see cref="BlogApiClient.SetPublishAsync"/> on explicit
    /// toggle — it is NOT included in the regular Save
    /// payload, mirroring the wire contract where
    /// <c>BlogPostDto</c> doesn't carry <c>Publish</c> as a
    /// mutable field. Toggling is its own action.</summary>
    [ObservableProperty]
    public partial bool DraftIsPublished { get; set; }
    public bool IsLoaded { get; private set; }
    public Settings SettingsModel { get; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<BlogPostDto> Posts { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<BlogPostDto> FilteredPosts { get; set; }

    [ObservableProperty]
    public partial BlogPostDto? SelectedPost { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial Settings Settings { get; private set; }

    [RelayCommand]
    internal async Task RefreshAsync()
    {
        await ExecuteAsync(async () =>
        {
            var posts = await BlogClient!.GetPostsAsync();
            Posts.Clear();
            foreach (var post in posts.OrderByDescending(p => p.DateModified))
            {
                Posts.Add(post);
            }
            ApplyFilter();
            StatusMessage = $"Loaded {Posts.Count} posts.";
        });
    }

    [RelayCommand]
    internal async Task SearchAsync() {
        await RefreshAsync();
        ApplyFilter();
    }

    [RelayCommand]
    internal async Task SaveAsync()
    {
        // The button is already disabled when the title is empty
        // (see CanSave), but the test path (and any programmatic
        // ICommand.Execute) bypasses CanExecute, so we still
        // guard here. Better to no-op with a status message
        // than to send a request the server will reject.
        if (string.IsNullOrWhiteSpace(DraftTitle))
        {
            StatusMessage = "Title is required.";
            return;
        }

        await ExecuteAsync(async () =>
        {
            // Build a fresh BlogPostDto from the editor buffer on
            // every Save — we no longer mutate SelectedPost in
            // place. The previous behaviour copied the buffer
            // (which was a no-op when SelectedPost was null)
            // back onto the model and relied on a
            // [Required] violation to surface the missing
            // input; the new shape keeps the editor buffer as
            // the single source of truth for outgoing payloads
            // and the selected post as a read-only hint for
            // the update path.
            if (SelectedPost is null || SelectedPost.Id == 0)
            {
                var draft = new BlogPostDto
                {
                    Title = DraftTitle,
                    Article = DraftArticle ?? string.Empty,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    IsPublished = DraftIsPublished
                };
                var created = await BlogClient!.CreatePostAsync(draft);
                if (created is not null)
                {
                    SelectedPost = created;
                    StatusMessage = $"Created post {created.Id}.";
                }
            }
            else
            {
                var update = new BlogPostDto
                {
                    Id = SelectedPost.Id,
                    AuthorId = SelectedPost.AuthorId,
                    Photo = SelectedPost.Photo,
                    Title = DraftTitle,
                    Article = DraftArticle ?? string.Empty,
                    DateCreated = SelectedPost.DateCreated,
                    DateModified = DateTime.UtcNow,
                };
                await BlogClient!.UpdatePostAsync(SelectedPost.Id, update);
                StatusMessage = $"Saved post {SelectedPost.Id}.";
            }

            await RefreshPostsAsync();
        });
    }

    [RelayCommand]
    internal async Task DeleteAsync()
    {
        if (SelectedPost is null || SelectedPost.Id == 0)
        {
            StatusMessage = "Select an existing post before deleting.";
            return;
        }

        await ExecuteAsync(async () =>
        {
            await BlogClient!.DeletePostAsync(SelectedPost.Id);
            StatusMessage = $"Deleted post {SelectedPost.Id}.";
            SelectedPost = null;
            await RefreshPostsAsync();
        });
    }

    /// <summary>
    /// Toggle the publication state of the currently selected
    /// post. Pushes the new state to
    /// <c>PUT /api/BlogApi/{id}/publish</c> and reflects it
    /// locally in <see cref="DraftIsPublished"/> + the
    /// selected post so the UI updates without a full
    /// refresh.
    ///
    /// <para>The toggle is its own action — separate from Save
    /// — because <c>Publish</c> is not part of the
    /// <c>BlogPostDto</c> payload. Bundling it into Save
    /// would require a wire-shape change and a second server
    /// overload; the dedicated endpoint keeps the wire
    /// contract clean.</para>
    /// </summary>
    public async Task SetPublishStateAsync(bool publish)
    {
        if (SelectedPost is null || SelectedPost.Id == 0)
        {
            StatusMessage = "Sélectionnez un billet existant pour changer sa publication.";
            return;
        }

        await ExecuteAsync(async () =>
        {
            // The checkbox updates DraftIsPublished before the command is
            // executed. Using the current bound value avoids the
            // double-toggle bug in which the UI has already flipped the
            // state and the command flips it again.
            await BlogClient!.SetPublishAsync(SelectedPost.Id, publish);
            DraftIsPublished = publish;
            // Mirror into the selected post so a subsequent
            // RefreshPostsAsync() doesn't blow away the
            // locally flipped state until the round-trip
            // re-hydrates it.
            SelectedPost.IsPublished = publish;
            StatusMessage = publish
                ? $"Billet {SelectedPost.Id} publié."
                : $"Billet {SelectedPost.Id} remis en brouillon.";
        });
    }

    [RelayCommand]
    internal async Task TogglePublishAsync()
    {
        await SetPublishStateAsync(DraftIsPublished);
    }

    /// <summary>
    /// DEV ONLY: open the signature capture page. The production
    /// entry point is a SignalR push from Yavsc.Org ("devis
    /// received, sign here"); this command is the dev-time
    /// shortcut to reach the page without that infrastructure.
    /// Aligned on the same VM-first navigation pattern as
    /// <see cref="OpenSettings"/>: the VM resolves the target VM
    /// through <see cref="Services"/>, the <c>ViewLocator</c> picks
    /// the matching <c>Control</c> at bind time. No
    /// <c>Click</code> handler, no <c>App.ServiceProvider</c>
    /// access from the view layer.
    /// </summary>
    [RelayCommand]
    internal async Task OpenSignatureDevAsync()
    {
        await ((App)App.Current!).PushPageAsync(SignatureModel).ConfigureAwait(true);
    }


    [RelayCommand(CanExecute = nameof(CanManageAcl))]
    public async Task ManageAclAsync()
    {
        if (SelectedPost is null)
        {
            StatusMessage = "Select an existing post before managing ACL.";
            return;
        }

        var postForAcl = SelectedPost;
        try
        {
            var detailed = await BlogClient!.GetPostAsync(SelectedPost.Id).ConfigureAwait(true);
            if (detailed is not null)
            {
                postForAcl = detailed;
                SelectedPost = detailed;
            }
        }
        catch
        {
            // Keep the dialog usable even if the detail refresh fails.
        }

        await ((App)App.Current!).PushPageAsync(GetACLViewModel(postForAcl)).ConfigureAwait(true);
    }

    [RelayCommand]
    public async Task OpenCirclesAsync()
    {
        var circlesVm = ResolveServices().GetRequiredService<CirclesPageViewModel>();
        await ((App)App.Current!).PushPageAsync(circlesVm).ConfigureAwait(true);
    }

    private ViewModelBase GetACLViewModel(BlogPostDto selectedPost)
    {
        var sp = ResolveServices();
        var aclClient = sp.GetRequiredService<BlogAclApiClient>();
        var circleClient = sp.GetRequiredService<CircleApiClient>();
        return new PostAclDialogViewModel(selectedPost, aclClient, circleClient);
    }

    /// <summary>
    /// API surface that hits the Yavsc.Blogs deployment at
    /// <see cref="Settings.ApiUrl"/>. Owned and constructed by
    /// <c>App.axaml.cs</c> so the same client (and its token store)
    /// is shared with the login flow.
    /// </summary>
    public BlogApiClient? BlogClient { get; }

    /// <summary>
    /// DI container the VM uses to resolve navigation targets
    /// (other ViewModels) when the user clicks a toolbar button
    /// that opens a sub-screen. Owned by <c>App.ServiceProvider</c>
    /// in production; injected directly in tests. The VM resolves
    /// <em>ViewModels</em> via this provider, never Views — the
    /// actual <see cref="Control"/> to push is decided by
    /// <see cref="ViewLocator"/> at bind time, per CONTRIBUTING.md
    /// §"Navigation (PostIt)".
    /// </summary>
    public IServiceProvider? Services { get; }

    private SignaturePageViewModel? _signatureModel;

    /// <summary>
    /// Resolved on first access. Lazy so the test path (which
    /// never pushes <c>SignaturePage</c>) does not require a
    /// fully-built DI graph just to construct the VM. Mirrors the
    /// pattern of <see cref="SettingsModel"/> for the Settings case.
    /// </summary>
    public SignaturePageViewModel SignatureModel =>
        _signatureModel ??= ResolveSignatureModel();

    public override bool CanNavigateNext { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    public override bool CanNavigatePrevious { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    private SignaturePageViewModel ResolveSignatureModel()
    {
        var sp = ResolveServices();
        return sp.GetRequiredService<SignaturePageViewModel>();
    }

    private IServiceProvider ResolveServices()
    {
        return Services ?? (Application.Current as App)?.ServiceProvider ??
            throw new InvalidOperationException(
                "No IServiceProvider available for navigation. Inject one in tests " +
                "or ensure App.ServiceProvider is initialized in production.");
    }


    public MainViewModel()
    {
        SettingsModel = new Settings();
        Init(SettingsModel);
        BlogClient = null;
    }

    private void Init(Settings? settings)
    {
        Posts = new ObservableCollection<BlogPostDto>();
        FilteredPosts = new ObservableCollection<BlogPostDto>();
        SelectedPost = null;
        IsBusy = false;
        StatusMessage = "Ready";
        Settings = settings ?? new Settings();
        SearchText = Settings.SearchText;
        WindowTitle = "PostIt";
        DraftTitle = string.Empty;
        DraftArticle = string.Empty;
        DraftIsPublished = false;
        IsLoaded = false;
        // Production path: DI injects the canonical Settings singleton
        // and we use it as-is. Test path: tests call this constructor
        // without a Settings argument; we fall back to a fresh
        // instance so the fixture can build a self-contained VM.
        // The previous "?? new Settings()" silently worked in prod
        // too, which is what allowed a second Settings instance to
        // race the singleton and crash the postit://callback binding
        // sink; that crash is fixed in Settings.OnPropertyChanged
        // (thread-safe dispatcher marshalling) so the duplicate
        // instance is now merely wasteful, not dangerous.

        Settings.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Settings.SearchText))
            {
                SearchText = Settings.SearchText;
                ApplyFilter();
            }
        };

    }

    /// <summary>Save is enabled as soon as the user has typed
    /// a non-whitespace title in the editor, regardless of
    /// whether a post is selected. The "no selection" case is
    /// the create-new-post path; the "with selection" case is
    /// the update path. Both read from the editor buffer.
    /// Previously this also required <c>SelectedPost is not null</c>
    /// — which contradicted the create-new-post intent and
    /// forced the buggy "draft with empty title" branch.</summary>
    private bool CanSave() => !IsBusy && !string.IsNullOrWhiteSpace(DraftTitle);
    private bool CanDelete() => SelectedPost is not null && SelectedPost.Id != 0 && !IsBusy;
    private bool CanManageAcl() => SelectedPost is not null && SelectedPost.Id != 0 && !IsBusy;

    /// <summary>
    /// Test-friendly constructor: caller supplies a pre-built
    /// <see cref="BlogApiClient"/>. Production code uses the
    /// (Settings, BlogApiClient) overload below.
    /// </summary>
    public MainViewModel(BlogApiClient blogClient, Settings? settings = null, IServiceProvider? services = null)
    {
        SettingsModel = new Settings();
        BlogClient = blogClient ?? throw new ArgumentNullException(nameof(blogClient)); ;
        Services = services;
        Init(settings);
    }

    partial void OnSearchTextChanged(string value)
    {
        if (Settings is not null && Settings.SearchText != value)
        {
            Settings.SearchText = value;
        }
        ApplyFilter();
    }

    partial void OnSelectedPostChanged(BlogPostDto? value)
    {
        // Mirror the selection into the editor buffer so the
        // XAML-bound TextBox/TextEditor show the right content
        // when the user clicks a post in the list. When the
        // selection is cleared (e.g. after a successful create
        // rebinds to the server-issued record, or Delete
        // nulls it out), the buffer is reset so the editor
        // doesn't show stale content.
        DraftTitle = value?.Title ?? string.Empty;
        DraftArticle = value?.Article ?? string.Empty;
        // Mirror publication state too. Defaults to false on
        // null selection so a fresh draft starts unpublished.
        DraftIsPublished = value?.IsPublished ?? false;
        UpdateCommandStates();
    }

    partial void OnIsBusyChanged(bool value) => UpdateCommandStates();

    // Save's CanExecute depends on the buffer: the button must
    // enable as soon as the user has typed a non-whitespace
    // title, regardless of whether a post is selected.
    partial void OnDraftTitleChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnDraftArticleChanged(string value) => SaveCommand.NotifyCanExecuteChanged();


    private async Task RefreshPostsAsync()
    {
        var posts = await BlogClient!.GetPostsAsync();
        Posts.Clear();
        foreach (var post in posts.OrderByDescending(p => p.DateModified))
        {
            Posts.Add(post);
        }
        ApplyFilter();

        if (SelectedPost is not null)
        {
            SelectedPost = Posts.FirstOrDefault(post => post.Id == SelectedPost.Id) ?? SelectedPost;
        }
    }

    private void ApplyFilter()
    {
        if (Posts is null) return;

        var query = SearchText?.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? Posts.OrderByDescending(p => p.DateModified)
            : Posts.Where(p => p.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
                || p.Article?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
                || p.AuthorId?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
                .OrderByDescending(p => p.DateModified);

        FilteredPosts.Clear();
        foreach (var post in filtered)
        {
            FilteredPosts.Add(post);
        }
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Working...";
            await action();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateCommandStates()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }


    internal async Task InitializeAsync()
    {
        if (!IsLoaded)
        {
            await RefreshAsync();
            IsLoaded = true;
        }
    }
}
