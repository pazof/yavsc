using Avalonia.Controls;
using PostIt.Controls;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class SignaturePage : ContentPage
{
    private SignaturePageViewModel? _vm;

    public SignaturePage()
    {
        InitializeComponent();

        // SignaturePadView owns the capture area wiring and the ink
        // repaint; the page only forwards the view to the VM so it can
        // read snapshots / subscribe to stroke events.
        DataContextChanged += (_, _) => RebindViewModel(DataContext as SignaturePageViewModel);
    }

    private void RebindViewModel(SignaturePageViewModel? vm)
    {
        if (_vm is not null)
        {
            _vm.Detach();
        }

        _vm = vm;

        if (_vm is null || Pad is null) return;

        _vm.Attach(Pad.SignaturePad);
    }
}