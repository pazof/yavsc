using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class CirclesPage : ContentPage
{
    private CirclesPageViewModel? _vm;

    public CirclesPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Unsubscribe from the previous VM to avoid leaking
        // handlers across navigation pushes / DataContext resets.
        if (_vm is not null)
            _vm.AddMemberRequested -= OnAddMemberRequested;

        _vm = DataContext as CirclesPageViewModel;
        if (_vm is not null)
            _vm.AddMemberRequested += OnAddMemberRequested;
    }

    private void OnAddMemberRequested(object? sender, EventArgs e)
    {
        var app = Application.Current as App;
        var services = app?.ServiceProvider;
        if (services is null || _vm is null) return;

        // Resolve the directory via DI. The dialog raises its
        // own Confirmed event; the VM subscribes via the method
        // below — we pass the VM in so the closure can call
        // back into it without the dialog needing to know the
        // type of its caller. EventHandler<UserSummary> wants a
        // void return, so wrap the async VM method in a fire-
        // and-forget helper.
        var directory = services.GetRequiredService<IUserDirectory>();
        var dialog = new AddCircleMemberDialog(directory);
        dialog.ViewModel!.Confirmed += async (sender, picked) =>
            await _vm.OnAddMemberConfirmedAsync(sender, picked);

        if (this.VisualRoot is MainWindow window)
            _ = window.NavRoot.PushAsync(dialog);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
