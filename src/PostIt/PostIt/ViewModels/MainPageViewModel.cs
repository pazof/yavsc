using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Models;
using PostIt.Services;

namespace PostIt.ViewModels;

public partial class MainPageViewModel : ViewModelBase
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
    /// and caused Save to POST a <c>BlogPost</c> with an empty
    /// title — hence the 400 "The Title field is required".</summary>
    [ObservableProperty]
    public partial string DraftTitle { get; set; }

    /// <summary>Editor buffer for the post body. Same pattern as
    /// <see cref="DraftTitle"/>.</summary>
    [ObservableProperty]
    public partial string DraftArticle { get; set; }

    [ObservableProperty]
    public partial ViewModelBase? CurrentViewModel { get; set; }

    public Settings SettingsModel { get; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<BlogPost> Posts { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<BlogPost> FilteredPosts { get; set; }

    [ObservableProperty]
    public partial BlogPost? SelectedPost { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial Settings Settings { get; private set; }

    /// <summary>
    /// API surface that hits the Yavsc.Blogs deployment at
    /// <see cref="Settings.ApiUrl"/>. Owned and constructed by
    /// <c>App.axaml.cs</c> so the same client (and its token store)
    /// is shared with the login flow.
    /// </summary>
    public BlogApiClient? BlogClient { get; }

    public override bool CanNavigateNext { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    public override bool CanNavigatePrevious { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }


    public MainPageViewModel()
    {
        Init(null);
        SettingsModel = new Settings();
        BlogClient = null;
    }

    private void Init(Settings? settings)
    {
        SearchText = string.Empty;
        Posts = new ObservableCollection<BlogPost>();
        FilteredPosts = new ObservableCollection<BlogPost>();
        SelectedPost = null;
        IsBusy = false;
        StatusMessage = "Ready";
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
        Settings = settings ?? new Settings();
        WindowTitle = "PostIt";
        DraftTitle = string.Empty;
        DraftArticle = string.Empty;
        CurrentViewModel = this;
    }

    /// <summary>
    /// Test-friendly constructor: caller supplies a pre-built
    /// <see cref="BlogApiClient"/>. Production code uses the
    /// (Settings, BlogApiClient) overload below.
    /// </summary>
    public MainPageViewModel(BlogApiClient blogClient, Settings? settings = null)
    {
         SettingsModel = new Settings();
        BlogClient = blogClient ?? throw new ArgumentNullException(nameof(blogClient));;

        Init(settings);
        }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedPostChanged(BlogPost? value)
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
        UpdateCommandStates();
    }

    partial void OnIsBusyChanged(bool value) => UpdateCommandStates();

    // Save's CanExecute depends on the buffer: the button must
    // enable as soon as the user has typed a non-whitespace
    // title, regardless of whether a post is selected.
    partial void OnDraftTitleChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnDraftArticleChanged(string value) => SaveCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    internal async Task LoadPosts()
    {
        await ExecuteAsync(async () =>
        {
            var posts = await BlogClient.GetPostsAsync();
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
    internal void Search() => ApplyFilter();

    [RelayCommand]
    internal async Task Save()
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
            // Build a fresh BlogPost from the editor buffer on
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
                var draft = new BlogPost
                {
                    Title = DraftTitle,
                    Article = DraftArticle ?? string.Empty,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                };
                var created = await BlogClient.CreatePostAsync(draft);
                if (created is not null)
                {
                    SelectedPost = created;
                    StatusMessage = $"Created post {created.Id}.";
                }
            }
            else
            {
                var update = new BlogPost
                {
                    Id = SelectedPost.Id,
                    AuthorId = SelectedPost.AuthorId,
                    Photo = SelectedPost.Photo,
                    Title = DraftTitle,
                    Article = DraftArticle ?? string.Empty,
                    DateCreated = SelectedPost.DateCreated,
                    DateModified = DateTime.UtcNow,
                };
                await BlogClient.UpdatePostAsync(SelectedPost.Id, update);
                StatusMessage = $"Saved post {SelectedPost.Id}.";
            }

            await RefreshPostsAsync();
        });
    }

    [RelayCommand]
    internal async Task Delete()
    {
        if (SelectedPost is null || SelectedPost.Id == 0)
        {
            StatusMessage = "Select an existing post before deleting.";
            return;
        }

        await ExecuteAsync(async () =>
        {
            await BlogClient.DeletePostAsync(SelectedPost.Id);
            StatusMessage = $"Deleted post {SelectedPost.Id}.";
            SelectedPost = null;
            await RefreshPostsAsync();
        });
    }

    [RelayCommand]
    internal void OpenSettings()
    {
        CurrentViewModel = SettingsModel;
    }

    private async Task RefreshPostsAsync()
    {
        var posts = await BlogClient.GetPostsAsync();
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
        LoadPostsCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
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
}
