using Microsoft.Extensions.DependencyInjection;
using PostIt.Models;
using PostIt.Services;

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
        var statusNotice = severity switch
        {
            StatusSeverity.Error => StatusNotice.Error(normalizedMessage),
            StatusSeverity.Warning => StatusNotice.Warning(normalizedMessage),
            _ => StatusNotice.Info(normalizedMessage),
        };

        viewModel.StatusMessage = normalizedMessage;
        viewModel.ActionStatus = statusNotice;

        if (App.Current is App app && app.ServiceProvider is { } serviceProvider)
        {
            var notificationService = serviceProvider.GetRequiredService<NotificationService>();
            notificationService.Post(normalizedMessage, severity);
        }
    }
}
