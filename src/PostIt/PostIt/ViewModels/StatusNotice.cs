namespace PostIt.ViewModels;

public enum StatusSeverity
{
    Info,
    Warning,
    Error
}

public sealed class StatusNotice
{
    public string Message { get; }
    public StatusSeverity Severity { get; }
    public string Glyph { get; }
    public string Background { get; }
    public string BorderBrush { get; }
    public string Foreground { get; }

    private StatusNotice(string message, StatusSeverity severity)
    {
        Message = string.IsNullOrWhiteSpace(message) ? "Pret." : message;
        Severity = severity;

        (Glyph, Background, BorderBrush, Foreground) = severity switch
        {
            StatusSeverity.Error => ("!", "#FDECEA", "#C62828", "#7F1D1D"),
            StatusSeverity.Warning => ("~", "#FFF8E1", "#E6A700", "#7C4A03"),
            _ => ("i", "#E8F0FE", "#5B8DEF", "#1E3A8A"),
        };
    }

    public static StatusNotice Info(string message) => new(message, StatusSeverity.Info);
    public static StatusNotice Warning(string message) => new(message, StatusSeverity.Warning);
    public static StatusNotice Error(string message) => new(message, StatusSeverity.Error);
}
