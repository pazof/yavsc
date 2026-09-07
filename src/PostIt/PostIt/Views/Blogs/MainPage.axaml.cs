using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PostIt.Views.Blogs;

public partial class BlogsPage : ContentPage
{
    public BlogsPage()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (DataContext is ViewModels.BlogsViewModel vm)
        {
            if (!vm.IsLoaded)
            {
                vm.RefreshAsync().Wait();
            }
        }
    }
}
