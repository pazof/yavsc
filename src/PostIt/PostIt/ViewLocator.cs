using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using PostIt.ViewModels;
using PostIt.Views;

namespace PostIt;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{

     public Control Build(object? data)
    {
        try
        {
            return BuildCore(data);
        }
        catch (Exception ex)
        {
            return new TextBlock { Text = $"ViewLocator threw: {ex}" };
        }
    }


    private Control BuildCore(object? data)
    {
        var app = App.Current as App;
        var services = app!.ServiceProvider!;
        return data switch
        {
            MainViewModel => services.GetRequiredService<MainPage>(),
            Settings => services.GetRequiredService<SettingsPage>(),
            HomePageViewModel => services.GetRequiredService<HomePage>(),
            SignaturePageViewModel => services.GetRequiredService<SignaturePage>(),
            AddCircleMemberDialogViewModel => services.GetRequiredService<AddCircleMemberDialog>(),
            CirclesPageViewModel => services.GetRequiredService<CirclesPage>(),
            PostAclDialogViewModel => services.GetRequiredService<PostAclDialog>(),
            null => new TextBlock { Text = "No view for <null>" },
            _ => new TextBlock { Text = $"No view for {data.GetType().Name}" }
        };
    }

     public bool Match(object? data) => data is ViewModelBase;
}
