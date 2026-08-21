using PostIt.Views;

namespace PostIt.Tests;

internal class TestAppContext
{
    public MainWindow? Window {get; set; }
    public CirclesPage? page {get; set; }
    public AddCircleMemberDialog? dialog { get; set; }
    public App? App { get; internal set; }
}
