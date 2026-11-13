#if ANDROID
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using AvaloniaEdit;
using PostIt.Android;
using PostIt.Controls;
using PostIt.ViewModels;
using PostIt.Views.Blogs;

namespace PostIt.Tests;

[Activity(Name = "fr.pschneider.postit.MarkdownEditorTestActivity", Exported = true)]
public sealed class AndroidMarkdownEditorTestActivity : Activity
{
    private string _stage = "startup";
    private ScreenOrientation? _previousOrientation;
    protected override async void OnCreate(Bundle? state)
    {
        base.OnCreate(state);
        var result = Path.Combine(FilesDir!.AbsolutePath, "markdown-editor-test-result.txt");
        File.WriteAllText(result, "RUNNING");
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            await RunAsync(timeout.Token);
            File.WriteAllText(result, "PASS\nbounded viewport above IME,at least three visible lines,visible caret,keyboard reopen,retained focus");
        }
        catch (Exception ex)
        {
            File.WriteAllText(result, $"FAIL at {_stage}\n" + ex);
        }
        finally
        {
            RunOnUiThread(Finish);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(30))
            throw new PlatformNotSupportedException("Android 11 required for keyboard visibility assertions.");
        StartActivity(new Intent(this, typeof(MainActivity)).AddFlags(ActivityFlags.NewTask));
        _stage = "waiting for main activity";
        await WaitAsync(() => MainActivity.IsForeground && (Avalonia.Application.Current as App)?.View is not null, ct);
        var root = ((App)Avalonia.Application.Current!).View!;
        if (Intent?.GetBooleanExtra("landscape", false) == true)
        {
            _previousOrientation = MainActivity.Current!.RequestedOrientation;
            MainActivity.Current.RequestedOrientation = ScreenOrientation.Landscape;
            await WaitAsync(() => TopLevel.GetTopLevel(root)!.ClientSize.Width > TopLevel.GetTopLevel(root)!.ClientSize.Height, ct);
        }
        var navigation = root.FindControl<NavigationPage>("NavRoot")!;
        BlogsPage? page = null;
        TextEditor? source = null;
        var focusLosses = 0;
        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                page = new BlogsPage
                {
                    DataContext = new BlogsViewModel
                    {
                        IsEditing = true,
                        DraftTitle = "Local keyboard test",
                        DraftArticle = string.Join("\n", Enumerable.Range(1, 150).Select(i => $"Line {i}: local keyboard test"))
                    }
                };
                await navigation.PushAsync(page, null);
                source = page.GetLogicalDescendants().OfType<MarkdownEditorControl>().Single()
                    .FindControl<TextEditor>("SourceEditor")!;
            });
            _stage = "waiting for editor layout";
            await WaitAsync(() => source!.TextArea.Bounds.Height > 0, ct);
            await Dispatcher.UIThread.InvokeAsync(() => CheckSystemBars(root));
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!source!.TextArea.Focus())
                    throw new InvalidOperationException("Unable to focus the mounted editor.");
                source.CaretOffset = source.Document.TextLength;
                source.TextArea.Caret.BringCaretToView();
                source.TextArea.LostFocus += (_, _) => focusLosses++;
            });
            _stage = "waiting for native input connection";
            await WaitAsync(() =>
            {
                var activity = MainActivity.Current!;
                var manager = (InputMethodManager)activity.GetSystemService(InputMethodService)!;
                return activity.CurrentFocus is { } view && view.HasWindowFocus
                    && manager.InvokeIsActive(view) && manager.IsAcceptingText;
            }, ct);
            _stage = "opening keyboard";
            await TapAsync(source!, root);
            await WaitAsync(KeyboardVisible, ct);
            await Task.Delay(500, ct);
            await Dispatcher.UIThread.InvokeAsync(() => CheckCaret(source!, root));
            var activity = MainActivity.Current!;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var manager = (InputMethodManager)activity.GetSystemService(InputMethodService)!;
                if (!manager.HideSoftInputFromWindow(activity.CurrentFocus!.WindowToken, HideSoftInputFlags.None))
                    throw new InvalidOperationException("Keyboard hide request rejected.");
            });
            await WaitAsync(() => !KeyboardVisible(), ct);
            await Task.Delay(500, ct);
            await Dispatcher.UIThread.InvokeAsync(() => CheckSystemBars(root));
            await TapAsync(source!, root);
            _stage = "reopening keyboard";
            await WaitAsync(KeyboardVisible, ct);
            await Task.Delay(500, ct);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                CheckCaret(source!, root);
                if (focusLosses != 0) throw new InvalidOperationException($"Editor lost focus {focusLosses} times.");
            });
        }
        finally
        {
            if (page is not null)
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    if (ReferenceEquals(navigation.NavigationStack.LastOrDefault(), page))
                        await navigation.PopAsync();
                });
            if (_previousOrientation is { } orientation && MainActivity.Current is { } main)
                main.RunOnUiThread(() => main.RequestedOrientation = orientation);
        }
    }

    private static bool KeyboardVisible()
        => OperatingSystem.IsAndroidVersionAtLeast(30)
            && MainActivity.Current?.Window?.DecorView?.RootWindowInsets?.IsVisible(WindowInsets.Type.Ime()) == true;

    private static void CheckCaret(TextEditor source, Control root)
    {
        var area = source.TextArea;
        if (!area.IsFocused) throw new InvalidOperationException("Editor is not focused.");
        var rect = area.Caret.CalculateCaretRectangle();
        var view = area.TextView;
        var bottom = rect.Bottom - view.ScrollOffset.Y;
        if (rect.Top - view.ScrollOffset.Y < -1 || bottom > view.Bounds.Height + 1)
            throw new InvalidOperationException($"Caret outside viewport: bottom={bottom}, height={view.Bounds.Height}.");
        var editorBottom = source.TranslatePoint(new Point(0, source.Bounds.Height), root)!.Value.Y;
        if (editorBottom > root.Bounds.Height + 1)
            throw new InvalidOperationException($"Editor outside root: bottom={editorBottom}, height={root.Bounds.Height}.");
        if (view.Bounds.Height < view.DefaultLineHeight * 3)
            throw new InvalidOperationException($"Less than three visible lines: viewport={view.Bounds.Height}, line={view.DefaultLineHeight}.");
        if (OperatingSystem.IsAndroidVersionAtLeast(30) && KeyboardVisible())
        {
            var activity = MainActivity.Current!;
            var decor = activity.Window!.DecorView;
            var keyboardTop = decor.Height - decor.RootWindowInsets!.GetInsets(WindowInsets.Type.Ime()).Bottom;
            var location = new int[2];
            activity.CurrentFocus!.GetLocationInWindow(location);
            var topLevel = TopLevel.GetTopLevel(root)!;
            var bottomInWindow = source.TranslatePoint(new Point(0, source.Bounds.Height), topLevel)!.Value.Y
                * topLevel.RenderScaling + location[1];
            if (bottomInWindow > keyboardTop + 2)
                throw new InvalidOperationException($"Editor hidden by keyboard: bottom={bottomInWindow}, keyboardTop={keyboardTop}.");
        }
        CheckSystemBars(root);
    }

    private static void CheckSystemBars(Control root)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(30))
            throw new PlatformNotSupportedException("Android 11 required for native inset assertions.");
        var activity = MainActivity.Current!;
        var decor = activity.Window!.DecorView;
        var insets = decor.RootWindowInsets!;
        var bars = insets.GetInsets(WindowInsets.Type.SystemBars() | WindowInsets.Type.DisplayCutout());
        var location = new int[2];
        activity.CurrentFocus!.GetLocationInWindow(location);
        var topLevel = TopLevel.GetTopLevel(root)!;
        var navigation = root.FindControl<NavigationPage>("NavRoot")!;
        var top = navigation.TranslatePoint(new Point(), topLevel)!.Value.Y * topLevel.RenderScaling + location[1];
        var bottom = navigation.TranslatePoint(new Point(0, navigation.Bounds.Height), topLevel)!.Value.Y
            * topLevel.RenderScaling + location[1];
        var visibleBottom = decor.Height - bars.Bottom;
        if (KeyboardVisible())
            visibleBottom = Math.Min(visibleBottom, decor.Height - insets.GetInsets(WindowInsets.Type.Ime()).Bottom);
        if (top < bars.Top - 2 || bottom > visibleBottom + 2)
            throw new InvalidOperationException($"Controls overlap system bars: top={top}, safeTop={bars.Top}, bottom={bottom}, safeBottom={visibleBottom}.");
        if (navigation.NavigationStack.LastOrDefault() is BlogsPage page)
        {
            var bar = NavigationPage.GetTopCommandBar(page)!;
            var shouldShowTitle = !KeyboardVisible();
            if (!bar.IsVisible || bar.Bounds.Height <= 0
                || page.FindControl<TextBox>("DraftTitleTextBox")?.IsVisible != shouldShowTitle)
                throw new InvalidOperationException("Blog controls are unavailable.");
            if (page.FindControl<TextBox>("DraftTitleTextBox")?.Text != "Local keyboard test")
                throw new InvalidOperationException("The title changed during keyboard layout transitions.");
        }
    }

    private static async Task TapAsync(TextEditor source, Control root)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var activity = MainActivity.Current!;
            var point = source.TextArea.TranslatePoint(new Point(80, source.TextArea.Bounds.Height / 2), root)!.Value;
            var location = new int[2];
            activity.CurrentFocus!.GetLocationInWindow(location);
            var scale = TopLevel.GetTopLevel(root)!.RenderScaling;
            var x = (float)(point.X * scale + location[0]);
            var y = (float)(point.Y * scale + location[1]);
            var now = SystemClock.UptimeMillis();
            using var properties = new MotionEvent.PointerProperties { Id = 0, ToolType = MotionEventToolType.Finger };
            using var coordinates = new MotionEvent.PointerCoords { X = x, Y = y, Pressure = 1, Size = 1 };
            using var down = MotionEvent.Obtain(now, now, MotionEventActions.Down, 1,
                new[] { properties }, new[] { coordinates }, 0, 0, 1, 1, 0, 0,
                InputSourceType.Touchscreen, 0)!;
            using var up = MotionEvent.Obtain(now, now + 50, MotionEventActions.Up, 1,
                new[] { properties }, new[] { coordinates }, 0, 0, 1, 1, 0, 0,
                InputSourceType.Touchscreen, 0)!;
            activity.DispatchTouchEvent(down);
            activity.DispatchTouchEvent(up);
        });
    }

    private static async Task WaitAsync(Func<bool> condition, CancellationToken ct)
    {
        while (!await Dispatcher.UIThread.InvokeAsync(condition))
            await Task.Delay(50, ct);
    }
}
#endif
