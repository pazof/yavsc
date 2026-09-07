using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Helpers;
using PostIt.Services;
using Yavsc.Api.Client;
using Yavsc.Api.Client.Dtos;

namespace PostIt.ViewModels;

/// <summary>
/// View model for the "Mes cercles" page. CRUD on the caller's own
/// circles (the server scopes every endpoint to the caller's uid
/// since the BlogAcl fix on this branch), plus membership
/// management on the currently selected circle.
///
/// <para>The view lists circles in <see cref="Circles"/>, supports
/// create / edit via <see cref="DraftName"/>, and exposes
/// per-item Delete and per-item edit commands. <see cref="IsBusy"/>
/// drives a progress overlay during API calls; <see cref="StatusMessage"/>
/// surfaces success / error feedback in the view footer.</para>
///
/// <para>When the user selects a circle in the list,
/// <see cref="LoadMembersAsync"/> fetches its members into
/// <see cref="Members"/>. The "Add a member" command
/// (<see cref="OpenAddMemberAsync"/>) is a UI event the view
/// raises to open <c>AddCircleMemberDialog</c>; the dialog
/// raises a <c>Confirmed</c> event back, which the page's
/// code-behind forwards here via
/// <see cref="OnAddMemberConfirmedAsync"/>. The "remove"
/// command is per-row and runs inline.</para>
/// </summary>
public partial class CirclesPageViewModel : ViewModelBase, IActionStatusViewModel
{
    private readonly CircleApiClient _client;

    [ObservableProperty]
    public partial ObservableCollection<CircleDto> Circles { get; set; } = new();

    [ObservableProperty]
    public partial CircleDto? SelectedCircle { get; set; }

    /// <summary>Editor buffer for the new / edited circle's name.</summary>
    [ObservableProperty]
    public partial string DraftName { get; set; } = string.Empty;

    /// <summary>Editor buffer for the new / edited circle's visibility flag.</summary>
    [ObservableProperty]
    public partial bool DraftPublic { get; set; }

    /// <summary>Members of the currently selected circle. Empty
    /// when no circle is selected or after a refresh that
    /// produced an empty list. Updated by
    /// <see cref="LoadMembersAsync"/>.</summary>
    [ObservableProperty]
    public partial ObservableCollection<CircleMemberDto> Members { get; set; } = new();

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Pret.";

    [ObservableProperty]
    public partial StatusNotice ActionStatus { get; set; } = StatusNotice.Info("Pret.");

    public CirclesPageViewModel(CircleApiClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public override bool CanNavigateNext { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    public override bool CanNavigatePrevious { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    /// <summary>
    /// Partial property setter: when the selected circle
    /// changes, refresh the members list. The setter is
    /// invoked by the [ObservableProperty] source generator
    /// for both user selections and programmatic resets.
    /// </summary>
    partial void OnSelectedCircleChanged(CircleDto? value)
    {
        Members = new ObservableCollection<CircleMemberDto>();
        if (value is not null)
        {
            // Fire-and-forget: load members in the background.
            // Errors are routed to StatusMessage inside
            // LoadMembersAsync.
            _ = LoadMembersAsync(value.Id);
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _client.GetMyCirclesAsync();
            Circles = new ObservableCollection<CircleDto>(list ?? new());
            this.SetInfoStatus($"{Circles.Count} cercle(s)");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    internal async Task OpenAddMemberAsync()
    {
        var app = Application.Current as App
            ?? throw new InvalidOperationException("Application PostIt indisponible.");
        var services = app.ServiceProvider
            ?? throw new InvalidOperationException("ServiceProvider PostIt indisponible.");
        var directory = services.GetRequiredService<IUserDirectory>();
        AddCircleMemberDialogViewModel model =
        new AddCircleMemberDialogViewModel(directory);
        // Wire the dialog's Confirmed event to OnAddMemberConfirmedAsync.
        // Without this, the dialog's "Ajouter" button fires the event
        // into the void: no subscriber, the picked user is silently
        // dropped, and nothing is added to the circle. The dialog
        // stays open until the user uses the back gesture — which is
        // how the user noticed the button was a no-op.
        // Async-void is intentional here: Confirmed is an
        // EventHandler<T> (returns void), and bridging to the
        // async Task OnAddMemberConfirmedAsync requires it.
        model.Confirmed += async (_, picked) =>
            await OnAddMemberConfirmedAsync(_, picked);
        await app.PushPageAsync(model);
    }
    /// <summary>
    /// Load the members of one of the caller's circles. The
    /// server scopes the endpoint with a 404 when the circle
    /// doesn't belong to the caller (mirroring the rest of the
    /// circle API); that case flattens to an empty list here.
    /// </summary>
    [RelayCommand]
    public async Task LoadMembersAsync(long circleId)
    {
        IsBusy = true;
        try
        {
            var list = await _client.GetMembersAsync(circleId);
            Members = new ObservableCollection<CircleMemberDto>(list ?? new());
            this.SetInfoStatus($"{Members.Count} membre(s)");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur: {ex.Message}");
            Members = new ObservableCollection<CircleMemberDto>();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void StartCreate()
    {
        SelectedCircle = null;
        DraftName = string.Empty;
        DraftPublic = false;
        this.SetInfoStatus("Nouveau cercle");
    }

    [RelayCommand]
    public void StartEdit(CircleDto? circle)
    {
        if (circle is null) return;
        SelectedCircle = circle;
        DraftName = circle.Name;
        DraftPublic = circle.Public;
        this.SetInfoStatus($"Édition de « {circle.Name} »");
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(DraftName))
        {
            this.SetWarningStatus("Le nom est obligatoire");
            return;
        }

        IsBusy = true;
        try
        {
            if (SelectedCircle is null)
            {
                var created = await _client.CreateCircleAsync(new CircleDto
                {
                    Name = DraftName.Trim(),
                    Public = DraftPublic,
                });
                this.SetStatus(
                    created is null ? "Création échouée" : $"Cercle « {created.Name} » créé",
                    created is null ? StatusSeverity.Warning : StatusSeverity.Info);
            }
            else
            {
                SelectedCircle.Name = DraftName.Trim();
                SelectedCircle.Public = DraftPublic;
                await _client.UpdateCircleAsync(SelectedCircle.Id, SelectedCircle);
                this.SetInfoStatus($"Cercle « {SelectedCircle.Name} » mis à jour");
            }
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DeleteAsync(CircleDto? circle)
    {
        if (circle is null) return;
        IsBusy = true;
        try
        {
            await _client.DeleteCircleAsync(circle.Id);
            this.SetInfoStatus($"Cercle « {circle.Name} » supprimé");
            // If the deleted circle was the selected one,
            // clear the selection so the Members view goes
            // empty too (the partial setter on
            // SelectedCircle will reset Members).
            if (SelectedCircle?.Id == circle.Id)
                SelectedCircle = null;
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }


    /// <summary>
    /// Called by the view when the dialog confirms a
    /// selection. Adds the picked user to the currently
    /// selected circle and refreshes the members list.
    /// </summary>
    public async Task OnAddMemberConfirmedAsync(object? sender, UserSummary picked)
    {
        if (SelectedCircle is null || picked is null) return;
        IsBusy = true;
        try
        {
            await _client.AddMemberAsync(SelectedCircle.Id, picked.Id);
            this.SetInfoStatus($"« {picked.DisplayName} » ajouté au cercle");
            await LoadMembersAsync(SelectedCircle.Id);
        }
        catch (Exception ex)
        {
            // 409 (already a member) is a likely race — surface
            // it as a friendly status, not an error. The
            // server returns 409 for "already a member";
            // YavscApiClient surfaces that as an exception
            // today; future refactors could route 409 into a
            // typed result, but for now the message string is
            // distinctive enough.
            var msg = ex.Message.Contains("409") || ex.Message.Contains("Conflict")
                ? "Déjà membre du cercle"
                : $"Erreur: {ex.Message}";
            this.SetStatus(msg, msg == "Déjà membre du cercle" ? StatusSeverity.Warning : StatusSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Per-row "remove" command. Updates the local
    /// collection in place so the UI doesn't flash.
    /// </summary>
    [RelayCommand]
    public async Task RemoveMemberAsync(CircleMemberDto? member)
    {
        if (member is null || SelectedCircle is null) return;
        IsBusy = true;
        try
        {
            await _client.RemoveMemberAsync(SelectedCircle.Id, member.Id);
            Members.Remove(member);
            this.SetInfoStatus($"« {member.UserName} » retiré du cercle");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
