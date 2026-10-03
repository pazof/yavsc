using System.Windows.Input;
using PostIt.Models;

namespace PostIt.ViewModels;

public sealed class StatusNotice
{
    public string Message { get; }
    public StatusSeverity Severity { get; }
    public string Glyph { get; }
    public string Background { get; }
    public string BorderBrush { get; }
    public string Foreground { get; }

    public bool HasCheckCommand { get; }

    public ICommand? CheckCommand { get; }

    public string CheckCommandLabel { get; }

    public StatusNotice(string message, StatusSeverity severity, ICommand? checkCommand)
    {
        Message = string.IsNullOrWhiteSpace(message) ? "Pret." : message;
        Severity = severity;

        (Glyph, Background, BorderBrush, Foreground) = severity switch
        {
            StatusSeverity.Error => ("!", "#7F1D1D", "#C62828", "#e1f0f6"),
            StatusSeverity.Warning => ("~", "#7C4A03", "#E6A700", "#eaeaea"),
            _ => ("i", "#E8F0FE", "#5B8DEF", "#1E3A8A"),
        };
        HasCheckCommand = checkCommand != null;
        CheckCommand = checkCommand;
        CheckCommandLabel = HasCheckCommand ? "Check" :
            checkCommand == null ?
                string.Empty :
                checkCommand is IWithLabel labeled ? labeled.Label : string.Empty;
    }

    public static StatusNotice Info(string message, ICommand? checkCommand = null) => new(message, StatusSeverity.Info, checkCommand);
    public static StatusNotice Warning(string message, ICommand? checkCommand = null) => new(message, StatusSeverity.Warning, checkCommand);
    public static StatusNotice Error(string message, ICommand? checkCommand = null) => new(message, StatusSeverity.Error, checkCommand);
}
