using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PostIt.ViewModels;

/// <summary>
/// ViewModel de la page « À propos de PostIt ». Page statique :
/// version de l'application, description, licence et liens vers le
/// dépôt Forgejo et son suivi de tickets.
/// </summary>
public partial class AboutPageViewModel : ViewModelBase
{
    public const string RepositoryUrl = "https://forgejo.pschneider.fr/notazof/yavsc";
    public const string IssuesUrl = RepositoryUrl + "/issues";

    public AboutPageViewModel()
    {
        Version = ReadVersion();
    }

    public string Version { get; }

    public string Description { get; } =
        "PostIt est le client de la plateforme Yavsc : blog, devis, " +
        "demandes de prestations et fichiers personnels.";

    public string License { get; } =
        "Licence WTFPL — Do What The Fuck You Want To Public License, version 2";

    public override bool CanNavigateNext
    {
        get => false;
        protected set => throw new NotImplementedException();
    }

    public override bool CanNavigatePrevious
    {
        get => true;
        protected set => throw new NotImplementedException();
    }

    [RelayCommand]
    private Task OpenRepository() => OpenUrlAsync(RepositoryUrl);

    [RelayCommand]
    private Task OpenIssues() => OpenUrlAsync(IssuesUrl);

    private static async Task OpenUrlAsync(string url)
    {
        var app = (App)App.Current!;
        var topLevel = TopLevel.GetTopLevel(app.View);
        if (topLevel is not null)
        {
            await topLevel.Launcher.LaunchUriAsync(new Uri(url)).ConfigureAwait(true);
        }
    }

    private static string ReadVersion()
    {
        var assembly = typeof(AboutPageViewModel).Assembly;
        var info = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (!string.IsNullOrEmpty(info))
        {
            // "1.1.0-estimate.1+364.Branch.x.Sha.y" → "1.1.0-estimate.1"
            var plus = info.IndexOf('+');
            return plus > 0 ? info[..plus] : info;
        }

        return assembly.GetName().Version?.ToString() ?? "inconnue";
    }
}
