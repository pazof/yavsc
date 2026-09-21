using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Controls;
using PostIt.Helpers;
using PostIt.Models;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

/// <summary>
/// Validation (accept / reject) of a single estimate, with an optional
/// signature. Backs the estimate validation page opened from
/// <see cref="EstimateListPageViewModel"/> for either perspective.
///
/// The server (<c>POST api/v1/front/query/accept</c>) resolves the
/// caller's role from the authenticated user compared to the query's
/// <c>PerformerId</c>/<c>ClientId</c>; the perspective passed in here
/// is only a UI hint (the <see cref="RoleLabel"/>). The billing code
/// sent to the server is the estimate's <see cref="EstimateDto.CommandType"/>
/// and the query id is its <see cref="EstimateDto.CommandId"/>.
/// </summary>
public partial class EstimateValidationPageViewModel : ViewModelBase, IActionStatusViewModel
{
    private readonly EstimateDto _estimate;
    private readonly EstimateListPerspective _perspective;
    private readonly FrontOfficeApiClient _frontClient;
    private SignaturePadControl? _pad;

    public string Title => _estimate.Title;
    public string Description => _estimate.Description;
    public long EstimateId => _estimate.Id;
    public string CommandType => _estimate.CommandType;
    public IReadOnlyList<EstimateLineDto> Bill => _estimate.Bill;
    public DateTime ProviderValidationDate => _estimate.ProviderValidationDate;
    public DateTime ClientValidationDate => _estimate.ClientValidationDate;

    public string RoleLabel => _perspective == EstimateListPerspective.Provider
        ? "Vous validez en tant que prestataire"
        : "Vous validez en tant que client";

    /// <summary>
    /// Human-readable summary of who has already validated this
    /// estimate, shown above the signature pad.
    /// </summary>
    public string ValidationStateText
    {
        get
        {
            var parts = new List<string>();
            if (ProviderValidationDate != default)
                parts.Add($"Prestataire validé le {ProviderValidationDate.ToLocalTime():g}");
            if (ClientValidationDate != default)
                parts.Add($"Client validé le {ClientValidationDate.ToLocalTime():g}");
            return parts.Count == 0 ? "Devis en attente de validation." : string.Join(" · ", parts);
        }
    }

    /// <summary>
    /// True when the estimate has a linked query to accept/reject.
    /// Estimates without a <see cref="EstimateDto.CommandId"/> are
    /// templates and cannot be validated through the front-office
    /// endpoint.
    /// </summary>
    public bool HasCommand => _estimate.CommandId is not null;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Prêt.";

    [ObservableProperty]
    public partial StatusNotice ActionStatus { get; set; } = StatusNotice.Info("Prêt.");

    [ObservableProperty]
    public partial int StrokeCount { get; set; }

    [ObservableProperty]
    public partial int PointCount { get; set; }

    [ObservableProperty]
    public partial bool HasSignature { get; set; }

    public override bool CanNavigateNext
    {
        get => false;
        protected set { _ = value; }
    }

    public override bool CanNavigatePrevious
    {
        get => true;
        protected set { _ = value; }
    }

    public EstimateValidationPageViewModel(
        EstimateDto estimate,
        EstimateListPerspective perspective,
        FrontOfficeApiClient frontClient)
    {
        _estimate = estimate ?? throw new ArgumentNullException(nameof(estimate));
        _perspective = perspective;
        _frontClient = frontClient ?? throw new ArgumentNullException(nameof(frontClient));
    }

    /// <summary>
    /// Bind the page's signature pad control (the inner control of the
    /// page's <see cref="SignaturePadView"/>). Called from the view's
    /// code-behind once the view is in the visual tree. Also calls
    /// <see cref="LoadExistingSignature"/> so a previously drawn
    /// signature — carried on the estimate payload — is rendered back
    /// into the pad on reopen.
    /// </summary>
    public void Attach(SignaturePadControl pad)
    {
        if (pad is null) throw new ArgumentNullException(nameof(pad));
        Detach();
        _pad = pad;
        _pad.RedrawRequested += OnPadRedraw;
        RefreshFromPad();
        LoadExistingSignature();
    }

    public void Detach()
    {
        if (_pad is null) return;
        _pad.RedrawRequested -= OnPadRedraw;
        _pad = null;
    }

    private void OnPadRedraw(object? sender, EventArgs e) => RefreshFromPad();

    /// <summary>
    /// Render the signature already stored for this side of the
    /// estimate back into the pad. The strokes are carried on the
    /// estimate payload (<see cref="EstimateDto.Signatures"/>); the
    /// side is chosen by the current <see cref="_perspective"/> — the
    /// provider reopens to see their "Pro" signature, the client their
    /// "Client" signature. No network round-trip: the payload is used
    /// as-is, in the same wire format PostIt captured.
    /// </summary>
    public void LoadExistingSignature()
    {
        if (_pad is null) return;
        // SignatureType.Pro = 0, SignatureType.Client = 1 (server enum).
        int wanted = _perspective == EstimateListPerspective.Provider ? 0 : 1;
        var sig = _estimate.Signatures?.FirstOrDefault(s => s.Type == wanted);
        if (sig?.Strokes is { Length: > 0 } strokes)
        {
            _pad.Load(strokes);
            RefreshFromPad();
        }
    }

    private void RefreshFromPad()
    {
        if (_pad is null) return;
        var snap = _pad.Snapshot();
        StrokeCount = snap.StrokeCount;
        PointCount = snap.PointCount;
        HasSignature = !snap.IsEmpty;
    }

    partial void OnIsBusyChanged(bool value)
    {
        ValidateCommand.NotifyCanExecuteChanged();
        RejectCommand.NotifyCanExecuteChanged();
    }

    partial void OnHasSignatureChanged(bool value)
        => ValidateCommand.NotifyCanExecuteChanged();

    private bool CanValidate => !IsBusy && HasSignature && HasCommand;

    private bool CanReject => !IsBusy && HasCommand;

    [RelayCommand(CanExecute = nameof(CanValidate))]
    public async Task ValidateAsync()
    {
        if (_estimate.CommandId is not long queryId)
        {
            this.SetWarningStatus("Ce devis n'est pas lié à une demande et ne peut être validé.");
            return;
        }
        if (_pad is null || _pad.Snapshot().IsEmpty)
        {
            this.SetWarningStatus("Veuillez signer avant de valider.");
            return;
        }

        var snap = _pad.Snapshot();
        IsBusy = true;
        try
        {
            var body = new QueryAcceptanceRequestDto
            {
                Strokes = snap.Strokes,
                CoordinateMax = SignaturePadData.CoordinateMax,
                CapturedAtUtc = DateTime.UtcNow,
            };

            await _frontClient.AcceptQueryAsync(_estimate.CommandType, queryId, body)
                .ConfigureAwait(true);

            this.SetInfoStatus("Devis validé.");
            await GoBackAsync().ConfigureAwait(true);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.Forbidden)
        {
            this.SetWarningStatus("Vous n'êtes pas partie à ce devis.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadRequest)
        {
            this.SetWarningStatus($"Validation refusée: {ex.Message}");
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

    [RelayCommand(CanExecute = nameof(CanReject))]
    public async Task RejectAsync()
    {
        if (_estimate.CommandId is not long queryId)
        {
            this.SetWarningStatus("Ce devis n'est pas lié à une demande et ne peut être refusé.");
            return;
        }

        IsBusy = true;
        try
        {
            await _frontClient.RejectQueryAsync(_estimate.CommandType, queryId)
                .ConfigureAwait(true);

            this.SetInfoStatus("Devis refusé.");
            await GoBackAsync().ConfigureAwait(true);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.Forbidden)
        {
            this.SetWarningStatus("Vous n'êtes pas partie à ce devis.");
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
    public void ClearSignature()
    {
        _pad?.Clear();
        RefreshFromPad();
    }

    [RelayCommand]
    private async Task BackAsync() => await GoBackAsync().ConfigureAwait(true);

    private static async Task GoBackAsync()
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }
        await app.GoBackAsync().ConfigureAwait(true);
    }
}