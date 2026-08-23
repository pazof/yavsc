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
    private readonly IServiceProvider _services;

    public ViewLocator(IServiceProvider services)
    {
        _services = services;
    }

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
        return data switch
        {
            MainViewModel => _services.GetRequiredService<MainPage>(),
            Settings => _services.GetRequiredService<SettingsPage>(),
            HomePageViewModel => _services.GetRequiredService<HomePage>(),
            SignaturePageViewModel => _services.GetRequiredService<SignaturePage>(),
            AddCircleMemberDialogViewModel => _services.GetRequiredService<AddCircleMemberDialog>(),
            CirclesPageViewModel => _services.GetRequiredService<CirclesPage>(),
            PostAclDialogViewModel => _services.GetRequiredService<PostAclDialog>(),
            null => new TextBlock { Text = "No view for <null>" },
            _ => new TextBlock { Text = $"No view for {data.GetType().Name}" }
        };
    }

     public bool Match(object? data) => data is ViewModelBase;
}
