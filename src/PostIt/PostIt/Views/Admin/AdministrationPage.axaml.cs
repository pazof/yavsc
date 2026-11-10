using Avalonia.Controls;
using System;
using System.IO;
using System.Text.Json;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PostIt.ViewModels;
using PostIt.Helpers;
using Yavsc.Abstract.Administration;

namespace PostIt.Views;

public partial class AdministrationPage : ContentPage
{
    public AdministrationPage()
    {
        InitializeComponent();
        DetachedFromVisualTree += (_, _) =>
        {
            if (DataContext is AdministrationPageViewModel vm)
            {
                vm.NewSecretValue = null;
                vm.SecretInput = "";
                vm.ConfirmDestructiveAction = false;
            }
        };
    }

    private async void Export_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AdministrationPageViewModel vm || vm.IsBusy) return;
        vm.IsBusy = true;
        try
        {
            var data = await vm.ExportTransferAsync();
            var content = JsonSerializer.SerializeToUtf8Bytes(data, new JsonSerializerOptions { WriteIndented = true });
            if (!await FileSaveHelpers.SaveAsync("yavsc-business-data.json", "json", content)) return;
            vm.StatusMessage = "Données métier exportées.";
        }
        catch (Exception ex)
        {
            vm.StatusMessage = $"Export impossible : {ex.Message}";
        }
        finally { vm.IsBusy = false; }
    }

    private async void Import_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AdministrationPageViewModel vm || vm.IsBusy) return;
        vm.IsBusy = true;
        try
        {
            if (!vm.ConfirmDestructiveAction) throw new InvalidOperationException("Confirmez l’import avant de choisir un fichier.");
            var topLevel = TopLevel.GetTopLevel(this) ?? throw new InvalidOperationException("Sélecteur de fichiers indisponible.");
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Importer les données métier", AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }]
            });
            if (files.Count == 0) return;
            const int limit = 16 * 1024 * 1024;
            await using var stream = await files[0].OpenReadAsync();
            using var data = new MemoryStream();
            var buffer = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(buffer)) > 0)
            {
                if (data.Length + count > limit) throw new InvalidOperationException("Le fichier dépasse 16 Mo.");
                await data.WriteAsync(buffer.AsMemory(0, count));
            }
            data.Position = 0;
            var transfer = await JsonSerializer.DeserializeAsync<AdministrationTransfer>(data,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Le fichier est vide.");
            var result = await vm.ImportTransferAsync(transfer);
            await vm.RefreshAsync();
            vm.StatusMessage = $"Import effectué : {result.Users} comptes, {result.Blogs} billets, {result.Activities} activités. {vm.StatusMessage}";
        }
        catch (Exception ex)
        {
            vm.StatusMessage = $"Import impossible : {ex.Message}";
        }
        finally
        {
            vm.ConfirmDestructiveAction = false;
            vm.IsBusy = false;
        }
    }
}
