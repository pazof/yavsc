namespace PostIt.ViewModels;

public interface IActionStatusViewModel
{
    string StatusMessage { get; set; }
    StatusNotice ActionStatus { get; set; }
}

public static class ActionStatusViewModelExtensions
{
    public static void SetInfoStatus(this IActionStatusViewModel viewModel, string message)
        => viewModel.SetStatus(message, StatusSeverity.Info);

    public static void SetWarningStatus(this IActionStatusViewModel viewModel, string message)
        => viewModel.SetStatus(message, StatusSeverity.Warning);

    public static void SetErrorStatus(this IActionStatusViewModel viewModel, string message)
        => viewModel.SetStatus(message, StatusSeverity.Error);

    public static void SetStatus(this IActionStatusViewModel viewModel, string message, StatusSeverity severity)
    {
        var normalizedMessage = string.IsNullOrWhiteSpace(message) ? "Pret." : message.Trim();

        viewModel.StatusMessage = normalizedMessage;
        viewModel.ActionStatus = severity switch
        {
            StatusSeverity.Error => StatusNotice.Error(normalizedMessage),
            StatusSeverity.Warning => StatusNotice.Warning(normalizedMessage),
            _ => StatusNotice.Info(normalizedMessage),
        };
    }
}