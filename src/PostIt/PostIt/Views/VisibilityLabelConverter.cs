using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Yavsc.Blogspot;

namespace PostIt.Views;

/// <summary>
/// Converts a <see cref="Visibility"/> enum value to a user-
/// facing French label. Used by <c>MainPage.axaml</c> to render
/// the visibility ComboBox without exposing the raw enum name
/// ("Private" / "Public") to the end user.
///
/// <para>Bidirectional: <c>ConvertBack</c> returns the value
/// unchanged, so the ComboBox can drive the bound
/// <c>DraftVisibility</c> property directly through the same
/// converter — the ComboBox just happens to use
/// <c>SelectedItem</c> binding so ConvertBack is never
/// invoked. The symmetry is kept for completeness in case a
/// future XAML needs to bind via <c>Text</c>.</para>
/// </summary>
public sealed class VisibilityLabelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility v)
        {
            return v switch
            {
                Visibility.Private => "Privé",
                Visibility.Public => "Public",
                _ => v.ToString(),
            };
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Reverse mapping: user input is unlikely to be the
        // raw English enum name (the ComboBox shows French
        // labels), so ConvertBack falls back to Private on any
        // unrecognised input. The ComboBox uses SelectedItem
        // binding so this path is never actually taken today.
        if (value is string s)
        {
            return s switch
            {
                "Privé" => Visibility.Private,
                "Public" => Visibility.Public,
                _ => Visibility.Private,
            };
        }
        return value;
    }
}
