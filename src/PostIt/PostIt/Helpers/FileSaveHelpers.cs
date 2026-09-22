using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace PostIt.Helpers;

/// <summary>
/// Saves a downloaded byte payload to a file chosen by the user through
/// the platform save picker. This is the first file-save (as opposed to
/// file-open) helper in PostIt; the open picker is already used for blog
/// attachment and avatar uploads
/// (<see cref="Views.Blogs.BlogsPage"/>, <see cref="Views.Layout.SettingsPage"/>).
///
/// <para>The <c>TopLevel</c> hosting the picker is resolved from the
/// application's root <c>MainView</c> (<c>App.View</c>), which works for
/// both the desktop and single-view lifetimes.</para>
/// </summary>
public static class FileSaveHelpers
{
    /// <summary>
    /// Prompt the user to choose a destination and write
    /// <paramref name="content"/> to it. Returns false when the user
    /// cancels the picker; throws on IO/platform errors so the caller
    /// can surface them.
    /// </summary>
    public static async Task<bool> SaveAsync(
        string suggestedName,
        string defaultExtension,
        byte[] content,
        CancellationToken ct = default)
    {
        var view = (Application.Current as App)?.View;
        var topLevel = view is null ? null : TopLevel.GetTopLevel(view);
        if (topLevel is null)
            throw new InvalidOperationException("Aucune fenêtre active pour enregistrer le fichier.");

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Enregistrer sous",
            DefaultExtension = defaultExtension,
            SuggestedFileName = suggestedName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType($"{defaultExtension.ToUpper()} file")
                {
                    Patterns = new[] { $"*.{defaultExtension}" }
                }
            }
        }).ConfigureAwait(true);

        if (file is null)
            return false; // user cancelled

        await using var stream = await file.OpenWriteAsync().ConfigureAwait(true);
        await stream.WriteAsync(content, ct).ConfigureAwait(true);
        return true;
    }
}