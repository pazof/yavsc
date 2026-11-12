using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using PostIt.Controls;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using PostIt.Views.Blogs;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
public class MarkdownEditorScrollTests
{
    private static readonly string LongArticle =
        string.Join("\n\n", Enumerable.Range(1, 100).Select(i => $"Paragraph {i}: article content."));

    [AvaloniaFact]
    public async Task Landscape_keyboard_preserves_controls_and_restores_editor_layout()
    {
        var root = new MainView();
        var page = new BlogsPage { DataContext = new BlogsViewModel { IsEditing = true, DraftArticle = LongArticle } };
        var window = new Window { Width = 800, Height = 400, Content = root };
        try
        {
            window.Show();
            await root.NavRoot.PushAsync(page, null);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var editor = page.ArticleEditor;
            var source = editor.FindControl<TextEditor>("SourceEditor")!;
            source.TextArea.Focus();
            source.CaretOffset = source.Document.TextLength;
            root.ApplyKeyboardOcclusion(new Rect(0, 200, 800, 200));
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.True(page.DraftTitleTextBox.IsVisible);
            Assert.True(page.AttachmentsBorder.IsVisible);
            Assert.True(root.SessionBanner.IsVisible);
            Assert.Equal(1, Grid.GetColumn(page.DraftTitleTextBox));
            Assert.Equal(1, Grid.GetColumn(page.AttachmentsBorder));
            Assert.True(page.StatusBar.IsVisible);
            Assert.Equal(2, Grid.GetColumn(page.StatusBar));
            Assert.Same(page.BlogsCommandBar, NavigationPage.GetTopCommandBar(page));
            Assert.True(source.TextArea.Bounds.Height >= source.TextArea.TextView.DefaultLineHeight * 3);
            var bottom = source.TranslatePoint(new Point(0, source.Bounds.Height), window)!.Value.Y;
            Assert.InRange(bottom, 1, 200);
            var document = source.Document;
            root.ApplyKeyboardOcclusion(null);
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.True(page.DraftTitleTextBox.IsVisible);
            Assert.True(page.AttachmentsBorder.IsVisible);
            Assert.True(root.SessionBanner.IsVisible);
            Assert.Equal(0, Grid.GetColumn(page.DraftTitleTextBox));
            Assert.Same(document, source.Document);
            Assert.True(source.TextArea.IsFocused);

            window.Height = 200;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            root.ApplyKeyboardOcclusion(new Rect(0, 200, 800, 200));
            Assert.Equal(0, root.Padding.Bottom);
        }
        finally
        {
            CloseWindow(window, page.ArticleEditor);
        }
    }

    [AvaloniaFact]
    public void Keyboard_padding_preserves_system_bar_insets_and_restores_them()
    {
        var root = new MainView();
        var window = new Window { Width = 600, Height = 800, Content = root };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var safeArea = new Thickness(10, 24, 12, 30);
            root.ApplySafeArea(safeArea);
            Assert.Equal(safeArea, root.Padding);
            root.ApplyKeyboardOcclusion(new Rect(0, 500, 600, 300));
            Assert.Equal(new Thickness(10, 24, 12, 300), root.Padding);
            root.ApplySafeArea(new Thickness(20, 26, 22, 32));
            Assert.Equal(new Thickness(20, 26, 22, 300), root.Padding);
            root.ApplyKeyboardOcclusion(null);
            window.UpdateLayout();
            Assert.Equal(new Thickness(20, 26, 22, 32), root.Padding);
            var navigationTop = root.NavRoot.TranslatePoint(new Point(), window)!.Value.Y;
            var bannerBottom = root.SessionBanner.TranslatePoint(new Point(0, root.SessionBanner.Bounds.Height), window)!.Value.Y;
            Assert.True(navigationTop >= 26);
            Assert.True(bannerBottom <= 768);
            Assert.True(root.SessionBanner.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Resizing_keeps_the_focused_caret_visible_without_changing_selection(bool focused)
    {
        var editor = new MarkdownEditorControl { MarkdownText = LongArticle };
        var other = new TextBox();
        var panel = new DockPanel();
        DockPanel.SetDock(other, Dock.Top);
        panel.Children.Add(other);
        panel.Children.Add(editor);
        var window = new Window { Width = 600, Height = 500, Content = panel };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var source = editor.FindControl<TextEditor>("SourceEditor")!;
            source.TextArea.Focus();
            source.Select(LongArticle.Length - 20, 10);
            source.TextArea.Caret.BringCaretToView();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var selectedText = source.SelectedText;
            var caretOffset = source.CaretOffset;
            var scroller = FindArticleScroller(editor, false);
            if (!focused)
            {
                other.Focus();
                scroller.ScrollToHome();
            }

            window.Height = 250;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Equal(selectedText, source.SelectedText);
            Assert.Equal(caretOffset, source.CaretOffset);
            if (focused)
            {
                Assert.True(source.TextArea.IsFocused);
                var caret = source.TextArea.Caret.CalculateCaretRectangle();
                var view = source.TextArea.TextView;
                Assert.InRange(caret.Top - view.ScrollOffset.Y, 0, view.Bounds.Height);
                Assert.InRange(caret.Bottom - view.ScrollOffset.Y, 0, view.Bounds.Height);
            }
            else
            {
                Assert.True(other.IsFocused);
                Assert.Equal(0, scroller.Offset.Y);
            }
        }
        finally
        {
            CloseWindow(window, editor);
        }
    }

    [AvaloniaFact]
    public void Repeated_touch_requests_keyboard_without_losing_focus_and_read_only_does_not()
    {
        var editor = new MarkdownEditorControl { MarkdownText = LongArticle };
        var window = new Window { Width = 600, Height = 400, Content = editor };
        var previous = Platform.ShowSoftKeyboard;
        var requests = 0;
        Platform.ShowSoftKeyboard = () => requests++;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var source = editor.FindControl<TextEditor>("SourceEditor")!;
            source.TextArea.Focus();
            var focusLosses = 0;
            source.TextArea.LostFocus += (_, _) => focusLosses++;
            var point = source.TextArea.TranslatePoint(new Point(60, 20), window)!.Value;
            for (var i = 0; i < 2; i++)
            {
                using var touch = window.TouchBegin(point);
                window.TouchEnd(touch, point);
                Assert.True(source.TextArea.IsFocused);
                Assert.Equal(i + 1, requests);
            }

            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Assert.Equal(2, requests);
            using (var scrollTouch = window.TouchBegin(point))
            {
                var end = point + new Vector(0, -80);
                window.TouchMove(scrollTouch, end);
                window.TouchEnd(scrollTouch, end);
            }
            Assert.Equal(2, requests);
            source.IsReadOnly = true;
            using var readOnlyTouch = window.TouchBegin(point);
            window.TouchEnd(readOnlyTouch, point);
            Assert.Equal(2, requests);
            Assert.Equal(0, focusLosses);
        }
        finally
        {
            Platform.ShowSoftKeyboard = previous;
            CloseWindow(window, editor);
        }
    }

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
        var page = new BlogsPage { DataContext = new PostIt.ViewModels.BlogsViewModel { IsEditing = true } };
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
