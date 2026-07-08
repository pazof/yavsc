using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Models;
using PostIt.Services;

namespace PostIt.ViewModels;

public partial class MainPageViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; }

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
    ThemeVariant themeVariant = ThemeVariant.Default;

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
        Title = "PostIt";
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

    partial void OnSelectedPostChanged(BlogPost? value) => UpdateCommandStates();

    partial void OnIsBusyChanged(bool value) => UpdateCommandStates();

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
        // No selection means "create a new post from the editor".
        // The server is the source of truth, so we POST without an id
        // and let BlogApiController assign one. The local view-model
        // is then rebound to the server-issued record.
        if (SelectedPost is null)
        {
            var draft = new BlogPost
            {
                Title = string.Empty,
                Article = string.Empty,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            await ExecuteAsync(async () =>
            {
                var created = await BlogClient.CreatePostAsync(draft);
                if (created is not null)
                {
                    SelectedPost = created;
                    StatusMessage = $"Created post {created.Id}.";
                }
            });
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (SelectedPost.Id == 0)
            {
                SelectedPost.DateCreated = DateTime.UtcNow;
                SelectedPost.DateModified = DateTime.UtcNow;
                var created = await BlogClient.CreatePostAsync(SelectedPost);
                if (created is not null)
                {
                    SelectedPost = created;
                    StatusMessage = $"Created post {created.Id}.";
                }
            }
            else
            {
                SelectedPost.DateModified = DateTime.UtcNow;
                await BlogClient.UpdatePostAsync(SelectedPost.Id, SelectedPost);
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

    private bool CanSave() => SelectedPost is not null && !IsBusy;
    private bool CanDelete() => SelectedPost is not null && SelectedPost.Id != 0 && !IsBusy;
}
