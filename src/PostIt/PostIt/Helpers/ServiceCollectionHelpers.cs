using System;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Api.Client;

namespace PostIt.Helpers;

public static class ServiceCollectionHelpers
{
    public static IServiceProvider BuildServices(this ServiceCollection services)
    {
        var settings = new Settings();
        settings.Load();

        var tokenStore = new TokenStore(System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "PostIt", "tokens.json"));

        var api = new YavscApiClient(settings, tokenStore);
        var client = new BlogApiClient(api, settings.BlogsApiUrl);
        var circleClient = new CircleApiClient(api, settings.BlogsApiUrl);
        var blogAclClient = new BlogAclApiClient(api, settings.BlogsApiUrl);
        var userSearchClient = new UserSearchClient(api, settings.BlogsApiUrl);
        var contactService = new ContactService();
        var userDirectory = new UserDirectory(userSearchClient);

        // Vues
        services.AddTransient<MainPage>();
        // SettingsPage is a singleton: there must be one and only one
        // instance of the settings UI for the lifetime of the app.
        // This guarantees that (a) the bindings always reflect the
        // current in-memory Settings state, (b) the page already has
        // its DataContext wired up at composition-root time (see
        // below), and (c) PushPageAsync's anti-empilement guard sees
        // the same instance across pushes, so a second Settings tap
        // is a no-op rather than re-pushing the page. Transient would
        // let the user accumulate stale SettingsPage instances on
        // the navigation stack, each bound to a fresh
        // SettingsViewModel and missing any in-flight edits.
        services.AddSingleton<SettingsPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<SignaturePage>();
        services.AddTransient<CirclesPage>();
        // Dialogs (modal-light pages): the ViewLocator resolves
        // them when a caller pushes a PostAclDialogViewModel or
        // AddCircleMemberDialogViewModel via App.PushPageAsync.
        // App.PushPageAsync overwrites the page's DataContext with
        // the caller-built VM, so the parameterless ctor is enough
        // here — the parametrised ctors stay for direct test wiring.
        services.AddTransient<PostAclDialog>();
        services.AddTransient<AddCircleMemberDialog>();
        // ViewModels
        services.AddSingleton(settings);
        services.AddSingleton<YavscApiClient>(api);
        services.AddSingleton(client);
        services.AddSingleton(circleClient);
        services.AddSingleton(blogAclClient);
        services.AddSingleton(userSearchClient);
        services.AddSingleton<IContactService>(contactService);
        services.AddSingleton<IUserDirectory>(userDirectory);
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomePageViewModel>();
        services.AddTransient<SignaturePageViewModel>();
        services.AddTransient<CirclesPageViewModel>();

        // Persistent session banner: one instance for the lifetime of
        // the app so the same VM survives page navigation.
        var sessionStatus = new SessionStatusViewModel { Api = api };
        sessionStatus.Refresh();
        services.AddSingleton(sessionStatus);
        services.AddTransient<SessionStatusBanner>();

        return services.BuildServiceProvider();
    }
}
