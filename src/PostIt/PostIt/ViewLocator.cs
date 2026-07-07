using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using PostIt.ViewModels;
using PostIt.Views;

namespace PostIt;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>

public class ViewLocator : IDataTemplate
{
     private readonly IServiceProvider _services;

    public ViewLocator(IServiceProvider services)
    {
        _services = services;
    }

    public Control Build(object? data)
    {
        return data switch
        {
            MainPageViewModel => _services.GetRequiredService<MainPage>(),
            SettingsPageViewModel => _services.GetRequiredService<SettingsPage>(),
            HomePageViewModel => _services.GetRequiredService<HomePage>(),
            SignaturePageViewModel => _services.GetRequiredService<SignaturePage>(),
            null => new TextBlock { Text = "No view for <null>" },
        _ => new TextBlock { Text = $"No view for {data.GetType().Name}" }
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
