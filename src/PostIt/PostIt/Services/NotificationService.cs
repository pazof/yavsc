using System;
using PostIt.Models;

namespace PostIt.Services;

public sealed class NotificationService
{
    public static NotificationService Instance { get; } = new();

    public event EventHandler<NotificationRequest>? NotificationRequested;

    private NotificationService()
    {
    }

    public void Show(string message, StatusSeverity severity = StatusSeverity.Info, string? actionLabel = null, Action? action = null)
        => NotificationRequested?.Invoke(this, new NotificationRequest(message, severity, actionLabel, action));

    public void Hide()
    {
        NotificationRequested?.Invoke(this, new NotificationRequest(string.Empty, StatusSeverity.Info));
    }
}

public sealed record NotificationRequest(string Message, StatusSeverity Severity, string? ActionLabel = null, Action? Action = null);
