using System;

public class ErrorViewModel
{
    public string RequestId { get; set; } = String.Empty;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
