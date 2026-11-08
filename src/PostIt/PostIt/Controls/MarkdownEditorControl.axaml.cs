using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit.Document;
using PostIt.Services;

namespace PostIt.Controls;

public partial class MarkdownEditorControl : UserControl
{
    private IPointer? _touchPointer;
    private Point _touchStart;
    public static readonly StyledProperty<string?> MarkdownTextProperty =
        AvaloniaProperty.Register<MarkdownEditorControl, string?>(nameof(MarkdownText));

    public static readonly StyledProperty<TextDocument?> DocumentProperty =
        AvaloniaProperty.Register<MarkdownEditorControl, TextDocument?>(nameof(Document));

    public static readonly StyledProperty<bool> IsPreviewVisibleProperty =
        AvaloniaProperty.Register<MarkdownEditorControl, bool>(nameof(IsPreviewVisible), false);

    public string? MarkdownText
    {
        get => GetValue(MarkdownTextProperty);
        set => SetValue(MarkdownTextProperty, value);
    }

    public TextDocument? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public bool IsPreviewVisible
    {
        get => GetValue(IsPreviewVisibleProperty);
        set => SetValue(IsPreviewVisibleProperty, value);
    }

    public MarkdownEditorControl()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
        SourceEditor.TextArea.SizeChanged += (_, _) =>
            Dispatcher.UIThread.Post(KeepCaretVisible, DispatcherPriority.Loaded);
        SourceEditor.TextArea.AddHandler(InputElement.PointerPressedEvent, OnEditorPointerPressed,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        SourceEditor.TextArea.AddHandler(InputElement.PointerReleasedEvent, OnEditorPointerReleased,
            RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void KeepCaretVisible()
    {
        if (SourceEditor.TextArea.IsFocused && SourceEditor.IsEffectivelyVisible
            && SourceEditor.TextArea.Bounds.Height > 0)
            SourceEditor.TextArea.Caret.BringCaretToView();
    }

    private void OnEditorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Touch)
            return;
        _touchPointer = e.Pointer;
        _touchStart = e.GetPosition(SourceEditor.TextArea);
    }

    private void OnEditorPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!ReferenceEquals(_touchPointer, e.Pointer))
            return;
        _touchPointer = null;
        var point = e.GetPosition(SourceEditor.TextArea);
        var settings = this.GetPlatformSettings()
            ?? throw new InvalidOperationException("The editor has no platform input settings.");
        var tapSize = settings.GetTapSize(PointerType.Touch);
        // AvaloniaEdit changes pointer capture during selection, so its taps may not raise Tapped.
        if (new Rect(_touchStart, default(Size)).Inflate(new Thickness(tapSize.Width, tapSize.Height)).Contains(point)
            && SourceEditor.TextArea.IsFocused
            && !SourceEditor.IsReadOnly
            && new Rect(SourceEditor.TextArea.Bounds.Size).Contains(point))
            Platform.ShowSoftKeyboard?.Invoke();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MarkdownTextProperty)
        {
            var text = e.NewValue as string ?? string.Empty;
            if (Document is null || !string.Equals(Document.Text, text, StringComparison.Ordinal))
            {
                Document = new TextDocument(text);
            }
        }
        else if (e.Property == DocumentProperty)
        {
            var newDocument = e.NewValue as TextDocument;
            var text = newDocument?.Text ?? string.Empty;
            if (!string.Equals(MarkdownText, text, StringComparison.Ordinal))
            {
                MarkdownText = text;
            }
        }
    }
}
