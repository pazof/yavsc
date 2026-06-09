using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Models;
using PostIt.Services;

namespace PostIt.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _apiUrl = "http://localhost:5000";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _bearerToken;

    [ObservableProperty]
    private ObservableCollection<BlogPost> _posts = new();

    [ObservableProperty]
    private ObservableCollection<BlogPost> _filteredPosts = new();

    [ObservableProperty]
    private BlogPost? _selectedPost;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isBusy;

    public MainViewModel()
    {
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
    public async Task LoadPostsAsync()
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
    public void Search()
    {
        ApplyFilter();
    }

    [RelayCommand]
    public async Task SaveAsync()
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
    public async Task DeleteAsync()
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
    public void New()
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

    private BlogApiClient CreateClient()
        => new BlogApiClient(ApiUrl, BearerToken);

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
