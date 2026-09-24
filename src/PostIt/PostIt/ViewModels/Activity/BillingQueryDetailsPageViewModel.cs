using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Helpers;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class BillingQueryDetailsPageViewModel : ViewModelBase, IActionStatusViewModel
{
    private readonly BillingApiClient _billingClient;
    private readonly FrontOfficeApiClient _frontClient;
    private readonly UserFilesApiClient _fsClient;
    private readonly BillingQueryDetailsDto _details;

    /// <summary>
    /// Vrai lorsque l'utilisateur courant est le client de la demande
    /// et peut y attacher/détacher des fichiers de son espace perso.
    /// Le fournisseur consulte et télécharge les pièces mais n'attache
    /// pas (il joint ses documents au devis, côté <c>EstimateEditionPage</c>).
    /// </summary>
    public bool CanAttach { get; }

    public ActivityInfo Activity { get; }
    public ActivityUserDisplayItem Performer { get; }
    public CommandFormSummary Form { get; }
    public bool IsReadOnly { get; }

    public long Id => _details.Id;
    public string Title => $"Detail commande #{_details.Id}";
    public string ContextLabel => $"{Performer.UserName} · {Activity.Name} · {Form.Title}";
    public string StatusLabel => _details.Status.ToString();
    public string StatusGlyph => GetStatusGlyph(_details.Status);
    public string StatusBadgeBackground => GetStatusBadgeBackground(_details.Status);
    public string StatusBadgeBorder => GetStatusBadgeBorder(_details.Status);
    public string StatusBadgeForeground => GetStatusBadgeForeground(_details.Status);
    public string TitleForeground => StatusBadgeForeground;
    public string BillingCode => _details.BillingCode;
    public string Description => EmptyAsPlaceholder(_details.Description, "(sans description)");
    public string Reason => EmptyAsPlaceholder(_details.Reason, "(aucun motif)");
    public string AdditionalInfo => EmptyAsPlaceholder(_details.AdditionalInfo, "(aucune info complementaire)");
    public string ClientId => EmptyAsPlaceholder(_details.ClientId, "(non renseigne)");
    public string EventDateLabel => _details.EventDate?.ToLocalTime().ToString("f") ?? "Date non precisee";
    public string ConsentLabel => _details.Consent ? "Oui" : "Non";
    public string ProvisionalLabel => _details.Provisional.HasValue ? _details.Provisional.Value.ToString("0.00") : "(non renseigne)";
    public string LocationLabel => BuildLocationLabel(_details.Location);
    public string PrestationsLabel => BuildPrestationsLabel(_details);
    public bool CanEdit => !IsReadOnly;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Pret.";

    [ObservableProperty]
    public partial StatusNotice ActionStatus { get; set; } = StatusNotice.Info("Pret.");

    /// <summary>Pièces jointes rattachées à la demande (par référence).</summary>
    [ObservableProperty]
    public partial ObservableCollection<AttachmentDto> Attachments { get; set; } = new();

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

    public BillingQueryDetailsPageViewModel(
        ActivityInfo activity,
        ActivityUserDisplayItem performer,
        CommandFormSummary form,
        BillingApiClient billingClient,
        FrontOfficeApiClient frontClient,
        UserFilesApiClient fsClient,
        BillingQueryDetailsDto details,
        bool isReadOnly,
        bool canAttach)
    {
        Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Performer = performer ?? throw new ArgumentNullException(nameof(performer));
        Form = form ?? throw new ArgumentNullException(nameof(form));
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));
        _frontClient = frontClient ?? throw new ArgumentNullException(nameof(frontClient));
        _fsClient = fsClient ?? throw new ArgumentNullException(nameof(fsClient));
        _details = details ?? throw new ArgumentNullException(nameof(details));
        IsReadOnly = isReadOnly;
        CanAttach = canAttach;

        this.SetInfoStatus("Details de commande charges.");
    }

    /// <summary>Constructeur pour le designer Avalonia.</summary>
    public BillingQueryDetailsPageViewModel() : this(null!, null!, null!, null!, null!, null!, null!, false, false) { }

    [RelayCommand]
    private async Task OpenEditorAsync()
    {
        if (IsReadOnly)
        {
            this.SetWarningStatus("Mode lecture seule: edition desactivee.");
            return;
        }

        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        IsBusy = true;
        try
        {
            var vm = Form.CreateCommandPageViewModel(Activity, Performer, _billingClient);
            if (vm is null)
            {
                this.SetWarningStatus("Ce formulaire n'est pas encore pris en charge en edition.");
                return;
            }

            await vm.InitializeAsync(_details).ConfigureAwait(true);
            await app.PushPageAsync(vm).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur lors de l'ouverture en edition: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        await app.GoBackAsync().ConfigureAwait(true);
    }

    /// <summary>Charge les pièces jointes de la demande.</summary>
    public async Task InitializeAsync()
    {
        await LoadQueryAttachmentsAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoadQueryAttachmentsAsync()
    {
        try
        {
            var list = await _frontClient
                .GetQueryAttachmentsAsync(BillingCode, Id)
                .ConfigureAwait(true);
            Attachments.Clear();
            if (list is not null)
                foreach (var a in list) Attachments.Add(a);
        }
        catch (Exception ex)
        {
            // Non bloquant : les pièces jointes sont un complément.
            this.SetWarningStatus($"Pièces jointes indisponibles : {ex.Message}");
        }
    }

    /// <summary>
    /// Ouvre la page « My Files » en mode sélecteur ; le fichier choisi
    /// est rattaché à la demande par référence via
    /// <c>POST front/query/{id}/attachments</c>. Réservé au client
    /// (<see cref="CanAttach"/>).
    /// </summary>
    [RelayCommand]
    private async Task AddQueryAttachmentAsync()
    {
        if (!CanAttach) return;

        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var billingCode = BillingCode;
        var queryId = Id;
        var picker = new MyFilesViewModel(_fsClient, async fileId =>
        {
            try
            {
                await _frontClient.AttachQueryFileAsync(billingCode, queryId, fileId).ConfigureAwait(true);
                await LoadQueryAttachmentsAsync().ConfigureAwait(true);
                this.SetInfoStatus("Pièce jointe ajoutée à la demande.");
            }
            catch (Exception ex)
            {
                this.SetErrorStatus($"Échec de l'attachement : {ex.Message}");
            }
        });
        await picker.InitializeAsync().ConfigureAwait(true);
        await app.PushPageAsync(picker).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task DetachQueryFileAsync(AttachmentDto? attachment)
    {
        if (attachment is null || !CanAttach) return;
        IsBusy = true;
        try
        {
            await _frontClient.DetachQueryFileAsync(BillingCode, Id, attachment.FileId).ConfigureAwait(true);
            Attachments.Remove(attachment);
            this.SetInfoStatus("Pièce jointe détachée de la demande.");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Échec du détachement : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DownloadQueryAttachmentAsync(AttachmentDto? attachment)
    {
        if (attachment is null) return;
        IsBusy = true;
        try
        {
            var bytes = await _fsClient.DownloadFileAsync(attachment.FileId).ConfigureAwait(true);
            var name = System.IO.Path.GetFileName(attachment.Path ?? "file");
            var saved = await FileSaveHelpers.SaveAsync(name, "bin", bytes).ConfigureAwait(true);
            this.SetInfoStatus(saved ? "Fichier enregistré." : "Téléchargement annulé.");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Échec du téléchargement : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Download the estimate linked to this command as a LaTeX source
    /// (<c>GET api/v1/front/query/{Id}/estimate.tex</c>). The estimate is
    /// resolved server-side from <c>Estimate.CommandId == query Id</c>.
    /// </summary>
    [RelayCommand]
    public async Task DownloadTexAsync()
    {
        await DownloadAsync(
            () => _frontClient.GetEstimateTexAsync(_details.Id),
            $"devis-{_details.Id}", "tex").ConfigureAwait(true);
    }

    /// <summary>
    /// Download the estimate linked to this command as a compiled PDF
    /// (<c>GET api/v1/front/query/{Id}/estimate.pdf</c>). Requires
    /// <c>texi2pdf</c> on the server host.
    /// </summary>
    [RelayCommand]
    public async Task DownloadPdfAsync()
    {
        await DownloadAsync(
            () => _frontClient.GetEstimatePdfAsync(_details.Id),
            $"devis-{_details.Id}", "pdf").ConfigureAwait(true);
    }

    private async Task DownloadAsync(Func<Task<byte[]>> fetch, string fileName, string ext)
    {
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

    private static string EmptyAsPlaceholder(string? value, string placeholder)
        => string.IsNullOrWhiteSpace(value) ? placeholder : value;

    private static string BuildLocationLabel(BillingLocationDto? location)
    {
        if (location is null)
        {
            return "(non renseignee)";
        }

        var text = EmptyAsPlaceholder(location.Address, "adresse vide");
        if (location.Latitude.HasValue && location.Longitude.HasValue)
        {
            text += $" ({location.Latitude.Value:0.####}, {location.Longitude.Value:0.####})";
        }

        return text;
    }

    private static string BuildPrestationsLabel(BillingQueryDetailsDto details)
    {
        if (details.PrestationIds.Count > 0)
        {
            return string.Join(", ", details.PrestationIds.Select(static id => id.ToString()));
        }

        return details.PrestationId.HasValue
            ? details.PrestationId.Value.ToString()
            : "(aucune)";
    }

    private static string GetStatusBadgeBackground(QueryStatus status)
        => status switch
        {
            QueryStatus.Accepted => "#E6F7EC",
            QueryStatus.ProAccepted => "#E6F7EC",
            QueryStatus.ClientAccepted => "#E1F5FE",
            QueryStatus.InProgress => "#FFF4D6",
            QueryStatus.Rejected => "#FDECEA",
            QueryStatus.Failed => "#ECEFF1",
            QueryStatus.Success => "#E8F8EF",
            _ => "#EAF3FF",
        };

    private static string GetStatusBadgeBorder(QueryStatus status)
        => status switch
        {
            QueryStatus.Accepted => "#2E7D32",
            QueryStatus.ProAccepted => "#2E7D32",
            QueryStatus.ClientAccepted => "#1565C0",
            QueryStatus.InProgress => "#B26A00",
            QueryStatus.Rejected => "#C62828",
            QueryStatus.Failed => "#607D8B",
            QueryStatus.Success => "#1E8E3E",
            _ => "#2A5EA8",
        };

    private static string GetStatusBadgeForeground(QueryStatus status)
        => status switch
        {
            QueryStatus.Accepted => "#1B5E20",
            QueryStatus.ProAccepted => "#1B5E20",
            QueryStatus.ClientAccepted => "#0D47A1",
            QueryStatus.InProgress => "#7A4A00",
            QueryStatus.Rejected => "#8E0000",
            QueryStatus.Failed => "#37474F",
            QueryStatus.Success => "#145A2A",
            _ => "#1A4178",
        };

    private static string GetStatusGlyph(QueryStatus status)
        => status switch
        {
            QueryStatus.Accepted => "OK",
            QueryStatus.ProAccepted => "P",
            QueryStatus.ClientAccepted => "C",
            QueryStatus.InProgress => "~",
            QueryStatus.Rejected => "!",
            QueryStatus.Failed => "X",
            QueryStatus.Success => "V",
            _ => "i",
        };
}