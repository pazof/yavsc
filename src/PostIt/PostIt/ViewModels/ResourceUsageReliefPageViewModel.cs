using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class ResourceUsageReliefPageViewModel : ViewModelBase, IDisposable
{
    private readonly ResourceUsageApiClient? _client;
    private readonly SessionStatusViewModel? _session;
    private readonly CancellationTokenSource _lifetime = new();
    private bool _disposed;
    private int _accessVersion;
    private bool _snapshotCurrent;

    public ResourceUsageReliefPageViewModel(ResourceUsageApiClient? client = null, SessionStatusViewModel? session = null)
    {
        _client = client;
        _session = session;
        if (session is not null) session.PropertyChanged += OnAccessChanged;
    }

    public ObservableCollection<ResourceUsageSummary> Users { get; } = [];
    public event Action<ResourceUsageSummary>? UsageUpdated;
    public bool IsAdministrator => !_disposed && _session?.IsAdministrator == true;
    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => true; protected set { } }

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(EraseCommand), nameof(RefreshCommand))]
    public partial bool IsBusy { get; set; }
    [ObservableProperty] public partial string SearchText { get; set; } = "";
    [ObservableProperty] public partial ResourceUsageSummary? SelectedUsage { get; set; }
    [ObservableProperty] public partial string PointsText { get; set; } = "";
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(EraseCommand))]
    public partial bool ConfirmErasure { get; set; }
    [ObservableProperty] public partial string StatusMessage { get; set; } = "";

    public IReadOnlyList<ResourceUsageSummary> FilteredUsers => Users.Where(u =>
        string.IsNullOrWhiteSpace(SearchText) ||
        u.UserName.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase) ||
        u.UserId.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

    public string AmountError => TryGetPoints(out _) ? "" :
        "Saisissez un montant positif, avec au plus huit décimales, inférieur ou égal à la consommation.";
    public decimal? ConsumptionAfter => SelectedUsage is { } usage && TryGetPoints(out var points)
        ? usage.Consumption - points : null;

    partial void OnSearchTextChanged(string value) => OnPropertyChanged(nameof(FilteredUsers));
    partial void OnSelectedUsageChanged(ResourceUsageSummary? value)
    {
        PointsText = "";
        ConfirmErasure = false;
        NotifyAmount();
    }
    partial void OnPointsTextChanged(string value)
    {
        ConfirmErasure = false;
        NotifyAmount();
    }

    private void NotifyAmount()
    {
        OnPropertyChanged(nameof(AmountError));
        OnPropertyChanged(nameof(ConsumptionAfter));
        EraseCommand.NotifyCanExecuteChanged();
    }

    private bool TryGetPoints(out decimal points) =>
        decimal.TryParse(PointsText.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out points) &&
        points > 0m && points <= 99999999999999999999.99999999m &&
        decimal.Round(points, 8) == points && SelectedUsage is { } usage && points <= usage.Consumption;

    private bool CanRefresh() => IsAdministrator && !IsBusy;
    private bool CanErase() => CanRefresh() && _snapshotCurrent && ConfirmErasure &&
        SelectedUsage?.IsQuotaReached == true && TryGetPoints(out _);

    private ResourceUsageApiClient RequireAccess()
    {
        if (!IsAdministrator) throw new UnauthorizedAccessException("Le rôle Administrator est requis.");
        return _client ?? throw new InvalidOperationException("Client API d’administration indisponible.");
    }

    public Task InitializeAsync(string? userId = null) => LoadAsync(userId);

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private Task RefreshAsync() => LoadAsync(SelectedUsage?.UserId);

    private async Task LoadAsync(string? preferredUserId)
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearUsage();
        var version = _accessVersion;
        try
        {
            var client = RequireAccess();
            var users = await client.GetAdminOverviewAsync(_lifetime.Token);
            RequireAccess();
            if (version != _accessVersion) return;
            foreach (var user in users.Where(u => u.IsQuotaReached)) Users.Add(user);
            _snapshotCurrent = true;
            SelectedUsage = Users.FirstOrDefault(u => u.UserId == preferredUserId);
            OnPropertyChanged(nameof(FilteredUsers));
            StatusMessage = Users.Count == 0 ? "Aucun utilisateur n’a atteint sa limite."
                : $"{Users.Count} utilisateur(s) à la limite. Sélectionnez le compte à débloquer.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error)
        {
            ClearUsage();
            StatusMessage = $"Chargement impossible : {error.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanErase))]
    private async Task EraseAsync()
    {
        if (IsBusy) return;
        if (!ConfirmErasure)
        {
            var selectedUser = SelectedUsage?.UserName ?? "ce compte";
            var confirmed = await App.ConfirmAsync(
                "Effacer les points de consommation ?",
                $"Vous allez effacer les points pour {selectedUser}. Cette action est définitive et ne peut pas être annulée.",
                confirmText: "Effacer",
                cancelText: "Annuler");
            if (!confirmed)
            {
                StatusMessage = "Confirmez l’effacement pour ce compte et ce montant.";
                return;
            }
            ConfirmErasure = true;
        }

        IsBusy = true;
        var version = _accessVersion;
        try
        {
            var client = RequireAccess();
            if (!_snapshotCurrent || SelectedUsage is not { IsQuotaReached: true } target)
                throw new InvalidOperationException("Actualisez puis sélectionnez un utilisateur à la limite.");
            if (!TryGetPoints(out var points)) throw new InvalidOperationException(AmountError);
            _snapshotCurrent = false;
            var result = await client.EraseAsync(target.UserId, new EraseResourceUsageRequest
            {
                Points = points, ExpectedCredits = target.AppliedCredits + target.CreditBalance
            }, _lifetime.Token);
            RequireAccess();
            if (version != _accessVersion) return;
            Users[Users.IndexOf(target)] = result;
            SelectedUsage = result;
            _snapshotCurrent = true;
            OnPropertyChanged(nameof(FilteredUsers));
            UsageUpdated?.Invoke(result);
            StatusMessage = $"{points:G29} points effacés pour {result.UserName} ({result.UserId}). "
                + (result.IsQuotaReached ? "La limite reste atteinte." : "La limite n’est plus atteinte.")
                + " Les compteurs techniques sont conservés.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error)
        {
            StatusMessage = $"Effacement impossible : {error.Message} Actualisez avant toute nouvelle tentative.";
        }
        finally
        {
            ConfirmErasure = false;
            IsBusy = false;
            EraseCommand.NotifyCanExecuteChanged();
        }
    }

    private void ClearUsage()
    {
        _snapshotCurrent = false;
        SelectedUsage = null;
        PointsText = "";
        ConfirmErasure = false;
        Users.Clear();
        OnPropertyChanged(nameof(FilteredUsers));
    }

    private void OnAccessChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(SessionStatusViewModel.IsAdministrator)) return;
        ++_accessVersion;
        void Apply()
        {
            if (!IsAdministrator)
            {
                ClearUsage();
                StatusMessage = "Le rôle Administrator est requis.";
            }
            OnPropertyChanged(nameof(IsAdministrator));
            EraseCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
        }
        if (Dispatcher.UIThread.CheckAccess()) Apply();
        else Dispatcher.UIThread.Post(Apply);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_session is not null) _session.PropertyChanged -= OnAccessChanged;
        _lifetime.Cancel();
        ClearUsage();
        UsageUpdated = null;
        OnPropertyChanged(nameof(IsAdministrator));
        EraseCommand.NotifyCanExecuteChanged();
        RefreshCommand.NotifyCanExecuteChanged();
    }
}
