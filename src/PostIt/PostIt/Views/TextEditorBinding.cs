using Avalonia;
using Avalonia.Data;
using AvaloniaEdit;

namespace PostIt.Views;

public sealed class TextEditorBinding
{
    public static readonly AttachedProperty<string?> TextProperty =
        AvaloniaProperty.RegisterAttached<TextEditorBinding, TextEditor, string?>(
            "Text",
            defaultBindingMode: BindingMode.TwoWay);

    private static readonly AttachedProperty<bool> IsTextChangeSubscribedProperty =
        AvaloniaProperty.RegisterAttached<TextEditorBinding, TextEditor, bool>("IsTextChangeSubscribed");

    static TextEditorBinding()
    {
        TextProperty.Changed.AddClassHandler<TextEditor>((editor, args) =>
        {
            EnsureTextChangeSubscribed(editor);

            var text = args.GetNewValue<string?>() ?? string.Empty;
            if (editor.Text != text)
            {
                editor.Text = text;
            }
        });
    }

    public static string? GetText(TextEditor editor) => editor.GetValue(TextProperty);

    public static void SetText(TextEditor editor, string? value) => editor.SetValue(TextProperty, value);

    private static void EnsureTextChangeSubscribed(TextEditor editor)
    {
        if (editor.GetValue(IsTextChangeSubscribedProperty))
        {
            return;
        }

        editor.SetValue(IsTextChangeSubscribedProperty, true);
        editor.TextChanged += (_, _) =>
        {
            if (editor.Text != GetText(editor))
            {
                SetText(editor, editor.Text);
            }
        };
    }
}
