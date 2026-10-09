using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using PostIt.Controls;
using PostIt.Views.Blogs;

namespace PostIt.Tests;

public class MarkdownEditorScrollTests
{
    private static readonly string LongArticle =
        string.Join("\n\n", Enumerable.Range(1, 100).Select(i => $"Paragraph {i}: article content."));

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Long_article_scrolls_to_the_end_and_adapts_to_resizing(bool preview)
    {
        var editor = new MarkdownEditorControl
        {
            MarkdownText = LongArticle,
            IsPreviewVisible = preview
        };
        var window = new Window { Width = 600, Height = 400, Content = editor };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var scroller = FindArticleScroller(editor, preview);
            AssertScrollable(scroller);
            var initialViewport = scroller.Viewport.Height;

            window.Height = 300;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.True(scroller.Viewport.Height < initialViewport);
            AssertScrollable(scroller);
        }
        finally
        {
            CloseWindow(window, editor);
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Blog_page_bounds_the_article_viewport(bool preview)
    {
        var page = new BlogsPage();
        var editor = page.GetLogicalDescendants().OfType<MarkdownEditorControl>().Single();
        editor.MarkdownText = LongArticle;
        editor.IsPreviewVisible = preview;
        var posts = page.FindControl<ListBox>("PostsListBox")!;
        posts.ItemsSource = Enumerable.Range(1, 50)
            .Select(i => new Yavsc.Blogspot.BlogPostDto { Title = $"Post {i}" }).ToArray();
        var navigation = new NavigationPage();
        navigation.PushAsync(page).GetAwaiter().GetResult();
        var window = new Window { Width = 800, Height = 700, Content = navigation };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var scroller = FindArticleScroller(editor, preview);
            Assert.InRange(scroller.Viewport.Height, 1, window.ClientSize.Height);
            AssertScrollable(scroller);
            AssertScrollable(posts.GetVisualDescendants().OfType<ScrollViewer>().Single());
            var editorBottom = editor.TranslatePoint(new Point(0, editor.Bounds.Height), window);
            Assert.NotNull(editorBottom);
            Assert.InRange(editorBottom.Value.Y, 1, window.ClientSize.Height);
        }
        finally
        {
            CloseWindow(window, editor);
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Short_article_needs_no_vertical_scrollbar(bool preview)
    {
        var editor = new MarkdownEditorControl
        {
            MarkdownText = "Short article",
            IsPreviewVisible = preview
        };
        var window = new Window { Width = 600, Height = 400, Content = editor };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var scroller = FindArticleScroller(editor, preview);
            Assert.True(scroller.Extent.Height <= scroller.Viewport.Height);
            Assert.DoesNotContain(scroller.GetVisualDescendants().OfType<ScrollBar>(),
                bar => bar.Orientation == Avalonia.Layout.Orientation.Vertical && bar.IsVisible);
        }
        finally
        {
            CloseWindow(window, editor);
        }
    }

    private static void CloseWindow(Window window, MarkdownEditorControl editor)
    {
        // The package's shared renderer caches the visual; detach it between headless applications.
        editor.FindControl<MarkdownViewer.Core.Controls.MarkdownViewer>("PreviewViewer")!.Content = null;
        window.UpdateLayout();
        window.Close();
    }

    private static ScrollViewer FindArticleScroller(MarkdownEditorControl editor, bool preview)
    {
        if (!preview)
            return editor.GetVisualDescendants().OfType<TextEditor>().Single()
                .GetVisualDescendants().OfType<ScrollViewer>().Single();

        return editor.FindControl<ScrollViewer>("PreviewScrollViewer")!;
    }

    private static void AssertScrollable(ScrollViewer scroller)
    {
        Assert.True(scroller.Extent.Height > scroller.Viewport.Height,
            $"Expected overflow, but extent={scroller.Extent.Height}, viewport={scroller.Viewport.Height}.");
        Assert.Contains(scroller.GetVisualDescendants().OfType<ScrollBar>(),
            bar => bar.Orientation == Avalonia.Layout.Orientation.Vertical && bar.IsVisible);
        scroller.ScrollToEnd();
        Assert.True(scroller.Offset.Y > 0);
        Assert.Equal(scroller.Extent.Height - scroller.Viewport.Height, scroller.Offset.Y, 1);
    }
}
