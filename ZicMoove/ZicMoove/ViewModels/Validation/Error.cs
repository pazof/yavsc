namespace ZicMoove.ViewModels.Validation
{
    public class InputError
    {
        public InputError(string errorMessage, ErrorSeverity severity)
        {
            Text = errorMessage;
            Severity = severity;
        }
        public string Text { get; set; }
        public ErrorSeverity Severity { get; set; }
    }
}
