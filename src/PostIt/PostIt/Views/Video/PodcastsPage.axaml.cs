using Avalonia.Controls;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class PodcastsPage : ContentPage
{
    public PodcastsPage() => InitializeComponent();

    private void PodcastSelectionChanged(object? sender, SelectionChangedEventArgs e) => ConfirmDelete.IsChecked = false;

    protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (DataContext is PodcastsPageViewModel vm && !vm.IsBroadcasting)
            _ = vm.ActivateAsync();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs e)
    {
        if (DataContext is PodcastsPageViewModel vm)
            _ = vm.DeactivateAsync();
        base.OnNavigatedFrom(e);
    }
}
