using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Helpers;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

/// <summary>
/// Gestion de l'espace de stockage personnel (« My Files ») :
/// navigation dans l'arbre, téléversement, suppression, téléchargement.
/// Sert aussi de **sélecteur** pour attacher un fichier à un devis
/// (fournisseur) ou à une demande (client) par référence : lorsque
/// <see cref="IsPickerMode"/> est vrai, la sélection d'un fichier invoque
/// <see cref="_onFilePicked"/> avec l'identifiant <see cref="FileEntryDto.Id"/>
/// puis revient à la page précédente.
/// </summary>
public partial class MyFilesViewModel : ViewModelBase, IActionStatusViewModel
{
    private readonly UserFilesApiClient _fs;
    private readonly Action<long>? _onFilePicked;

    /// <summary>
    /// Sous-répertoire courant, relatif à la racine de l'espace perso
    /// (chaîne vide à la racine). Servi tel quel à <c>GET/POST fs/{subdir}</c>.
    /// </summary>
    [ObservableProperty]
    public partial string CurrentPath { get; set; } = string.Empty;

    public string PathLabel => string.IsNullOrWhiteSpace(CurrentPath)
        ? "Mes fichiers"
        : $"Mes fichiers / {CurrentPath}";

    public bool IsPickerMode => _onFilePicked is not null;

    public bool CanGoUp => !string.IsNullOrWhiteSpace(CurrentPath);

    [ObservableProperty]
    public partial ObservableCollection<FileEntryDto> Files { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<DirEntryDto> SubDirectories { get; set; } = new();

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Prêt.";

    [ObservableProperty]
    public partial StatusNotice ActionStatus { get; set; } = StatusNotice.Info("Prêt.");

    public override bool CanNavigateNext { get => false; protected set { _ = value; } }
    public override bool CanNavigatePrevious { get => true; protected set { _ = value; } }

    public MyFilesViewModel(UserFilesApiClient fs, Action<long>? onFilePicked = null)
    {
        _fs = fs ?? throw new ArgumentNullException(nameof(fs));
        _onFilePicked = onFilePicked;
    }

    /// <summary>Constructeur pour le designer Avalonia.</summary>
    public MyFilesViewModel() : this(null!) { }

    public async Task InitializeAsync()
    {
        await RefreshAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var dir = await _fs.ListAsync(CurrentPath).ConfigureAwait(true);
            Files.Clear();
            SubDirectories.Clear();
            if (dir?.Files is not null)
                foreach (var f in dir.Files) Files.Add(f);
            if (dir?.SubDirectories is not null)
                foreach (var d in dir.SubDirectories) SubDirectories.Add(d);
            this.SetInfoStatus($"{Files.Count} fichier(s), {SubDirectories.Count} dossier(s).");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur de listage : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenDirAsync(DirEntryDto? dir)
    {
        if (dir is null) return;
        CurrentPath = CombinePath(CurrentPath, dir.Name ?? string.Empty);
        OnPropertyChanged(nameof(PathLabel));
        OnPropertyChanged(nameof(CanGoUp));
        await RefreshAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task UpAsync()
    {
        if (!CanGoUp) return;
        CurrentPath = ParentPath(CurrentPath);
        OnPropertyChanged(nameof(PathLabel));
        OnPropertyChanged(nameof(CanGoUp));
        await RefreshAsync().ConfigureAwait(true);
    }

    /// <summary>Téléversement déclenché par le code-behind (picker OS).</summary>
    public async Task UploadAsync(System.Collections.Generic.IReadOnlyCollection<UploadFile> uploads)
    {
        if (uploads is null || uploads.Count == 0) return;
        IsBusy = true;
        try
        {
            var received = await _fs.UploadAsync(CurrentPath, uploads).ConfigureAwait(true);
            this.SetInfoStatus($"{received.Count} fichier(s) téléversé(s).");
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Échec du téléversement : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteFileAsync(FileEntryDto? file)
    {
        if (file is null) return;
        IsBusy = true;
        try
        {
            await _fs.DeleteAsync(CombinePath(CurrentPath, file.Name ?? string.Empty)).ConfigureAwait(true);
            this.SetInfoStatus($"Fichier supprimé.");
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Échec de la suppression : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DownloadFileAsync(FileEntryDto? file)
    {
        if (file is null || file.Id is null) return;
        IsBusy = true;
        try
        {
            var bytes = await _fs.DownloadFileAsync(file.Id.Value).ConfigureAwait(true);
            var saved = await FileSaveHelpers.SaveAsync(file.Name ?? "file", "bin", bytes).ConfigureAwait(true);
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

    [RelayCommand]
    private async Task PickFileAsync(FileEntryDto? file)
    {
        if (file is null || file.Id is null)
        {
            this.SetWarningStatus("Ce fichier n'est pas référencé (aucun id). Téléversez-le d'abord.");
            return;
        }

        var callback = _onFilePicked;
        if (callback is null)
        {
            // Mode gestion : le téléchargement est l'action par défaut.
            await DownloadFileAsync(file).ConfigureAwait(true);
            return;
        }

        callback(file.Id.Value);
        await GoBackAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        await GoBackAsync().ConfigureAwait(true);
    }

    private static async Task GoBackAsync()
    {
        var app = (App?)Application.Current;
        if (app is null) throw new InvalidOperationException("Application PostIt indisponible.");
        await app.GoBackAsync().ConfigureAwait(true);
    }

    private static string CombinePath(string current, string name)
    {
        if (string.IsNullOrWhiteSpace(current)) return name;
        return $"{current}/{name}";
    }

    private static string ParentPath(string current)
    {
        if (string.IsNullOrWhiteSpace(current)) return string.Empty;
        var idx = current.LastIndexOf('/');
        return idx < 0 ? string.Empty : current[..idx];
    }
}