using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using PostIt.ViewModels;

namespace PostIt.Helpers;

public static class ViewModelBaseHelpers
{
    public static async Task PushPageAsync(this App app, ViewModelBase vm)
    {
        var window = app.View;
        if (window is null)
        {
            throw new InvalidOperationException("MainWindow is not initialized yet.");
        }

        var template = app.DataTemplates.FirstOrDefault(t => t.Match(vm));
        if (template is null)
        {
            throw new InvalidOperationException($"No IDataTemplate found for {vm.GetType().Name}.");
        }

        var view = template.Build(vm);
        if (view is null)
        {
            throw new InvalidOperationException(
                $"Template for {vm.GetType().Name} returned <null>.");
        }

        var page = view as Page;
        if (page is null)
        {
            // NavigationPage expects Page instances. Wrap any fallback control
            // (e.g. ViewLocator error TextBlock) into a ContentPage so it can render.
            page = new ContentPage { Content = view };
        }

        page.DataContext = vm;

        // Avoid stacking the same singleton page twice (e.g. SettingsPage).
        var stack = window.NavRoot.NavigationStack;
        if (stack.Count > 0 && ReferenceEquals(stack[stack.Count - 1], page))
        {
            return;
        }

        await window.NavRoot.PushAsync(page);
    }
}
