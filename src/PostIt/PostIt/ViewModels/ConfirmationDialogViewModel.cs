using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PostIt.ViewModels;

public partial class ConfirmationDialogViewModel : ViewModelBase
{
    private readonly TaskCompletionSource<bool> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ConfirmationDialogViewModel(string title, string message, string confirmText = "Confirmer", string cancelText = "Annuler")
    {
        Title = title;
        Message = message;
        ConfirmText = confirmText;
        CancelText = cancelText;
    }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; }

    [ObservableProperty]
    public partial string ConfirmText { get; set; }

    [ObservableProperty]
    public partial string CancelText { get; set; }

    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => true; protected set { } }

    public Task<bool> WaitForResultAsync() => _completion.Task;

    [RelayCommand]
    public async Task ConfirmAsync()
    {
        _completion.TrySetResult(true);
        if (App.Current is App app)
            await app.GoBackAsync();
    }

    [RelayCommand]
    public async Task CancelAsync()
    {
        _completion.TrySetResult(false);
        if (App.Current is App app)
            await app.GoBackAsync();
    }
}
