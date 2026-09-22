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
using Yavsc.Models.Billing;

namespace PostIt.ViewModels;

/// <summary>
/// Validation (accept / reject) of a single estimate, with optional
/// signatures. Backs the estimate validation page opened from
/// <see cref="EstimateListPageViewModel"/> for either perspective.
///
/// <para>Two signatures are shown, each in its own pad: the provider's
/// (Pro) and the client's (Client). Both are rendered from the estimate
/// payload when present, so reopening a partly-signed estimate shows
/// what was already captured. Only the pad matching the current
/// <see cref="EstimateListPerspective"/> is writable — the server
/// (<c>POST api/v1/front/query/accept</c>) independently enforces the
/// same rule (it resolves the caller's role from the authenticated user
/// vs the query's <c>PerformerId</c>/<c>ClientId</c>), and the UI marks
/// the other pad read-only. The billing code sent to the server is the
/// estimate's <see cref="EstimateDto.CommandType"/> and the query id is
/// its <see cref="EstimateDto.CommandId"/>.</para>
/// </summary>
public partial class EstimateValidationPageViewModel : ViewModelBase, IActionStatusViewModel
{
    private readonly EstimateDto _estimate;
    private readonly EstimateListPerspective _perspective;
    private readonly FrontOfficeApiClient _frontClient;
    private SignaturePadControl? _proPad;
    private SignaturePadControl? _clientPad;

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
    /// True when the current user may sign the provider's signature
    /// (i.e. they opened the list as the provider). The other party's
    /// pad is read-only.
    /// </summary>
    public bool CanWritePro => _perspective == EstimateListPerspective.Provider;

    /// <summary>
    /// True when the current user may sign the client's signature
    /// (i.e. they opened the list as the client).
    /// </summary>
    public bool CanWriteClient => _perspective == EstimateListPerspective.Client;

    /// <summary>
    /// Human-readable summary of who has already validated this
    /// estimate, shown above the signature pads.
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

    /// <summary>Stroke/point counts for the writable pad.</summary>
    [ObservableProperty]
    public partial int StrokeCount { get; set; }

    [ObservableProperty]
    public partial int PointCount { get; set; }

    /// <summary>True when the writable pad holds a signature.</summary>
    [ObservableProperty]
    public partial bool HasSignature { get; set; }

    [ObservableProperty]
    public partial bool HasProSignature { get; set; }

    [ObservableProperty]
    public partial bool HasClientSignature { get; set; }

    [ObservableProperty]
    public partial string ProSignatureHint { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ClientSignatureHint { get; set; } = string.Empty;

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
    /// Bind the page's two signature pads (the inner controls of the
    /// page's <see cref="SignaturePadView"/>s): the provider's pad and
    /// the client's pad. Called from the view's code-behind once the
    /// view is in the visual tree. Marks the non-author pad read-only,
    /// then renders any signatures already carried on the estimate
    /// payload.
    /// </summary>
    public void Attach(SignaturePadControl proPad, SignaturePadControl clientPad)
    {
        if (proPad is null) throw new ArgumentNullException(nameof(proPad));
        if (clientPad is null) throw new ArgumentNullException(nameof(clientPad));
        Detach();
        _proPad = proPad;
        _clientPad = clientPad;
        _proPad.IsReadOnly = !CanWritePro;
        _clientPad.IsReadOnly = !CanWriteClient;
        _proPad.RedrawRequested += OnPadRedraw;
        _clientPad.RedrawRequested += OnPadRedraw;
        LoadExistingSignatures();
        RefreshFromPad();
    }

    public void Detach()
    {
        if (_proPad is not null)
            _proPad.RedrawRequested -= OnPadRedraw;
        if (_clientPad is not null)
            _clientPad.RedrawRequested -= OnPadRedraw;
        _proPad = null;
        _clientPad = null;
    }

    private void OnPadRedraw(object? sender, EventArgs e) => RefreshFromPad();

    /// <summary>The pad the current user may sign in.</summary>
    private SignaturePadControl? WritablePad => CanWritePro ? _proPad : _clientPad;

    /// <summary>
    /// Render the signatures already stored for this estimate back into
    /// their pads. The strokes are carried on the estimate payload as
    /// two dedicated slots — <see cref="EstimateDto.SignaturePro"/> and
    /// <see cref="EstimateDto.SignatureClient"/> — so each pad loads
    /// its own side directly, in the same wire format PostIt captured.
    /// No network round-trip.
    /// </summary>
    public void LoadExistingSignatures()
    {
        LoadSide(_proPad, _estimate.SignaturePro);
        LoadSide(_clientPad, _estimate.SignatureClient);
        RefreshFromPad();
    }

    private static void LoadSide(SignaturePadControl? pad, EstimateSignatureDto? sig)
    {
        if (pad is null || sig is null) return;
        if (sig.Strokes is { Length: > 0 } strokes)
            pad.Load(strokes);
    }

    private void RefreshFromPad()
    {
        HasProSignature = _proPad?.Snapshot().IsEmpty == false;
        HasClientSignature = _clientPad?.Snapshot().IsEmpty == false;

        ProSignatureHint = CanWritePro
            ? (HasProSignature ? "Signé — dessinez pour remplacer" : "Signez ici en tant que prestataire")
            : (HasProSignature ? "Signature du prestataire (lecture seule)" : "En attente de signature du prestataire");
        ClientSignatureHint = CanWriteClient
            ? (HasClientSignature ? "Signé — dessinez pour remplacer" : "Signez ici en tant que client")
            : (HasClientSignature ? "Signature du client (lecture seule)" : "En attente de signature du client");

        var pad = WritablePad;
        if (pad is null) return;
        var snap = pad.Snapshot();
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
        var pad = WritablePad;
        if (pad is null || pad.Snapshot().IsEmpty)
        {
            this.SetWarningStatus("Veuillez signer avant de valider.");
            return;
        }

        var snap = pad.Snapshot();
        IsBusy = true;
        try
        {
            var body = new QueryAcceptanceRequestDto
            {
                Strokes = snap.Strokes,
                CoordinateMax = SignaturePadData.CoordinateMax,
                CapturedAtUtc = DateTime.UtcNow,
                // Declare which side the caller is signing as, so the
                // server stores the signature under the right author
                // (Pro/Client) even when one user is both parties.
                SignatureType = CanWritePro ? SignatureType.Pro : SignatureType.Client,
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
        WritablePad?.Clear();
        RefreshFromPad();
    }

    /// <summary>
    /// Download the estimate as a LaTeX source document. The server
    /// renders the Estimate_tex template to a <c>.tex</c> byte stream
    /// (<c>GET api/v1/front/query/{CommandId}/estimate.tex</c>); the
    /// user picks a destination via the platform save picker.
    /// </summary>
    [RelayCommand]
    public async Task DownloadTexAsync()
    {
        await DownloadAsync(
            () => _frontClient.GetEstimateTexAsync(_estimate.CommandId!.Value),
            $"devis-{EstimateId}", "tex").ConfigureAwait(true);
    }

    /// <summary>
    /// Download the estimate as a compiled PDF
    /// (<c>GET api/v1/front/query/{CommandId}/estimate.pdf</c>).
    /// Requires <c>texi2pdf</c> on the server host; a failure is
    /// surfaced as a status message.
    /// </summary>
    [RelayCommand]
    public async Task DownloadPdfAsync()
    {
        await DownloadAsync(
            () => _frontClient.GetEstimatePdfAsync(_estimate.CommandId!.Value),
            $"devis-{EstimateId}", "pdf").ConfigureAwait(true);
    }

    private async Task DownloadAsync(Func<Task<byte[]>> fetch, string fileName, string ext)
    {
        if (_estimate.CommandId is not long)
        {
            this.SetWarningStatus("Ce devis n'est pas lié à une demande.");
            return;
        }

        IsBusy = true;
        try
        {
            var bytes = await fetch().ConfigureAwait(true);
            var saved = await FileSaveHelpers.SaveAsync(fileName, ext, bytes).ConfigureAwait(true);
            this.SetInfoStatus(saved
                ? $"Fichier .{ext} enregistré."
                : "Téléchargement annulé.");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Échec du téléchargement: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
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