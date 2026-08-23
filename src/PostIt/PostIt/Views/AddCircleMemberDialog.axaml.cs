
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using PostIt.Services;
using PostIt.ViewModels;
using System.Threading.Tasks;

namespace PostIt.Views;

/// <summary>
/// Modal "add a member to a circle" page. Hosted by
/// <c>CirclesPage</c>; the caller passes the resolved
/// <see cref="IUserDirectory"/> via the constructor.
///
/// <para>The dialog raises <c>Confirmed</c> on its ViewModel
/// when the user picks a result and clicks "Ajouter"; the
/// hosting page subscribes to that event and calls
/// <c>CircleApiClient.AddMemberAsync</c> with the target
/// circle id. The dialog itself does not know the circle id
/// by design.</para>
/// </summary>
public partial class AddCircleMemberDialog : Avalonia.Controls.ContentPage
{
    public AddCircleMemberDialog()
    {
        InitializeComponent();

    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Subscribe a handler to be notified when the user
    /// confirms a selection. Returns the underlying VM so
    /// the caller can also drive further state (clear the
    /// selection, close the dialog, refresh its own list).
    /// </summary>
    public AddCircleMemberDialogViewModel? ViewModel
        => DataContext as AddCircleMemberDialogViewModel;

    private async Task OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        App app = App.Current! as App;
        await app!.GoBackAsync();
    }
}
