using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

/// <summary>
/// View model for the "add a Yavsc user to a circle" modal.
///
/// <para>Resolves users through <see cref="IUserDirectory"/>
/// (which delegates to <c>/api/user-search</c>); the caller
/// (CirclesPage) decides whether to add the picked user to
/// the circle by calling
/// <see cref="AddCircleMemberDialogViewModel.AddCommand"/>
/// (which is bound to the dialog's "Ajouter" button).</para>
///
/// <para>The dialog itself doesn't know the target
/// <c>CircleId</c>: that's set by the caller via the
/// constructor and the dialog only triggers
/// <see cref="IUserDirectory.SearchAsync"/> against the
/// <see cref="SearchQuery"/> string. The "Add" command
/// returns the picked <see cref="UserSummary"/> via the
/// <see cref="Confirmed"/> event, and the hosting
/// <c>CirclesPage</c> then calls
/// <see cref="CircleApiClient.AddMemberAsync"/>.</para>
/// </summary>
public partial class AddCircleMemberDialogViewModel : ViewModelBase
{
    private readonly IUserDirectory _directory;

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<UserSummary> Results { get; set; } = new();

    [ObservableProperty]
    public partial UserSummary? Selected { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Raised when the user confirms a selection. The hosting
    /// <c>CirclesPage</c> subscribes to this event and calls
    /// <c>CircleApiClient.AddMemberAsync</c> with the target
    /// circle id + the picked user's id. The dialog itself
    /// does not know the circle id by design: separation of
    /// concerns — the modal is a user picker, not a
    /// "circle joiner" form.
    /// </summary>
    public event EventHandler<UserSummary>? Confirmed;

    public AddCircleMemberDialogViewModel(IUserDirectory directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    public override bool CanNavigateNext { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    public override bool CanNavigatePrevious { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    /// <summary>
    /// Search the directory for users matching the current
    /// <see cref="SearchQuery"/>. Triggered explicitly via the
    /// "Rechercher" button — no debouncing, so the caller
    /// stays in control of how often the network is hit.
    /// </summary>
    [RelayCommand]
    public async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            Results.Clear();
            StatusMessage = "Tapez un nom ou un email";
            return;
        }

        IsBusy = true;
        try
        {
            var hits = await _directory.SearchAsync(SearchQuery, CancellationToken.None).ConfigureAwait(true);
            Results = new ObservableCollection<UserSummary>(hits ?? Array.Empty<UserSummary>());
            StatusMessage = $"{Results.Count} résultat(s)";
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

    /// <summary>
    /// Raise <see cref="Confirmed"/> for the currently selected
    /// user. No-op when no selection has been made — keeps the
    /// UI from firing an event with a null payload.
    /// </summary>
    [RelayCommand]
    public void Add()
    {
        if (Selected is null)
        {
            StatusMessage = "Sélectionnez un utilisateur";
            return;
        }
        Confirmed?.Invoke(this, Selected);
    }
}
