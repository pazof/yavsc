using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using PostIt.ViewModels;
using PostIt.ViewModels.Commands;
using PostIt.Views;
using PostIt.Views.Blogs;
using PostIt.Views.Commands;

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
            BlogsViewModel => services.GetRequiredService<BlogsPage>(),
            Settings => services.GetRequiredService<SettingsPage>(),
            HomePageViewModel => services.GetRequiredService<HomePage>(),
            ActivitiesPageViewModel => services.GetRequiredService<ActivitiesPage>(),
            CommandFormsPageViewModel => services.GetRequiredService<CommandFormsPage>(),
            BrushViewModel => services.GetRequiredService<BrushPage>(),
            RdvViewModel => services.GetRequiredService<RdvPage>(),
            SignaturePageViewModel => services.GetRequiredService<SignaturePage>(),
            AddCircleMemberDialogViewModel => services.GetRequiredService<AddCircleMemberDialog>(),
            CirclesPageViewModel => services.GetRequiredService<CirclesPage>(),
            PostAclDialogViewModel => services.GetRequiredService<PostAclDialog>(),
            BillingQueriesPageViewModel => services.GetRequiredService<BillingQueriesPage>(),
            BillingQueryDetailsPageViewModel => services.GetRequiredService<BillingQueryDetailsPage>(),
            ProviderOngoingRequestsPageViewModel => services.GetRequiredService<ProviderOngoingRequestsPage>(),
            null => new TextBlock { Text = "No view for <null>" },
            _ => new TextBlock { Text = $"No view for {data.GetType().Name}" }
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
