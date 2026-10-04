using System;
using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit.Document;

namespace PostIt.Controls;

public partial class MarkdownEditorControl : UserControl
{
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
