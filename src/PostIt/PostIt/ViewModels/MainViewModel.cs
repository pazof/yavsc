using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using PostIt.Models;
using PostIt.Services;
using Avalonia.Styling;

namespace PostIt.ViewModels;

public partial class MainPageViewModel : ViewModelBase
{

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial ViewModelBase? CurrentViewModel { get; set; }
    public SettingsPageViewModel SettingsModel { get; }
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; }

    [ObservableProperty]
    public partial string BearerToken { get; set; }

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
    public override bool CanNavigateNext { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    public override bool CanNavigatePrevious { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    public MainPageViewModel()
    {
        SearchText = string.Empty;
        Posts = new ObservableCollection<BlogPost>();
        FilteredPosts = new ObservableCollection<BlogPost>();
        SelectedPost = null;
        BearerToken = string.Empty;
        IsBusy = false;
        StatusMessage = "Ready";
        Settings = new Settings();
        Title = "PostIt";
        CurrentViewModel = this;
        SettingsModel = new SettingsPageViewModel();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedPostChanged(BlogPost? value)
    {
        UpdateCommandStates();
    }

    partial void OnIsBusyChanged(bool value)
    {
        UpdateCommandStates();
    }

    [RelayCommand]
    internal async Task LoadPosts()
    {
        await ExecuteAsync(async () =>
        {
            using var client = CreateClient();
            var posts = await client.GetPostsAsync();
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
    internal void Search()
    {
        ApplyFilter();
    }

    [RelayCommand]
    internal async Task Save()
    {
        if (SelectedPost is null)
        {
            StatusMessage = "A post must be selected before saving.";
            return;
        }

        await ExecuteAsync(async () =>
        {
            using var client = CreateClient();

            if (SelectedPost.Id == 0)
            {
                SelectedPost.DateCreated = DateTime.UtcNow;
                SelectedPost.DateModified = DateTime.UtcNow;
                var created = await client.CreatePostAsync(SelectedPost);
                if (created is not null)
                {
                    SelectedPost = created;
                    StatusMessage = $"Created post {created.Id}.";
                }
            }
            else
            {
                SelectedPost.DateModified = DateTime.UtcNow;
                await client.UpdatePostAsync(SelectedPost.Id, SelectedPost);
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
            using var client = CreateClient();
            await client.DeletePostAsync(SelectedPost.Id);
            StatusMessage = $"Deleted post {SelectedPost.Id}.";
            SelectedPost = null;
            await RefreshPostsAsync();
        });
    }

    [RelayCommand]
    internal void New()
    {
        SelectedPost = new BlogPost
        {
            Title = string.Empty,
            Article = string.Empty,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        StatusMessage = "New blog post ready.";
    }

    [RelayCommand]
    internal void OpenSettings()
    {
        // Appeler la méthode OpenSettings de la vue MainWindow
        CurrentViewModel = SettingsModel;
    }

    private async Task RefreshPostsAsync()
    {
        using var client = CreateClient();
        var posts = await client.GetPostsAsync();
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

    /// <summary>
    /// Performs an interactive Authorization Code + PKCE login against the
    /// configured authority and stores the resulting access token in
    /// <see cref="BearerToken"/>. No client secret is sent; PKCE prevents
    /// authorization-code interception by relying on a per-request verifier
    /// generated locally and never leaving the device.
    /// </summary>
    /// <param name="browser">
    /// Platform-specific <see cref="IBrowser"/> implementation. On desktop
    /// pass a <c>LoopbackBrowser</c>; on Android a custom-scheme
    /// deep-link browser is required.
    /// </param>
    public async Task LoginAsync(IBrowser browser)
    {
        IsBusy = true;
        StatusMessage = "Signing in...";
        try
        {
            var client = new OidcClient(Settings.GetOidcClientOptions(browser));
            var loginResult = await client.LoginAsync(new LoginRequest()).ConfigureAwait(false);

            if (loginResult.IsError)
            {
                StatusMessage = loginResult.Error ?? "Login failed.";
                return;
            }

            BearerToken = loginResult.AccessToken ?? string.Empty;
            StatusMessage = string.IsNullOrEmpty(BearerToken)
                ? "Login succeeded but no access token was returned."
                : "Signed in.";
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

    private BlogApiClient CreateClient()
        => new BlogApiClient(Settings.ApiUrl, BearerToken);

    private void UpdateCommandStates()
    {
        LoadPostsCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        NewCommand.NotifyCanExecuteChanged();
    }

    private bool CanSave() => SelectedPost is not null && !IsBusy;
    private bool CanDelete() => SelectedPost is not null && SelectedPost.Id != 0 && !IsBusy;
}
