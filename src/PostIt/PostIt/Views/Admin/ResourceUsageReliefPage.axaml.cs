using Avalonia;
using Avalonia.Controls;
using PostIt.ViewModels;

namespace PostIt.Views;

public partial class ResourceUsageReliefPage : ContentPage
{
    public ResourceUsageReliefPage() => InitializeComponent();

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is ResourceUsageReliefPageViewModel vm) vm.Dispose();
    }
}
