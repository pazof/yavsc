using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc.Blogspot;
using Yavsc.Api.Client;
using Yavsc.Api.Client.Dtos;
using Yavsc.Abstract.BlogSpot;
using Yavsc.Abstract.Identity.Security;
using System.Net.Http;

namespace PostIt.ViewModels;

public sealed class PostAclEntry
{
    public long CircleId { get; init; }
    public string CircleName { get; init; } = string.Empty;
}

/// <summary>
/// View model for the "Gérer l'ACL" modal of a single blog post.
///
/// <para>Loads the caller's circles once on construct (the dropdown
/// only shows circles the user owns), then keeps an in-memory list
/// of the ACL entries for the post. <see cref="AddAsync"/> /
/// <see cref="RevokeAsync"/> are the only mutating verbs; both
/// refresh the list afterwards so the UI stays in sync with the
/// server.</para>
///
/// <para>The server is the source of truth: it scopes every
/// endpoint to the caller's uid and rejects ACL grants on posts
/// the caller doesn't own. This VM does not re-validate that —
/// any 403 / 404 will surface as an exception caught by the
/// command and routed to <see cref="StatusMessage"/>.</para>
/// </summary>
public partial class PostAclDialogViewModel : ViewModelBase
{
    private readonly BlogAclApiClient _aclClient;
    private readonly CircleApiClient _circleClient;

    /// <summary>The post whose ACL is being edited. Set by the
    /// caller (MainPage) when opening the dialog.</summary>
    public BlogPostDto Post { get; }

    [ObservableProperty]
    public partial ObservableCollection<CircleDto>
    MyCircles { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<PostAclEntry>
    AclEntries { get; set; } = new();

    [ObservableProperty]
    public partial CircleDto? SelectedCircleToAdd { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Idempotency gate for <see cref="LoadAsync"/>: the dialog
    /// attaches the load trigger in <c>DataContextChanged</c>,
    /// which can fire more than once if the page is detached
    /// and re-attached (dialog re-use, navigation edge cases)
    /// with a different VM. Without this guard, the second load
    /// would race against the first and could overwrite
    /// <see cref="AclEntries"/> mid-edit. Pattern copied from
    /// <c>Settings.Load</c>.
    /// </summary>
    private bool _loaded;

    /// <summary>True once <see cref="LoadAsync"/> has run at least
    /// once. Exposed for tests; do not bind from XAML.</summary>
    public bool Loaded => _loaded;

    public PostAclDialogViewModel(
        BlogPostDto post,
        BlogAclApiClient aclClient,
        CircleApiClient circleClient)
    {
        Post = post ?? throw new ArgumentNullException(nameof(post));
        _aclClient = aclClient ?? throw new ArgumentNullException(nameof(aclClient));
        _circleClient = circleClient ?? throw new ArgumentNullException(nameof(circleClient));

        AclEntries = new ObservableCollection<PostAclEntry>(post.GetACL().Select(a => ToAclEntry(a.CircleId)));
        SelectedCircleToAdd = null;
    }

    public override bool CanNavigateNext { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    public override bool CanNavigatePrevious { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_loaded) return;

        IsBusy = true;
        try
        {
            // Load circles for the picker. ACL entries come from the
            // BlogPostDto detail payload (source of truth for initial state).
            var circlesTask = _circleClient.GetMyCirclesAsync();
            await Task.WhenAll(circlesTask);

            var circles = circlesTask.Result ?? new List<CircleDto>();
            MyCircles = new ObservableCollection<CircleDto>(circles);

            // Resolve labels now that circles are available.
            AclEntries = new ObservableCollection<PostAclEntry>(AclEntries.Select(a => ToAclEntry(a.CircleId)));


            StatusMessage = $"{AclEntries.Count} autorisation(s)";
            _loaded = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AddAsync()
    {
        if (SelectedCircleToAdd is null)
        {
            StatusMessage = "Sélectionnez un cercle à ajouter";
            return;
        }

        IsBusy = true;
        try
        {
            if (AclEntries.Any(a => a.CircleId == SelectedCircleToAdd.Id))
            {
                StatusMessage = $"Cercle « {SelectedCircleToAdd.Name} » déjà autorisé";
                return;
            }

            var created = await _aclClient.GrantAsync(new PostAccessControlRulePayload
            {
                CircleId = SelectedCircleToAdd.Id,
                BlogPostId = Post.Id
            });
            if (created is not null)
            {
                AclEntries.Add(ToAclEntry(created.CircleId));
                StatusMessage = $"Cercle « {SelectedCircleToAdd.Name} » autorisé";
            }
            else
            {
                StatusMessage = "Autorisation refusée par le serveur";
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // Conflict means the link already exists in backend. Resync
            // from the dedicated ACL API so the UI reflects server truth.
            await ReloadAclEntriesFromServerAsync();
            StatusMessage = $"Cercle « {SelectedCircleToAdd.Name} » déjà autorisé";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task RevokeAsync(PostAclEntry? acl)
    {
        if (acl is null) return;
        IsBusy = true;
        try
        {
            await _aclClient.RevokeAsync(acl.CircleId);
            var existing = AclEntries.FirstOrDefault(e => e.CircleId == acl.CircleId);
            if (existing is not null)
                AclEntries.Remove(existing);
            StatusMessage = "Autorisation révoquée";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadAclEntriesFromServerAsync()
    {
        var allAcl = await _aclClient.GetMyAclAsync();
        var currentPostAcl = (allAcl ?? new List<PostAccessControlRulePayload>())
            .Where(a => a.BlogPostId == Post.Id)
            .Select(a => ToAclEntry(a.CircleId))
            .GroupBy(a => a.CircleId)
            .Select(g => g.First())
            .ToList();
        AclEntries = new ObservableCollection<PostAclEntry>(currentPostAcl);
    }

    private PostAclEntry ToAclEntry(long circleId)
    {
        var circleName = MyCircles.FirstOrDefault(c => c.Id == circleId)?.Name;
        return new PostAclEntry
        {
            CircleId = circleId,
            CircleName = string.IsNullOrWhiteSpace(circleName) ? $"Cercle #{circleId}" : circleName
        };
    }
}
