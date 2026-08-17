using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc.Api.Client;
using Yavsc.Api.Client.Dtos;

namespace PostIt.ViewModels;

/// <summary>
/// View model for the "Mes cercles" page. CRUD on the caller's own
/// circles (the server scopes every endpoint to the caller's uid
/// since the BlogAcl fix on this branch).
///
/// <para>The view lists circles in <see cref="Circles"/>, supports
/// create / edit via <see cref="DraftName"/>, and exposes
/// per-item Delete and per-item edit commands. <see cref="IsBusy"/>
/// drives a progress overlay during API calls; <see cref="StatusMessage"/>
/// surfaces success / error feedback in the view footer.</para>
/// </summary>
public partial class CirclesPageViewModel : ViewModelBase
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

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public CirclesPageViewModel(CircleApiClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public override bool CanNavigateNext { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    public override bool CanNavigatePrevious { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _client.GetMyCirclesAsync();
            Circles = new ObservableCollection<CircleDto>(list ?? new());
            StatusMessage = $"{Circles.Count} cercle(s)";
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
    public void StartCreate()
    {
        SelectedCircle = null;
        DraftName = string.Empty;
        DraftPublic = false;
        StatusMessage = "Nouveau cercle";
    }

    [RelayCommand]
    public void StartEdit(CircleDto? circle)
    {
        if (circle is null) return;
        SelectedCircle = circle;
        DraftName = circle.Name;
        DraftPublic = circle.Public;
        StatusMessage = $"Édition de « {circle.Name} »";
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(DraftName))
        {
            StatusMessage = "Le nom est obligatoire";
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
                StatusMessage = created is null
                    ? "Création échouée"
                    : $"Cercle « {created.Name} » créé";
            }
            else
            {
                SelectedCircle.Name = DraftName.Trim();
                SelectedCircle.Public = DraftPublic;
                await _client.UpdateCircleAsync(SelectedCircle.Id, SelectedCircle);
                StatusMessage = $"Cercle « {SelectedCircle.Name} » mis à jour";
            }
            await RefreshAsync();
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
    public async Task DeleteAsync(CircleDto? circle)
    {
        if (circle is null) return;
        IsBusy = true;
        try
        {
            await _client.DeleteCircleAsync(circle.Id);
            StatusMessage = $"Cercle « {circle.Name} » supprimé";
            await RefreshAsync();
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
}
